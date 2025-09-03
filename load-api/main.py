import os
import re
import time
import math
import glob
import asyncio
from pathlib import Path
from functools import lru_cache
from concurrent.futures import ThreadPoolExecutor
from statistics import mean, median
from typing import List

from fastapi import FastAPI, HTTPException, Query, UploadFile, File, Body
from fastapi.responses import JSONResponse, HTMLResponse
from psycopg_pool import ConnectionPool

# -----------------------------
# Конфиг через переменные окружения
# -----------------------------
DB_DSN = os.getenv(
    "DB_DSN",
    "postgresql://vtb_hack:j~)S5X3rQwL)rW@89.23.118.214:5432/vtb_hack",  # поменяйте под себя
)
SCRIPTS_DIR = Path(os.getenv("SCRIPTS_DIR", "./sql")).resolve()
MAX_WORKERS = int(os.getenv("MAX_WORKERS", str(max(4, (os.cpu_count() or 2) * 4))))

# Пул соединений (autocommit по умолчанию включён, чтобы DDL/многооператорные скрипты отрабатывали без явного commit)
def _configure(conn):
    conn.autocommit = True

pool = ConnectionPool(DB_DSN, min_size=1, max_size=MAX_WORKERS, configure=_configure)

# Глобальный пул потоков для параллельного запуска
executor = ThreadPoolExecutor(max_workers=MAX_WORKERS)

app = FastAPI(title="SQL Runner", version="1.0")

NUMERIC_SQL_RE = re.compile(r"^(\d+)\.sql$", re.IGNORECASE)


# -----------------------------
# Утилиты
# -----------------------------
def _require_scripts_dir():
    if not SCRIPTS_DIR.exists():
        raise HTTPException(status_code=500, detail=f"Scripts dir not found: {SCRIPTS_DIR}")
    if not SCRIPTS_DIR.is_dir():
        raise HTTPException(status_code=500, detail=f"Scripts path is not a dir: {SCRIPTS_DIR}")


def _ensure_scripts_dir():
    """
    Создать каталог sql при необходимости.
    """
    if not SCRIPTS_DIR.exists():
        SCRIPTS_DIR.mkdir(parents=True, exist_ok=True)
    if not SCRIPTS_DIR.is_dir():
        raise HTTPException(status_code=500, detail=f"Scripts path is not a dir: {SCRIPTS_DIR}")


def _list_numeric_sql_paths() -> list[Path]:
    """
    Вернуть пути к *.sql, у которых имя — число, отсортированные по возрастанию числа.
    """
    _ensure_scripts_dir()
    pairs: list[tuple[int, Path]] = []
    for p in SCRIPTS_DIR.glob("*.sql"):
        m = NUMERIC_SQL_RE.match(p.name)
        if m:
            pairs.append((int(m.group(1)), p))
    pairs.sort(key=lambda t: t[0])
    return [p for _, p in pairs]


def _renumber_scripts() -> None:
    """
    Обеспечить непрерывную нумерацию файлов 1..n без дырок.
    Выполняется в две фазы, чтобы избежать конфликтов имён при переименовании.
    """
    files = _list_numeric_sql_paths()
    if not files:
        return

    # Фаза 1: временные имена, чтобы избежать конфликтов
    temp_paths: list[Path] = []
    for idx, path in enumerate(files, start=1):
        if path.name != f"{idx}.sql":
            tmp = path.with_name(f".__tmp_{idx}__.sql")
            if tmp.exists():
                tmp.unlink()
            path.rename(tmp)
            temp_paths.append(tmp)
        else:
            # Уже корректное имя — оставляем как есть, но фиксируем как не требующий второй фазы
            pass

    # Фаза 2: пронумеровать строго как 1.sql .. n.sql
    # Сначала заново собрать список: темпы + уже правильные имена
    files_after = _list_numeric_sql_paths() + temp_paths
    # Создать упорядоченный список на основе чисел в имени, если это .__tmp_*.sql — берём число из шаблона
    ordered: list[Path] = []
    for p in files_after:
        m_num = NUMERIC_SQL_RE.match(p.name)
        if m_num:
            ordered.append(p)
    # Плюс временные
    ordered += temp_paths

    # Итоговая перенумерация от 1
    current = _list_numeric_sql_paths()  # пересобрать список корректных на данный момент
    # Начнём с чистого упорядоченного множества путей к файлам (темпы в конце тоже учтём)
    paths_to_process = []
    seen = set()
    for p in current + temp_paths:
        if p.exists() and p not in seen:
            paths_to_process.append(p)
            seen.add(p)

    for idx, p in enumerate(paths_to_process, start=1):
        dest = SCRIPTS_DIR / f"{idx}.sql"
        if p == dest:
            continue
        if dest.exists():
            dest.unlink()
        p.rename(dest)

    # Сбросить кэш чтения, так как имена/содержимое могли измениться
    _load_script.cache_clear()


