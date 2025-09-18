using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Extensions;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models; // DataContext и модель DbConnections

/// <summary>
/// Анализатор pg_stat_statements — упрощённая версия:
/// - нет limit и нет EXPLAIN
/// - возвращает все записи pg_stat_statements для заданного dbConnectionId
/// - не ломается при отсутствии колонок/расширения
/// - рекомендации и сообщения — только на русском; если проблем не найдено — добавляется "Нет замечаний"
/// </summary>
public class PgStatAnalyzerService : IPgStatAnalyzerService
{
    private readonly DataContext _db; // содержит список DbConnections
    private readonly ILogger<PgStatAnalyzerService> _log;
    private readonly IStaticSqlAnalyzer _staticSqlAnalyzer;

    // Кеш карт колонок на целевую БД (ключ — hash connection string)
    private readonly Dictionary<string, (Dictionary<string, string> map, DateTime loadedAt)> _columnMaps = new();
    private readonly TimeSpan _columnMapTtl = TimeSpan.FromMinutes(5);

    public PgStatAnalyzerService(DataContext db, 
        ILogger<PgStatAnalyzerService> log, 
        IStaticSqlAnalyzer staticSqlAnalyzer)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _staticSqlAnalyzer = staticSqlAnalyzer;
    }

    /// <summary>
    /// Анализирует все записи pg_stat_statements в целевой базе (dbConnectionId).
    /// Возвращает полный отчёт (включая все найденные записи).
    /// </summary>
    public async Task<AnalysisReportAdvanced> AnalyzeTopAsync(Guid dbConnectionId, CancellationToken cancellationToken = default)
    {
        var map = await EnsureColumnMapAsync(dbConnectionId, cancellationToken);
        if (map == null || map.Count == 0)
        {
            return new AnalysisReportAdvanced
            {
                GeneratedAtUtc = DateTime.UtcNow,
                Results = new List<QueryStatAdvanced>(),
                Note = "pg_stat_statements недоступен в целевой базе или не удалось определить колонки."
            };
        }

        var stats = await FetchAllPgStatStatementsAsync(dbConnectionId, map, cancellationToken);

        if (stats.Count == 0)
            return new AnalysisReportAdvanced { GeneratedAtUtc = DateTime.UtcNow, Results = new List<QueryStatAdvanced>(), Note = "В pg_stat_statements нет доступных записей." };

        // Нормализация для score
        var maxTotal = stats.Max(x => x.TotalTimeMs);
        var maxMean = stats.Max(x => x.MeanTimeMs);
        var maxCalls = Math.Max(1, stats.Max(x => x.Calls));
        var maxShared = Math.Max(1, stats.Max(x => x.SharedBlksRead));
        var maxTemp = Math.Max(1, stats.Max(x => x.TempBlksWritten));
        var maxRows = Math.Max(1, stats.Max(x => x.Rows));

        foreach (var s in stats)
        {
            s.Score = ComputeScore(s, maxTotal, maxMean, maxCalls, maxShared, maxTemp, maxRows);
            var suggestions = (await GenerateSuggestionsInRussianAsync(s)).ToList();

            // если предложений нет (т.е. явных проблем не выявлено) — добавляем "Нет замечаний"
            if (!suggestions.Any())
            {
                suggestions.Add(new Suggestion
                {
                    Title = "Нет замечаний",
                    Description = "По текущим эвристикам проблем не обнаружено.",
                    Priority = 0
                });
                s.Severity = Criticality.Info;
            }
            else
            {
                s.Severity = ClassifySeverity(s.Score);
            }

            s.Suggestions = suggestions;
        }

        // возвращаем все записи в отчёте (по запросу без лимита)
        return new AnalysisReportAdvanced
        {
            GeneratedAtUtc = DateTime.UtcNow,
            Results = stats,
            Note = "Анализ выполнен. EXPLAIN отключён (по запросу)."
        };
    }

    /// <summary>
    /// Проверяет наличие расширения и строит карту доступных колонок (fallback для PG15),
    /// используя соединение по dbConnectionId.
    /// </summary>
    private async Task<Dictionary<string, string>> EnsureColumnMapAsync(Guid dbConnectionId, CancellationToken cancellationToken)
    {
        var dbConnection = _db.DbConnections.FirstOrDefault(x => x.Id == dbConnectionId);
        if (dbConnection == null) throw new Exception("DbConnection не найден.");

        var connStr = dbConnection.GetConnectionString();
        var cacheKey = connStr.GetHashCode().ToString();

        if (_columnMaps.TryGetValue(cacheKey, out var cached) && DateTime.UtcNow - cached.loadedAt < _columnMapTtl)
            return cached.map;

        // Открываем NpgsqlConnection
        await using var conn = new NpgsqlConnection(connStr);
        try
        {
            await conn.OpenAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Не удалось открыть соединение к целевой БД (id={DbConnectionId})", dbConnectionId);
            _columnMaps[cacheKey] = (new Dictionary<string, string>(), DateTime.UtcNow);
            return _columnMaps[cacheKey].map;
        }

        try
        {
            // Проверим установлено ли расширение в базе
            try
            {
                await using var cmdExt = conn.CreateCommand();
                cmdExt.CommandText = "SELECT EXISTS(SELECT 1 FROM pg_extension WHERE extname = 'pg_stat_statements')";
                cmdExt.CommandType = CommandType.Text;
                cmdExt.CommandTimeout = 5;
                var exists = await cmdExt.ExecuteScalarAsync(cancellationToken);
                if (exists is bool b && !b)
                {
                    _columnMaps[cacheKey] = (new Dictionary<string, string>(), DateTime.UtcNow);
                    return _columnMaps[cacheKey].map;
                }
            }
            catch
            {
                // продолжим — попробуем information_schema (пусть будет fallback)
            }

            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT column_name FROM information_schema.columns WHERE table_name = 'pg_stat_statements';";
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 10;

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                    existing.Add(reader.GetString(0));
            }
            catch
            {
                existing.Clear(); // force fallback
            }

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (existing.Count > 0)
            {
                map["queryid"] = existing.Contains("queryid") ? "queryid" : "";
                map["query"] = existing.Contains("query") ? "query" : "";
                map["calls"] = existing.Contains("calls") ? "calls" : "";
                map["total_time"] = existing.Contains("total_exec_time") ? "total_exec_time" : (existing.Contains("total_time") ? "total_time" : "");
                map["mean_time"] = existing.Contains("mean_exec_time") ? "mean_exec_time" : (existing.Contains("mean_time") ? "mean_time" : "");
                map["rows"] = existing.Contains("rows") ? "rows" : "";
                map["shared_blks_read"] = existing.Contains("shared_blks_read") ? "shared_blks_read" : "";
                map["temp_blks_written"] = existing.Contains("temp_blks_written") ? "temp_blks_written" : "";
            }
            else
            {
                // fallback assume PG15-ish names
                map["queryid"] = "queryid";
                map["query"] = "query";
                map["calls"] = "calls";
                map["total_time"] = "total_exec_time";
                map["mean_time"] = "mean_exec_time";
                map["rows"] = "rows";
                map["shared_blks_read"] = "shared_blks_read";
                map["temp_blks_written"] = "temp_blks_written";
            }

            var clean = map.Where(kv => !string.IsNullOrEmpty(kv.Value)).ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
            _columnMaps[cacheKey] = (clean, DateTime.UtcNow);
            return clean;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    // Формируем SQL с фолбэком: отсутствующие колонки заменяются на константы (0 / '')
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
  {col("rows")},
  {col("shared_blks_read")},
  {col("temp_blks_written")}
FROM pg_stat_statements
WHERE COALESCE(query, '') <> '<insufficient privilege>' AND COALESCE(query, '') <> '';";

        return select;
    }

    private async Task<List<QueryStatAdvanced>> FetchAllPgStatStatementsAsync(Guid dbConnectionId, Dictionary<string, string> map, CancellationToken cancellationToken)
    {
        var dbConnection = _db.DbConnections.FirstOrDefault(x => x.Id == dbConnectionId);
        if (dbConnection == null) throw new Exception("DbConnection не найден.");

        var connStr = dbConnection.GetConnectionString();
        var sql = BuildSelectAllSql(map);
        var result = new List<QueryStatAdvanced>();

        await using var conn = new NpgsqlConnection(connStr);
        try
        {
            await conn.OpenAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Не удалось открыть соединение для FetchAllPgStatStatementsAsync (id={DbConnectionId})", dbConnectionId);
            return result;
        }

        try
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 120;

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var qs = new QueryStatAdvanced
                {
                    Query = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Calls = reader.IsDBNull(2) ? 0L : reader.GetFieldValue<long>(2),
                    TotalTimeMs = reader.IsDBNull(3) ? 0.0 : reader.GetFieldValue<double>(3),
                    MeanTimeMs = reader.IsDBNull(4) ? 0.0 : reader.GetFieldValue<double>(4),
                    Rows = reader.IsDBNull(5) ? 0L : reader.GetFieldValue<long>(5),
                    SharedBlksRead = reader.IsDBNull(6) ? 0L : reader.GetFieldValue<long>(6),
                    TempBlksWritten = reader.IsDBNull(7) ? 0L : reader.GetFieldValue<long>(7),
                    Suggestions = new List<Suggestion>()
                };

                result.Add(qs);
            }
        }
        catch (PostgresException pgEx)
        {
            _log.LogWarning(pgEx, "Ошибка при чтении pg_stat_statements (возможно расширение не создано в базе).");
            return new List<QueryStatAdvanced>();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Неожиданная ошибка при чтении pg_stat_statements.");
            return new List<QueryStatAdvanced>();
        }
        finally
        {
            await conn.CloseAsync();
        }

        return result;
    }

    // Score и рекомендации (русский)
    private double ComputeScore(QueryStatAdvanced s, double maxTotal, double maxMean, double maxCalls, double maxSharedRead, double maxTemp, double maxRows)
    {
        const double wTotal = 0.40;
        const double wMean = 0.15;
        const double wCalls = 0.10;
        const double wShared = 0.20;
        const double wTemp = 0.10;
        const double wRows = 0.05;

        double nTotal = maxTotal > 0 ? s.TotalTimeMs / maxTotal : 0;
        double nMean = maxMean > 0 ? s.MeanTimeMs / maxMean : 0;
        double nCalls = maxCalls > 0 ? (double)s.Calls / maxCalls : 0;
        double nShared = maxSharedRead > 0 ? (double)s.SharedBlksRead / maxSharedRead : 0;
        double nTemp = maxTemp > 0 ? (double)s.TempBlksWritten / maxTemp : 0;
        double nRows = maxRows > 0 ? (double)s.Rows / maxRows : 0;

        double baseScore = wTotal * nTotal + wMean * nMean + wCalls * nCalls + wShared * nShared + wTemp * nTemp + wRows * nRows;

        double textBoost = 0;
        var q = (s.Query ?? "").ToLowerInvariant();
        if (q.Contains("select *")) textBoost += 0.08;
        if (Regex.IsMatch(q, @"like\s+'%")) textBoost += 0.12;
        if (q.Contains("ilike")) textBoost += 0.08;
        if (Regex.IsMatch(q, @"order\s+by") && !Regex.IsMatch(q, @"limit\s+\d+")) textBoost += 0.06;
        if (s.TempBlksWritten > 0) textBoost += 0.10;

        var raw = baseScore + textBoost;
        var scaled = Math.Min(1.0, raw) * 100.0;
        return Math.Round(scaled, 2);
    }

    private Criticality ClassifySeverity(double score)
    {
        if (score >= 80) return Criticality.Critical;
        if (score >= 60) return Criticality.High;
        if (score >= 40) return Criticality.Medium;
        if (score >= 20) return Criticality.Low;
        return Criticality.Info;
    }

    private async Task<IEnumerable<Suggestion>> GenerateSuggestionsInRussianAsync(QueryStatAdvanced s)
    {
        var list = new List<Suggestion>();
        var q = (s.Query ?? "").ToLowerInvariant();

        // Вставляем только целевые предложения (без призывов к EXPLAIN)
        if (s.Calls > 10_000 && s.MeanTimeMs > 1)
            list.Add(new Suggestion { Title = "Использовать подготовленные запросы / batching", Description = $"Запрос вызывается {s.Calls} раз. Рассмотрите prepared statements или batching.", Priority = 90 });

        if (s.SharedBlksRead > 1000)
            list.Add(new Suggestion { Title = "Проверить индексы / селективность", Description = $"Много чтения блоков: shared_blks_read = {s.SharedBlksRead}. Проверьте WHERE/JOIN и подумайте об индексе.", Priority = 85 });

        if (s.TempBlksWritten > 0)
            list.Add(new Suggestion { Title = "Spill на диск", Description = $"temp_blks_written = {s.TempBlksWritten}. Рассмотрите временное повышение work_mem при тестировании и оптимизацию сортировок/агрегаций.", Priority = 85 });

        if (q.Contains("select *"))
            list.Add(new Suggestion { Title = "Избегать SELECT *", Description = "Указывайте только нужные колонки.", Priority = 60 });

        var idx = TryProposeSimpleIndex(s.Query);
        if (!string.IsNullOrEmpty(idx))
            list.Add(new Suggestion { Title = "Возможный индекс (эвристика)", Description = "Проверьте селективность и сравните план выполнения до/после.", Priority = 80, ExampleSql = idx });

        var a = await _staticSqlAnalyzer.AnalyzeAsync(new SqlQuery(s.Query));
        list.AddRange(a.Findings.Select(finding => new Suggestion { Title = finding.Problem, Description = finding.Recommendations, Priority = (int)finding.Severity }));
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
            var whereMatch = Regex.Match(qn, @"where\s+(.*?)($|\border\b|\bgroup\b|\blimit\b)", RegexOptions.IgnoreCase);
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

    public Task<string> EnsurePgStatStatementsInstalledAsync(bool tryCreateExtension = false,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

// DTOs (минимальные)
public enum Criticality { Critical, High, Medium, Low, Info }

public class QueryStatAdvanced
{
    public string Query { get; set; } = "";
    public long Calls { get; set; }
    public double TotalTimeMs { get; set; }
    public double MeanTimeMs { get; set; }
    public long Rows { get; set; }
    public long SharedBlksRead { get; set; }
    public long TempBlksWritten { get; set; }
    public double Score { get; set; }
    public Criticality Severity { get; set; }
    public List<Suggestion> Suggestions { get; set; } = new();
    public object? ExplainPlanJson { get; set; } = null;
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
