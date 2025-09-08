using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Extensions;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;


/// <summary>
/// Продвинутый анализатор pg_stat_statements.
/// - scoped сервис (инжектится DataContext)
/// - анализирует все записи pg_stat_statements (без orderBy)
/// - вычисляет score (0..100), классифицирует критичность и генерирует рекомендации на русском
/// - опционально выполняет EXPLAIN (ANALYZE, BUFFERS, FORMAT JSON) для топ-запросов (includeExplain=true)
/// </summary>
public class PgStatAnalyzerService : IPgStatAnalyzerService
{
    private readonly DataContext _db;
    private readonly ILogger<PgStatAnalyzerService> _log;
    private readonly DataContext _dbContext;

    // кеш отображения колонок pg_stat_statements (TTL)
    private Dictionary<string, string>? _columnMap;
    private DateTime _columnMapLoadedAt = DateTime.MinValue;
    private readonly TimeSpan _columnMapTtl = TimeSpan.FromMinutes(5);

    public PgStatAnalyzerService(DataContext db, ILogger<PgStatAnalyzerService> log,
        DataContext dbContext)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _dbContext = dbContext;
    }

    public void Dispose()
    {
        // DbContext управляется DI, ничего не делаем
    }

    /// <summary>
    /// Анализирует все записи pg_stat_statements и возвращает top N проблемных запросов.
    /// - limit: сколько результатов вернуть (по score)
    /// - includeExplain: если true — попробует выполнить EXPLAIN (ANALYZE, BUFFERS, FORMAT JSON) для каждого топ-запроса (опасно в prod)
    /// </summary>
    public async Task<AnalysisReportAdvanced> AnalyzeTopAsync(Guid dbConnectionId, int limit = 50, bool includeExplain = false,
        CancellationToken cancellationToken = default)
    {
        if (limit <= 0) limit = 50;
        
        var map = await EnsureColumnMapAsync(dbConnectionId, cancellationToken);
        if (map == null || map.Count == 0)
            throw new InvalidOperationException(
                "pg_stat_statements недоступен или не удалось определить колонки. Убедитесь, что расширение установлено.");

        // Получаем все записи pg_stat_statements (без ORDER BY / LIMIT) — анализ "по всем сразу".
        // Важно: на очень больших инсталляциях это может вернуть много строк — мониторьте потребление памяти.
        var stats = await FetchAllPgStatStatementsAsync(map, cancellationToken);

        if (stats.Count == 0)
            return new AnalysisReportAdvanced { Note = "В pg_stat_statements нет записей." };

        // Нормализация: найдём максимумы по метрикам, чтобы вычислить относительный score
        var maxTotal = stats.Max(x => x.TotalTimeMs);
        var maxMean = stats.Max(x => x.MeanTimeMs);
        var maxCalls = Math.Max(1, stats.Max(x => x.Calls));
        var maxShared = Math.Max(1, stats.Max(x => x.SharedBlksRead));
        var maxTemp = Math.Max(1, stats.Max(x => x.TempBlksWritten));
        var maxRows = Math.Max(1, stats.Max(x => x.Rows));
        var maxStd = Math.Max(1.0, stats.Max(x => x.StdDevTimeMs));

        // Вычисляем score и генерируем рекомендации (на русском)
        foreach (var s in stats)
        {
            s.Score = ComputeScore(s, maxTotal, maxMean, maxCalls, maxShared, maxTemp, maxRows, maxStd);
            s.Suggestions.AddRange(GenerateSuggestionsInRussian(s));
        }

        // Берём top N по score
        var top = stats.OrderByDescending(x => x.Score).Take(limit).ToList();

        // Классифицируем критичность и сортируем рекомендации по приоритету
        foreach (var q in top)
        {
            q.Severity = ClassifySeverity(q.Score);
            q.Suggestions.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        // Опционально: run EXPLAIN (dangerous)
        if (includeExplain)
        {
            await RunExplainForTopAsync(top, cancellationToken);
        }

        return new AnalysisReportAdvanced
        {
            GeneratedAtUtc = DateTime.UtcNow,
            Results = top,
            Note = includeExplain
                ? "В отчёт включены EXPLAIN (ANALYZE). Убедитесь, что выполнение происходило на реплике или тестовой среде."
                : "Рекомендации сгенерированы эвристически. Для подтверждения используйте EXPLAIN (ANALYZE, BUFFERS)."
        };
    }

    /// <summary>
    /// Попытка создать расширение, если pg_stat_statements предзагружен.
    /// Возвращает текстовое сообщение о результате.
    /// </summary>
    public async Task<string> EnsurePgStatStatementsInstalledAsync(bool tryCreateExtension = false,
        CancellationToken cancellationToken = default)
    {
        var conn = _db.Database.GetDbConnection();
        var opened = false;
        if (conn.State != ConnectionState.Open)
        {
            await _db.Database.OpenConnectionAsync(cancellationToken);
            opened = true;
        }

        try
        {
            // Проверим shared_preload_libraries
            string preload;
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SHOW shared_preload_libraries;";
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 10;
                var res = await cmd.ExecuteScalarAsync(cancellationToken);
                preload = res?.ToString() ?? "";
            }

            var preloaded = preload.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Any(x => x.Equals("pg_stat_statements", StringComparison.OrdinalIgnoreCase));
            if (!preloaded)
            {
                return
                    $"pg_stat_statements отсутствует в shared_preload_libraries (текущий='{preload}'). Добавьте его в postgresql.conf и перезапустите сервер, затем выполните CREATE EXTENSION в каждой базе.";
            }

            if (tryCreateExtension)
            {
                try
                {
                    await using var cmd = conn.CreateCommand();
                    cmd.CommandText = "CREATE EXTENSION IF NOT EXISTS pg_stat_statements;";
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 30;
                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                    _columnMap = null; // очистить кеш
                    return "CREATE EXTENSION выполнен (или расширение уже существует).";
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "CREATE EXTENSION завершился ошибкой.");
                    return $"CREATE EXTENSION завершился ошибкой: {ex.Message}";
                }
            }

            return
                "pg_stat_statements предзагружен на сервере. Выполните CREATE EXTENSION IF NOT EXISTS pg_stat_statements; в текущей базе (нужны привилегии).";
        }
        finally
        {
            if (opened) await _db.Database.CloseConnectionAsync();
        }
    }

    #region Внутренние хелперы (детект колонок, выборка, explain, scoring, рекомендации на русском)

    private async Task<Dictionary<string, string>> EnsureColumnMapAsync(Guid dbConnectionId, CancellationToken cancellationToken)
    {
        var dbConnection = _db.DbConnections.FirstOrDefault(x => x.Id == dbConnectionId);
        if (dbConnection == null)
        {
            throw new Exception("Ошибка");
        }
        var conn = new NpgsqlConnection(dbConnection.GetConnectionString());
        if (_columnMap != null && DateTime.UtcNow - _columnMapLoadedAt < _columnMapTtl)
            return _columnMap;

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var openedHere = false;
        if (conn.State != ConnectionState.Open)
        {
            await _db.Database.OpenConnectionAsync(cancellationToken);
            openedHere = true;
        }

        try
        {
            // Получим колонки pg_stat_statements
            await using var cmd = conn.CreateCommand();
            cmd.CommandText =
                @"SELECT column_name FROM information_schema.columns WHERE table_name = 'pg_stat_statements';";
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 10;

            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                    existing.Add(reader.GetString(0));
            }

            // Сопоставляем логические имена -> реальные имена колонок
            map["total_time"] = existing.Contains("total_exec_time")
                ? "total_exec_time"
                : (existing.Contains("total_time") ? "total_time" : "");
            map["mean_time"] = existing.Contains("mean_exec_time")
                ? "mean_exec_time"
                : (existing.Contains("mean_time") ? "mean_time" : "");
            map["min_time"] = existing.Contains("min_exec_time")
                ? "min_exec_time"
                : (existing.Contains("min_time") ? "min_time" : "");
            map["max_time"] = existing.Contains("max_exec_time")
                ? "max_exec_time"
                : (existing.Contains("max_time") ? "max_time" : "");
            map["stddev_time"] = existing.Contains("stddev_exec_time")
                ? "stddev_exec_time"
                : (existing.Contains("stddev_time") ? "stddev_time" : "");
            map["rows"] = existing.Contains("rows") ? "rows" : "";
            map["shared_blks_read"] = existing.Contains("shared_blks_read") ? "shared_blks_read" : "";
            map["shared_blks_hit"] = existing.Contains("shared_blks_hit") ? "shared_blks_hit" : "";
            map["temp_blks_written"] = existing.Contains("temp_blks_written") ? "temp_blks_written" : "";
            map["blk_read_time"] = existing.Contains("blk_read_time") ? "blk_read_time" : "";
            map["blk_write_time"] = existing.Contains("blk_write_time") ? "blk_write_time" : "";
            map["calls"] = existing.Contains("calls") ? "calls" : "";
            map["queryid"] = existing.Contains("queryid") ? "queryid" : "";
            map["query"] = existing.Contains("query") ? "query" : "";

            var clean = map.Where(kv => !string.IsNullOrEmpty(kv.Value))
                .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);

            _columnMap = clean;
            _columnMapLoadedAt = DateTime.UtcNow;
            return _columnMap;
        }
        finally
        {
            if (openedHere) await _db.Database.CloseConnectionAsync();
        }
    }

    private string BuildSelectAllSql(Dictionary<string, string> map)
    {
        string col(string alias, string fallback = "0") =>
            map.TryGetValue(alias, out var actual) ? $"{actual} AS {alias}" : $"{fallback} AS {alias}";

        var select = $@"
SELECT 
  {(map.ContainsKey("queryid") ? "queryid::bigint AS queryid" : "0 AS queryid")},
  {(map.ContainsKey("query") ? "query AS query" : "'' AS query")},
  {(map.ContainsKey("calls") ? "calls::bigint AS calls" : "0 AS calls")},
  {col("total_time")},
  {col("mean_time")},
  {col("min_time")},
  {col("max_time")},
  {col("stddev_time")},
  {col("rows")},
  {col("shared_blks_read")},
  {col("shared_blks_hit")},
  {col("temp_blks_written")},
  {col("blk_read_time")},
  {col("blk_write_time")}
FROM pg_stat_statements;"; // intentionally no ORDER BY / LIMIT

        return select;
    }

    private async Task<List<QueryStatAdvanced>> FetchAllPgStatStatementsAsync(Dictionary<string, string> map,
        CancellationToken cancellationToken)
    {
        var sql = BuildSelectAllSql(map);
        var list = new List<QueryStatAdvanced>();

        var conn = _db.Database.GetDbConnection();
        var openedHere = false;
        if (conn.State != ConnectionState.Open)
        {
            await _db.Database.OpenConnectionAsync(cancellationToken);
            openedHere = true;
        }

        try
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 120; // можно увеличить при необходимости

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var qs = new QueryStatAdvanced
                {
                    QueryId = reader.IsDBNull(0) ? 0L : reader.GetFieldValue<long>(0),
                    Query = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Calls = reader.IsDBNull(2) ? 0L : reader.GetFieldValue<long>(2),
                    TotalTimeMs = reader.IsDBNull(3) ? 0.0 : reader.GetFieldValue<double>(3),
                    MeanTimeMs = reader.IsDBNull(4) ? 0.0 : reader.GetFieldValue<double>(4),
                    MinTimeMs = reader.IsDBNull(5) ? 0.0 : reader.GetFieldValue<double>(5),
                    MaxTimeMs = reader.IsDBNull(6) ? 0.0 : reader.GetFieldValue<double>(6),
                    StdDevTimeMs = reader.IsDBNull(7) ? 0.0 : reader.GetFieldValue<double>(7),
                    Rows = reader.IsDBNull(8) ? 0L : reader.GetFieldValue<long>(8),
                    SharedBlksRead = reader.IsDBNull(9) ? 0L : reader.GetFieldValue<long>(9),
                    SharedBlksHit = reader.IsDBNull(10) ? 0L : reader.GetFieldValue<long>(10),
                    TempBlksWritten = reader.IsDBNull(11) ? 0L : reader.GetFieldValue<long>(11),
                    BlkReadTimeMs = reader.IsDBNull(12) ? 0.0 : reader.GetFieldValue<double>(12),
                    BlkWriteTimeMs = reader.IsDBNull(13) ? 0.0 : reader.GetFieldValue<double>(13),
                    Suggestions = new List<Suggestion>()
                };

                list.Add(qs);
            }
        }
        finally
        {
            if (openedHere) await _db.Database.CloseConnectionAsync();
        }

        return list;
    }

    private double ComputeScore(QueryStatAdvanced s, double maxTotal, double maxMean, double maxCalls,
        double maxSharedRead, double maxTemp, double maxRows, double maxStddev)
    {
        // Веса можно подстроить под вашу нагрузку
        const double wTotal = 0.30;
        const double wMean = 0.12;
        const double wCalls = 0.10;
        const double wShared = 0.20;
        const double wTemp = 0.12;
        const double wRows = 0.06;
        const double wStd = 0.10;

        double nTotal = maxTotal > 0 ? s.TotalTimeMs / maxTotal : 0;
        double nMean = maxMean > 0 ? s.MeanTimeMs / maxMean : 0;
        double nCalls = maxCalls > 0 ? (double)s.Calls / maxCalls : 0;
        double nShared = maxSharedRead > 0 ? (double)s.SharedBlksRead / maxSharedRead : 0;
        double nTemp = maxTemp > 0 ? (double)s.TempBlksWritten / maxTemp : 0;
        double nRows = maxRows > 0 ? (double)s.Rows / maxRows : 0;
        double nStd = maxStddev > 0 ? s.StdDevTimeMs / maxStddev : 0;

        double baseScore = wTotal * nTotal + wMean * nMean + wCalls * nCalls + wShared * nShared + wTemp * nTemp +
                           wRows * nRows + wStd * nStd;

        // Текстовые эвристики — добавляют "бонус" к score
        double textBoost = 0;
        var q = (s.Query ?? "").ToLowerInvariant();

        if (q.Contains("select *")) textBoost += 0.08;
        if (Regex.IsMatch(q, @"like\s+'%")) textBoost += 0.14;
        if (q.Contains("ilike")) textBoost += 0.12;
        if (Regex.IsMatch(q, @"where\s+.*\b(lower|upper|to_char|date_trunc)\s*\(")) textBoost += 0.10;
        if (Regex.IsMatch(q, @"order\s+by") && !Regex.IsMatch(q, @"limit\s+\d+")) textBoost += 0.09;
        if (q.Contains(" in (select")) textBoost += 0.04;
        if (s.Rows > 10000) textBoost += 0.06;
        if (s.TempBlksWritten > 0) textBoost += 0.12;

        var raw = baseScore + textBoost;
        var scaled = Math.Min(1.0, raw) * 100.0;
        return Math.Round(scaled, 2);
    }

    private Criticality ClassifySeverity(double score)
    {
        if (score >= 75) return Criticality.Critical;
        if (score >= 55) return Criticality.High;
        if (score >= 35) return Criticality.Medium;
        if (score >= 15) return Criticality.Low;
        return Criticality.Info;
    }

    private IEnumerable<Suggestion> GenerateSuggestionsInRussian(QueryStatAdvanced s)
    {
        var list = new List<Suggestion>();
        var q = (s.Query ?? "").ToLowerInvariant();

        if (s.TotalTimeMs > 10_000 || s.MeanTimeMs > 2_000)
        {
            list.Add(new Suggestion
            {
                Title = "Выполните EXPLAIN (ANALYZE, BUFFERS)",
                Description =
                    $"Запрос суммарно выполнялся {s.TotalTimeMs:F0} ms (в среднем {s.MeanTimeMs:F0} ms). Запустите EXPLAIN (ANALYZE, BUFFERS) на реплике/тестовой среде, чтобы получить точный план — это даст информацию о seq scan / sort / hash spills.",
                Priority = 100
            });
        }

        if (s.Calls > 10_000 && s.MeanTimeMs > 1)
        {
            list.Add(new Suggestion
            {
                Title = "Использовать подготовленные запросы / batching",
                Description =
                    $"Этот запрос вызывается {s.Calls} раз. Если запрос параметризуем — используйте prepared statements, batching или кэширование на клиенте.",
                Priority = 90
            });
        }

        if (s.SharedBlksRead > 1000 || s.SharedBlksRead > s.SharedBlksHit * 2)
        {
            list.Add(new Suggestion
            {
                Title = "Много чтения блоков — возможен full table scan",
                Description =
                    $"shared_blks_read = {s.SharedBlksRead}, shared_blks_hit = {s.SharedBlksHit}. Проверьте фильтры WHERE/JOIN и селективность — возможно, нужен индекс или изменение запроса.",
                Priority = 95
            });
        }

        if (s.TempBlksWritten > 0)
        {
            list.Add(new Suggestion
            {
                Title = "Временные блоки — внешние сортировки/spills",
                Description =
                    $"temp_blks_written = {s.TempBlksWritten}. Запрос использует большие сортировки/хэш-операции и проливает на диск. Попробуйте увеличить work_mem локально и заново выполнить EXPLAIN; рассмотрите пересмотр ORDER BY/GROUP BY.",
                Priority = 95,
                ExampleSql = "SET LOCAL work_mem = '64MB'; -- выполнить EXPLAIN повторно на реплике"
            });
        }

        if (q.Contains("select *"))
        {
            list.Add(new Suggestion
            {
                Title = "Избегать SELECT *",
                Description =
                    "SELECT * увеличивает чтение ненужных колонок и передачу по сети. Указывайте только необходимые поля.",
                Priority = 70
            });
        }

        if (Regex.IsMatch(q, @"like\s+'%"))
        {
            list.Add(new Suggestion
            {
                Title = "LIKE с ведущим %",
                Description =
                    "LIKE '%foo' не использует B-tree индекс. Рассмотрите pg_trgm + GIN или полнотекстовый поиск.",
                Priority = 88,
                ExampleSql =
                    "CREATE EXTENSION IF NOT EXISTS pg_trgm;\nCREATE INDEX ON schema.table USING gin (column gin_trgm_ops);"
            });
        }

        if (q.Contains("ilike") || Regex.IsMatch(q, @"lower\("))
        {
            list.Add(new Suggestion
            {
                Title = "Поиск без учёта регистра",
                Description =
                    "ILIKE или LOWER(col) мешают стандартному индексу. Рассмотрите функциональный индекс (lower(col)) или pg_trgm для строковых совпадений.",
                Priority = 80,
                ExampleSql = "CREATE INDEX ON schema.table (lower(column));"
            });
        }

        if (Regex.IsMatch(q, @"order\s+by") && !Regex.IsMatch(q, @"limit\s+\d+"))
        {
            list.Add(new Suggestion
            {
                Title = "ORDER BY без LIMIT",
                Description =
                    "Полное сортирование большого количества строк дорого. Добавьте LIMIT, используйте индекс для порядка или материализованный вид.",
                Priority = 70
            });
        }

        if (q.Contains(" in (select"))
        {
            list.Add(new Suggestion
            {
                Title = "IN (SELECT...)",
                Description =
                    "IN (SELECT ...) иногда можно заменить на EXISTS или JOIN — это может дать более оптимальный план.",
                Priority = 60
            });
        }

        var idx = TryProposeSimpleIndex(s.Query);
        if (!string.IsNullOrEmpty(idx))
        {
            list.Add(new Suggestion
            {
                Title = "Возможный индекс (эвристика)",
                Description =
                    "Предложение индекса сформировано автоматически (best-effort). Проверьте селективность и сравните EXPLAIN до/после.",
                Priority = 90,
                ExampleSql = idx
            });
        }

        if (s.SharedBlksRead > 10_000)
        {
            list.Add(new Suggestion
            {
                Title = "ANALYZE / статистика",
                Description =
                    "Возможно статистика таблиц устарела — выполните ANALYZE на релевантных таблицах или настройте autovacuum.",
                Priority = 75
            });
        }

        if (q.Contains("group by") || q.Contains("count(") || q.Contains("sum("))
        {
            list.Add(new Suggestion
            {
                Title = "Материализованные представления / pre-aggregation",
                Description =
                    "Если запрос — heavy aggregate для отчётов, рассмотрите materialized views, инкрементальную агрегацию или денормализацию.",
                Priority = 65
            });
        }

        if (!list.Any())
        {
            list.Add(new Suggestion
            {
                Title = "Общий совет",
                Description =
                    "Выполните EXPLAIN (ANALYZE, BUFFERS) и проанализируйте план вручную. Автоматические подсказки — эвристики.",
                Priority = 10
            });
        }

        return list;
    }

    private string? TryProposeSimpleIndex(string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query)) return null;
            var qn = Regex.Replace(query, @"\s+", " ", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var fromMatch = Regex.Match(qn, @"from\s+([a-zA-Z0-9_\.]+)", RegexOptions.IgnoreCase);
            if (!fromMatch.Success) return null;
            var table = fromMatch.Groups[1].Value;
            var whereMatch = Regex.Match(qn, @"where\s+(.*?)($|\border\b|\bgroup\b|\blimit\b)",
                RegexOptions.IgnoreCase);
            if (!whereMatch.Success) return null;
            var where = whereMatch.Groups[1].Value;
            var colMatch = Regex.Match(where, @"([a-zA-Z0-9_\.]+)\s*=\s*(?:[:'\d\w\(])", RegexOptions.IgnoreCase);
            if (!colMatch.Success) return null;
            var col = colMatch.Groups[1].Value;
            if (col.Contains(".")) col = col.Substring(col.IndexOf('.') + 1);
            var idxName = $"idx_{table.Replace('.', '_')}_{col}";
            return $"-- Проверьте селективность перед созданием\nCREATE INDEX {idxName} ON {table} ({col});";
        }
        catch
        {
            return null;
        }
    }

    private async Task RunExplainForTopAsync(List<QueryStatAdvanced> top, CancellationToken cancellationToken)
    {
        foreach (var q in top)
        {
            try
            {
                var rawQuery = q.Query;
                if (string.IsNullOrWhiteSpace(rawQuery)) continue;

                // Заменяем $1,$2 на NULL — best-effort; план может отличаться
                var safe = Regex.Replace(rawQuery, @"\$\d+", "NULL", RegexOptions.Compiled);
                safe = safe.Trim().TrimEnd(';');

                var explainSql = $"EXPLAIN (ANALYZE, BUFFERS, FORMAT JSON) {safe};";

                var conn = _db.Database.GetDbConnection();
                var openedHere = false;
                if (conn.State != ConnectionState.Open)
                {
                    await _db.Database.OpenConnectionAsync(cancellationToken);
                    openedHere = true;
                }

                try
                {
                    await using var cmd = conn.CreateCommand();
                    cmd.CommandText = explainSql;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 300;

                    await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        var raw = reader.GetString(0);
                        try
                        {
                            q.ExplainPlanJson = JsonSerializer.Deserialize<object>(raw);
                        }
                        catch
                        {
                            q.ExplainPlanJson = raw;
                        }
                    }
                }
                finally
                {
                    if (openedHere) await _db.Database.CloseConnectionAsync();
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex,
                    "EXPLAIN завершился ошибкой для запроса — пропускаем. Запрос может содержать параметры или требовать прав.");
                q.ExplainPlanJson = null;
            }
        }
    }

    #endregion
}

// --- DTOs (если у тебя уже есть модели, используй свои; это минимальные реализации) ---
public enum Criticality
{
    Critical,
    High,
    Medium,
    Low,
    Info
}

public class QueryStatAdvanced
{
    public long QueryId { get; set; }
    public string Query { get; set; } = "";
    public long Calls { get; set; }
    public double TotalTimeMs { get; set; }
    public double MeanTimeMs { get; set; }
    public double MinTimeMs { get; set; }
    public double MaxTimeMs { get; set; }
    public double StdDevTimeMs { get; set; }
    public long Rows { get; set; }
    public long SharedBlksRead { get; set; }
    public long SharedBlksHit { get; set; }
    public long TempBlksWritten { get; set; }
    public double BlkReadTimeMs { get; set; }
    public double BlkWriteTimeMs { get; set; }
    public double Score { get; set; }
    public Criticality Severity { get; set; }
    public List<Suggestion> Suggestions { get; set; } = new();
    public object? ExplainPlanJson { get; set; }
}

public class Suggestion
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public int Priority { get; set; }
    public string? ExampleSql { get; set; }
}

public class AnalysisReportAdvanced
{
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
    public List<QueryStatAdvanced> Results { get; set; } = new();
    public string? Note { get; set; }
}