def _discover_script_numbers() -> list[int]:
    _require_scripts_dir()
    nums = []
    for p in SCRIPTS_DIR.glob("*.sql"):
        m = NUMERIC_SQL_RE.match(p.name)
        if m:
            nums.append(int(m.group(1)))
    return sorted(nums)


@lru_cache(maxsize=256)
def _load_script(n: int) -> str:
    _require_scripts_dir()
    path = SCRIPTS_DIR / f"{n}.sql"
    if not path.exists():
        raise HTTPException(status_code=404, detail=f"Script {n}.sql not found")
    return path.read_text(encoding="utf-8")


def _percentile(values: list[float], q: float) -> float:
    if not values:
        return 0.0
    s = sorted(values)
    idx = max(0, min(len(s) - 1, math.ceil(q * len(s)) - 1))
    return s[idx]


def _exec_script_once(n: int, transactional: bool = False) -> dict:
    """
    Блокирующая функция для потока: выполняет n.sql и измеряет время.
    Если transactional=True — оборачивает выполнение в одну транзакцию.
    """
    sql = _load_script(n)
    started_ns = time.perf_counter_ns()
    with pool.connection() as conn:
        orig_autocommit = conn.autocommit
        try:
            conn.autocommit = not transactional
            with conn.cursor() as cur:
                cur.execute(sql)  # допускает много операторов; результат не извлекаем
            if transactional:
                conn.commit()
        finally:
            # вернуть прежний режим перед возвратом коннекта в пул
            conn.autocommit = orig_autocommit
    dur_ms = (time.perf_counter_ns() - started_ns) / 1e6
    return {"script": f"{n}.sql", "n": n, "duration_ms": round(dur_ms, 3)}


# -----------------------------
# Роуты
# -----------------------------
@app.get("/request/{n}")
async def run_single(
    n: int,
    transactional: bool = Query(
        False,
        description="Если true — выполнить скрипт в одной транзакции (commit в конце). "
                    "По умолчанию autocommit."
    )
):
    """
    Выполнить один скрипт n.sql и вернуть время работы.
    """
    loop = asyncio.get_running_loop()
    try:
        result = await loop.run_in_executor(executor, _exec_script_once, n, transactional)
        return JSONResponse(
            {
                "status": "ok",
                "result": result,
            }
        )
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Execution error: {e}")


