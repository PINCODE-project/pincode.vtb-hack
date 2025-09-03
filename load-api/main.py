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

from fastapi import FastAPI, HTTPException, Query
from fastapi.responses import JSONResponse
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
# Запуск: uvicorn server:app --reload
# -----------------------------