@app.get("/requests/{count}")
async def run_all_many(
    count: int,
    transactional: bool = Query(
        False,
        description="Если true — каждый запуск скрипта идёт в одной транзакции."
    ),
    max_workers: int | None = Query(
        None,
        ge=1,
        le=1024,
        description="Переопределить число потоков для этого запроса (по умолчанию MAX_WORKERS)."
    )
):
    """
    Для КАЖДОГО скрипта в каталоге *.sql выполнить его count раз в параллельных потоках.
    Вернуть сводку по времени (min/median/avg/p95/max) и детальные замеры.
    """
    scripts = _discover_script_numbers()
    if not scripts:
        raise HTTPException(status_code=404, detail="No *.sql scripts found")

    # Локальный пул можно создать с иным числом потоков для этой нагрузки
    local_executor = executor if max_workers is None else ThreadPoolExecutor(max_workers=max_workers)

    tasks = []
    loop = asyncio.get_running_loop()

    started_ns = time.perf_counter_ns()
    try:
        for n in scripts:
            for _ in range(count):
                # Каждый запуск — отдельная задача в пуле потоков
                tasks.append(loop.run_in_executor(local_executor, _exec_script_once, n, transactional))

        # Дождаться всех результатов (если что-то упадёт — сообщение будет в HTTPException ниже)
        results = await asyncio.gather(*tasks, return_exceptions=True)
    finally:
        if local_executor is not executor:
            local_executor.shutdown(wait=True)

    # Разобрать результаты/ошибки
    by_script: dict[int, dict] = {}
    error_count = 0
    for res in results:
        if isinstance(res, Exception):
            error_count += 1
            continue
        n = res["n"]
        by_script.setdefault(n, {"script": f"{n}.sql", "runs_ms": []})
        by_script[n]["runs_ms"].append(res["duration_ms"])

    # Посчитать метрики
    for n, data in by_script.items():
        runs = data["runs_ms"]
        data["count"] = len(runs)
        data["stats"] = {
            "min_ms": round(min(runs), 3) if runs else 0.0,
            "median_ms": round(median(runs), 3) if runs else 0.0,
            "avg_ms": round(mean(runs), 3) if runs else 0.0,
            "p95_ms": round(_percentile(runs, 0.95), 3) if runs else 0.0,
            "max_ms": round(max(runs), 3) if runs else 0.0,
        }

    total_ms = (time.perf_counter_ns() - started_ns) / 1e6
    response = {
        "status": "ok" if error_count == 0 else "partial",
        "config": {
            "scripts_dir": str(SCRIPTS_DIR),
            "scripts": [f"{n}.sql" for n in scripts],
            "count_per_script": count,
            "transactional": transactional,
            "max_workers_used": max_workers or MAX_WORKERS,
            "db_dsn": DB_DSN,
        },
        "summary": {
            "total_tasks": len(scripts) * count,
            "completed": sum(d["count"] for d in by_script.values()),
            "errors": error_count,
            "wall_time_ms": round(total_ms, 3),
        },
        "by_script": by_script,
    }
    return JSONResponse(response)


@app.get("/health")
def health():
    return {"status": "ok"}


# -----------------------------
# Файловый менеджмент для каталога sql (загрузка/удаление/чтение/редактирование) + UI /editor
# -----------------------------

@app.get("/api/scripts")
def list_scripts():
    """
    Список файлов 1..n в каталоге sql.
    """
    _ensure_scripts_dir()
    _renumber_scripts()  # на всякий случай поддерживаем непрерывность
    items = []
    for p in _list_numeric_sql_paths():
        m = NUMERIC_SQL_RE.match(p.name)
        n = int(m.group(1)) if m else None
        items.append({
            "n": n,
            "filename": p.name,
            "size": p.stat().st_size,
        })
    return {"status": "ok", "items": items}


@app.get("/api/scripts/{n}")
def get_script_content(n: int):
    """
    Получить содержимое файла n.sql
    """
    content = _load_script(n)
    return {"status": "ok", "n": n, "filename": f"{n}.sql", "content": content}


@app.put("/api/scripts/{n}")
def update_script_content(n: int, payload: dict = Body(...)):
    """
    Перезаписать содержимое файла n.sql. Тело: {"content": "..."}
    """
    _ensure_scripts_dir()
    path = SCRIPTS_DIR / f"{n}.sql"
    if not path.exists():
        raise HTTPException(status_code=404, detail=f"Script {n}.sql not found")
    content = payload.get("content")
    if content is None:
        raise HTTPException(status_code=400, detail="Missing 'content' field")
    path.write_text(str(content), encoding="utf-8")
    _load_script.cache_clear()
    return {"status": "ok", "n": n, "filename": f"{n}.sql"}


@app.post("/api/scripts/upload")
async def upload_scripts(files: List[UploadFile] = File(...)):
    """
    Загрузка одного или нескольких файлов. Игнорируем исходные имена —
    сохраняем как next.sql, где next = (кол-во существующих файлов) + 1,
    предварительно выравнивая нумерацию до 1..n.
    """
    _ensure_scripts_dir()
    _renumber_scripts()
    saved = []
    for f in files:
        try:
            data = await f.read()
        finally:
            await f.close()
        existing = _list_numeric_sql_paths()
        next_n = len(existing) + 1
        dest = SCRIPTS_DIR / f"{next_n}.sql"
        dest.write_bytes(data)
        saved.append({"n": next_n, "filename": dest.name, "size": len(data)})
    _load_script.cache_clear()
    return {"status": "ok", "saved": saved}


@app.delete("/api/scripts/{n}")
def delete_script(n: int):
    """
    Удалить n.sql и перенумеровать оставшиеся в 1..(n-1).
    """
    _ensure_scripts_dir()
    path = SCRIPTS_DIR / f"{n}.sql"
    if not path.exists():
        raise HTTPException(status_code=404, detail=f"Script {n}.sql not found")
    path.unlink()
    _renumber_scripts()
    _load_script.cache_clear()
    return {"status": "ok"}


@app.get("/editor")
def editor_page():
    """
    Простой UI-редактор, доступный по /editor, для просмотра/редактирования/загрузки/удаления SQL-скриптов.
    """
    html = """
<!DOCTYPE html>
<html lang=\"ru\">
<head>
  <meta charset=\"UTF-8\" />
  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />
  <title>SQL Editor</title>
  <style>
    body { font-family: system-ui, -apple-system, Segoe UI, Roboto, Arial, sans-serif; margin: 0; background: #0f172a; color: #e2e8f0; }
    header { padding: 12px 16px; background: #111827; border-bottom: 1px solid #1f2937; display:flex; align-items:center; gap:12px; }
    header h1 { margin: 0; font-size: 16px; font-weight: 600; }
    .container { display: grid; grid-template-columns: 300px 1fr; height: calc(100vh - 50px); }
    .sidebar { border-right: 1px solid #1f2937; overflow-y: auto; }
    .toolbar { padding: 12px; display:flex; flex-direction: column; gap: 8px; border-bottom: 1px solid #1f2937; }
    .toolbar button, .toolbar label { background:#1f2937; color:#e5e7eb; border:1px solid #374151; padding:8px 10px; border-radius:6px; cursor:pointer; font-size: 13px; }
    .toolbar input[type=file] { display:none; }
    .file-list { padding: 8px; }
    .file-item { display:flex; justify-content: space-between; align-items:center; padding: 8px 10px; margin-bottom:6px; background:#0b1220; border:1px solid #1f2937; border-radius:6px; cursor:pointer; }
    .file-item.active { outline: 2px solid #3b82f6; }
    .file-actions { display:flex; gap:6px; }
    .main { display: grid; grid-template-rows: 1fr auto; }
    textarea { width: 100%; height: 100%; resize: none; border: none; outline: none; padding: 14px; box-sizing: border-box; font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, \"Liberation Mono\", monospace; font-size: 13px; color:#e5e7eb; background:#0b1220; }
    .bottom-bar { display:flex; gap:8px; padding: 10px; border-top:1px solid #1f2937; background:#111827; }
    .primary { background:#2563eb; border-color:#1d4ed8; }
    .danger { background:#dc2626; border-color:#b91c1c; }
    .muted { background:#374151; border-color:#4b5563; }
    .status { margin-left:auto; opacity:0.8; }
  </style>
  <link rel=\"icon\" href=\"data:,\" />
  <meta http-equiv=\"Content-Security-Policy\" content=\"default-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:;\" />
</head>
<body>
  <header>
    <h1>SQL Editor</h1>
    <span id=\"status\"></span>
  </header>
  <div class=\"container\">
    <aside class=\"sidebar\">
      <div class=\"toolbar\">
        <label>
          Загрузить файл(ы)
          <input id=\"fileInput\" type=\"file\" multiple />
        </label>
        <button id=\"createBtn\" class=\"muted\">Создать пустой скрипт</button>
        <button id=\"refreshBtn\" class=\"muted\">Обновить список</button>
      </div>
      <div id=\"files\" class=\"file-list\"></div>
    </aside>
    <main class=\"main\">
      <textarea id=\"editor\" spellcheck=\"false\" placeholder=\"Выберите файл слева...\"></textarea>
      <div class=\"bottom-bar\">
        <button id=\"saveBtn\" class=\"primary\">Сохранить</button>
        <button id=\"deleteBtn\" class=\"danger\">Удалить</button>
        <span class=\"status\" id=\"opStatus\"></span>
      </div>
    </main>
  </div>
  <script>
    const el = (sel) => document.querySelector(sel);
    const filesEl = el('#files');
    const editorEl = el('#editor');
    const opStatusEl = el('#opStatus');
    const statusEl = el('#status');
    let currentN = null;

    const api = {
      async list() {
        const r = await fetch('/api/scripts');
        if (!r.ok) throw new Error('list failed');
        return r.json();
      },
      async get(n) {
        const r = await fetch(`/api/scripts/${n}`);
        if (!r.ok) throw new Error('get failed');
        return r.json();
      },
      async save(n, content) {
        const r = await fetch(`/api/scripts/${n}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ content }) });
        if (!r.ok) throw new Error('save failed');
        return r.json();
      },
      async upload(fileList) {
        const fd = new FormData();
        for (const f of fileList) fd.append('files', f);
        const r = await fetch('/api/scripts/upload', { method: 'POST', body: fd });
        if (!r.ok) throw new Error('upload failed');
        return r.json();
      },
      async del(n) {
        const r = await fetch(`/api/scripts/${n}`, { method: 'DELETE' });
        if (!r.ok) throw new Error('delete failed');
        return r.json();
      },
      async createEmpty() {
        // загрузить пустой файл как новый скрипт
        const blob = new Blob([''], { type: 'text/plain' });
        const file = new File([blob], 'empty.sql');
        return this.upload([file]);
      }
    };

    function setStatus(text, transient = true) {
      opStatusEl.textContent = text || '';
      if (transient && text) setTimeout(() => { if (opStatusEl.textContent === text) opStatusEl.textContent = ''; }, 2000);
    }

    function renderList(items) {
      filesEl.innerHTML = '';
      for (const it of items) {
        const div = document.createElement('div');
        div.className = 'file-item' + (currentN === it.n ? ' active' : '');
        div.innerHTML = `<span>${it.filename}</span><span class=\"file-actions\"></span>`;
        div.addEventListener('click', async () => {
          try {
            const data = await api.get(it.n);
            currentN = it.n;
            editorEl.value = data.content || '';
            renderList(items.map(x => ({...x})));
            setStatus(`Открыт ${it.filename}`);
          } catch (e) { setStatus('Ошибка открытия'); }
        });
        filesEl.appendChild(div);
      }
    }

    async function refresh() {
      try {
        statusEl.textContent = '';
        const data = await api.list();
        renderList(data.items);
        // если после операций номера могли сместиться — пересчитать currentN
        if (currentN) {
          const exists = data.items.some(i => i.n === currentN);
          if (!exists) { currentN = null; editorEl.value = ''; }
        }
      } catch (e) {
        statusEl.textContent = 'Ошибка загрузки списка';
      }
    }

    // Wire UI
    el('#refreshBtn').addEventListener('click', refresh);
    el('#fileInput').addEventListener('change', async (ev) => {
      if (!ev.target.files?.length) return;
      try { await api.upload(ev.target.files); setStatus('Загружено'); await refresh(); }
      catch (e) { setStatus('Ошибка загрузки'); }
      finally { ev.target.value = ''; }
    });
    el('#saveBtn').addEventListener('click', async () => {
      if (!currentN) { setStatus('Нет выбранного файла'); return; }
      try { await api.save(currentN, editorEl.value); setStatus('Сохранено'); }
      catch (e) { setStatus('Ошибка сохранения'); }
    });
    el('#deleteBtn').addEventListener('click', async () => {
      if (!currentN) { setStatus('Нет выбранного файла'); return; }
      if (!confirm('Удалить файл?')) return;
      try { await api.del(currentN); currentN = null; editorEl.value = ''; await refresh(); setStatus('Удалено'); }
      catch (e) { setStatus('Ошибка удаления'); }
    });
    el('#createBtn').addEventListener('click', async () => {
      try { await api.createEmpty(); await refresh(); setStatus('Создан'); }
      catch (e) { setStatus('Ошибка создания'); }
    });

    refresh();
  </script>
</body>
</html>
    """
    return HTMLResponse(content=html)

# -----------------------------
# Запуск: uvicorn server:app --reload
# -----------------------------