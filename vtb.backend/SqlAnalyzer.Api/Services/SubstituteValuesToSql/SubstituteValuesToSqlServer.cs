using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Extensions;
using SqlAnalyzer.Api.Services.SubstituteValuesToSql.Interfaces;

public class SubstituteValuesToSqlServer : ISubstituteValuesToSqlServer
{
    private readonly DataContext _db;
    public SubstituteValuesToSqlServer(DataContext db) => _db = db;

    public async Task<string> SubstituteValuesToSql(SubstituteValuesToSqlRequestDto dto)
    {
        var dbConnection = await _db.DbConnections.FirstOrDefaultAsync(x => x.Id == dto.DbConnectionId);
        if (dbConnection == null) throw new InvalidOperationException("DbConnection not found");

        var prepareName = "genc_" + Guid.NewGuid().ToString("N");
        await using var conn = new NpgsqlConnection(dbConnection.GetConnectionString());
        await conn.OpenAsync();

        try
        {
            // 1) PREPARE - с retry (на случай transient ошибок)
            await ExecuteWithRetryAsync(async () =>
            {
                // PREPARE expects the raw SQL statement after AS — если SQL содержит точку с запятой внутри
                // это может сломать, но в обычных запросах рабочий вариант.
                await using var prepareCmd = new NpgsqlCommand($"PREPARE {prepareName} AS {dto.Sql}", conn);
                await prepareCmd.ExecuteNonQueryAsync();
            });

            // 2) Получаем parameter_types из pg_prepared_statements
            var paramTypes = new List<string>();
            const string paramQuery = "SELECT parameter_types::text[] FROM pg_prepared_statements WHERE name = @name";
            await using (var cmd = new NpgsqlCommand(paramQuery, conn))
            {
                cmd.Parameters.AddWithValue("name", prepareName);
                await using var rdr = await cmd.ExecuteReaderAsync();
                if (await rdr.ReadAsync())
                {
                    var arr = rdr.IsDBNull(0) ? Array.Empty<string>() : (string[])rdr.GetValue(0);
                    paramTypes.AddRange(arr.Select(s => s ?? "unknown"));
                }
            }

            if (paramTypes.Count == 0)
            {
                // нет параметров — просто деаллокируем и вернём исходный SQL
                await using (var dealloc = new NpgsqlCommand($"DEALLOCATE {prepareName}", conn))
                    await dealloc.ExecuteNonQueryAsync();

                return dto.Sql;
            }

            // 3) resolvedTypes <- paramTypes (копируем)
            var resolvedTypes = paramTypes.ToArray();

            // Контекст использования параметра (например: Percentile)
            var paramUsages = new Dictionary<int, ParamUsage>();

            // 4) Функционные эвристики (percentile_disc/percentile_cont и пр.)
            ApplyFunctionHeuristics(dto.Sql, resolvedTypes, paramUsages);

            // 5) Для оставшихся unknown попробуем инферировать по выражениям "col = $n" и alias -> table из FROM/JOIN
            var unknownIndexes = resolvedTypes
                .Select((t, idx) => (t, idx))
                .Where(x => string.Equals(x.t, "unknown", StringComparison.OrdinalIgnoreCase))
                .Select(x => x.idx)
                .ToArray();

            if (unknownIndexes.Length > 0)
            {
                var heuristics = await TryInferUnknownParameterTypesFromSql(conn, dto.Sql, unknownIndexes);
                foreach (var (idx, pgType) in heuristics)
                {
                    resolvedTypes[idx] = pgType ?? "text";
                }
            }

            // 6) Генерируем литералы с учётом контекста (percentile -> 0..1)
            var paramLiterals = new Dictionary<int, (string PgType, string Literal)>();
            for (int i = 0; i < resolvedTypes.Length; i++)
            {
                var pgType = resolvedTypes[i] ?? "text";
                paramUsages.TryGetValue(i + 1, out var usage); // paramUsages keys are 1-based
                var literal = GenerateRandomLiteralForPgType(pgType, usage);
                paramLiterals[i + 1] = (pgType, literal);
            }

            // 7) Replace $n tokens safely (regex will match correct numbers: $1, $2, $10 etc.)
            var finalSql = Regex.Replace(dto.Sql, @"\$(\d+)", match =>
            {
                var idx = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                return paramLiterals.TryGetValue(idx, out var v) ? v.Literal : match.Value;
            });

            // 8) Deallocate prepared statement (по имени)
            await using (var dealloc = new NpgsqlCommand($"DEALLOCATE {prepareName}", conn))
                await dealloc.ExecuteNonQueryAsync();

            return finalSql;
        }
        catch
        {
            // Попытка аккуратно деаллокировать подготовленный statement (без "DEALLOCATE ALL")
            try
            {
                if (conn.FullState != System.Data.ConnectionState.Closed)
                {
                    await using var dealloc = new NpgsqlCommand($"DEALLOCATE {prepareName}", conn);
                    await dealloc.ExecuteNonQueryAsync();
                }
                else
                {
                    // если текущее соединение закрыто — откроем новое для cleanup (без влияния на пул)
                    await using var cleanupConn = new NpgsqlConnection(dbConnection.GetConnectionString());
                    await cleanupConn.OpenAsync();
                    await using var dealloc = new NpgsqlCommand($"DEALLOCATE {prepareName}", cleanupConn);
                    await dealloc.ExecuteNonQueryAsync();
                }
            }
            catch
            {
                // ignore cleanup errors
            }

            throw;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    // ---- Вспомогательные методы ----

    private static void ApplyFunctionHeuristics(string sql, string[] resolvedTypes,
        Dictionary<int, ParamUsage> paramUsages)
    {
        // percentile_cont($N) или percentile_disc($N) -> double precision, usage Percentile
        var pctMatches = Regex.Matches(sql, @"percentile_(?:cont|disc)\s*\(\s*\$(\d+)\s*\)", RegexOptions.IgnoreCase);
        foreach (Match m in pctMatches)
        {
            if (!int.TryParse(m.Groups[1].Value, out var pnum)) continue;
            var idx = pnum - 1;
            if (idx >= 0 && idx < resolvedTypes.Length)
            {
                resolvedTypes[idx] = "double precision";
                paramUsages[pnum] = ParamUsage.Percentile;
            }
        }

        // можно добавить другие функции/правила тут, например width_bucket, ntile, и т.д.
    }

    // Простая retry-реализация (exponential backoff). Нельзя бесконечно ретраить ошибки, поэтому лимит установлен.
    private static async Task ExecuteWithRetryAsync(Func<Task> action, int maxAttempts = 3)
    {
        var attempt = 0;
        while (true)
        {
            try
            {
                await action();
                return;
            }
            catch (NpgsqlException) when (++attempt < maxAttempts)
            {
                var wait = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                await Task.Delay(wait);
                continue;
            }
            catch (TimeoutException) when (++attempt < maxAttempts)
            {
                var wait = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                await Task.Delay(wait);
                continue;
            }
            catch
            {
                throw;
            }
        }
    }

    private enum ParamUsage
    {
        None,
        Percentile
    }

    private static string GenerateRandomLiteralForPgType(string pgType, ParamUsage usage)
    {
        pgType = (pgType ?? "text").ToLowerInvariant();
        var rnd = Random.Shared;

        switch (pgType)
        {
            case "integer":
            case "int":
            case "int4":
                return rnd.Next(1, 100000).ToString(CultureInfo.InvariantCulture);

            case "bigint":
            case "int8":
                return rnd.NextInt64(1, 1_000_000_000).ToString(CultureInfo.InvariantCulture);

            case "smallint":
            case "int2":
                return ((short)rnd.Next(1, 30000)).ToString(CultureInfo.InvariantCulture);

            case "real":
            case "float4":
                // Для float по умолчанию хуй знает — генерируем небольшое дробное
                return (rnd.NextDouble() * 1000).ToString("G", CultureInfo.InvariantCulture);

            case "double precision":
            case "float8":
                if (usage == ParamUsage.Percentile)
                {
                    // обязательно в интервале [0,1]
                    var v = rnd.NextDouble();
                    // формат с точностью до 6 знаков, без локали
                    return v.ToString("0.######", CultureInfo.InvariantCulture);
                }
                else
                {
                    // общий double
                    return (rnd.NextDouble() * 1000 - 500).ToString("G", CultureInfo.InvariantCulture);
                }

            case "boolean":
            case "bool":
                return rnd.Next(0, 2) == 0 ? "false" : "true";

            case "timestamp with time zone":
            case "timestamptz":
            case "timestamp without time zone":
            case "timestamp":
                var dt = DateTime.UtcNow.AddDays(-rnd.Next(0, 3650)).AddSeconds(rnd.Next(0, 86400));
                return $"'{dt:yyyy-MM-dd HH:mm:ss}'";

            case "date":
                var d = DateTime.UtcNow.Date.AddDays(-rnd.Next(0, 3650));
                return $"'{d:yyyy-MM-dd}'";

            case "uuid":
                return $"'{Guid.NewGuid()}'";

            case "json":
            case "jsonb":
                return $"'{{\"k\":\"{Guid.NewGuid().ToString("N").Substring(0, 6)}\"}}'::jsonb";

            case "numeric":
                // numeric без контекста — сгенерируем дробь 0..1000 с 2 знаками
                var num = Math.Round(rnd.NextDouble() * 1000, 2);
                return num.ToString(CultureInfo.InvariantCulture);

            case "text":
            case "character varying":
            case "varchar":
            case "character":
            case "char":
            default:
                var s = Guid.NewGuid().ToString("N").Substring(0, 10);
                var escaped = s.Replace("'", "''");
                return $"'{escaped}'";
        }
    }

    // Старая эвристика: пытаемся найти соответствие параметра и колонки по "alias.col = $N"
    private static async Task<Dictionary<int, string>> TryInferUnknownParameterTypesFromSql(NpgsqlConnection conn,
        string sql, int[] unknownIndexes)
    {
        var result = new Dictionary<int, string>();

        var aliasMap = new Dictionary<string, (string Schema, string Table)>(StringComparer.OrdinalIgnoreCase);

        // Простая регулярка для FROM и JOIN элементов: схематично извлекаем элементы "schema.table AS alias" или "table alias"
        var fromMatches = Regex.Matches(sql, @"(?:FROM|JOIN)\s+([^\s,()]+)(?:\s+(?:AS\s+)?(\w+))?",
            RegexOptions.IgnoreCase);
        foreach (Match m in fromMatches)
        {
            var full = m.Groups[1].Value.Trim();
            var alias = m.Groups[2].Success ? m.Groups[2].Value : null;
            if (alias == null) alias = full.Contains('.') ? full.Split('.').Last() : full;
            string schema = null;
            string table = full;
            if (full.Contains('.'))
            {
                var parts = full.Split('.');
                schema = parts[0].Trim('"');
                table = parts[1].Trim('"');
            }

            aliasMap[alias] = (schema, table);
        }

        // Ищем выражения вида alias.col = $N или col = $N
        var condMatches = Regex.Matches(sql, @"([A-Za-z_][\w\.]*)\s*(?:=|<>|!=|<|>|<=|>=)\s*\$(\d+)",
            RegexOptions.IgnoreCase);
        foreach (Match m in condMatches)
        {
            var left = m.Groups[1].Value;
            if (!int.TryParse(m.Groups[2].Value, out var paramNum)) continue;

            var zeroBasedParamIndex = paramNum - 1;
            if (!unknownIndexes.Contains(zeroBasedParamIndex)) continue;

            string schema = null, table = null, column = null;
            if (left.Contains('.'))
            {
                var parts = left.Split('.', 2);
                var maybeAlias = parts[0];
                column = parts[1].Trim('"');
                if (aliasMap.TryGetValue(maybeAlias, out var st))
                {
                    schema = st.Schema;
                    table = st.Table;
                }
            }
            else
            {
                column = left.Trim('"');
                if (aliasMap.Count == 1)
                {
                    var kv = aliasMap.First().Value;
                    schema = kv.Schema;
                    table = kv.Table;
                }
            }

            if (table != null && column != null)
            {
                var pgType = await GetColumnPgType(conn, schema, table, column);
                if (!string.IsNullOrEmpty(pgType))
                    result[zeroBasedParamIndex] = pgType;
            }
        }

        return result;
    }

    private static async Task<string> GetColumnPgType(NpgsqlConnection conn, string schema, string table, string column)
    {
        if (string.IsNullOrEmpty(schema)) schema = "public";
        var q = @"
SELECT udt_name, data_type
FROM information_schema.columns
WHERE table_schema = @schema AND table_name = @table AND column_name = @column
LIMIT 1";
        await using var cmd = new NpgsqlCommand(q, conn);
        cmd.Parameters.AddWithValue("schema", schema);
        cmd.Parameters.AddWithValue("table", table);
        cmd.Parameters.AddWithValue("column", column);
        await using var rdr = await cmd.ExecuteReaderAsync();
        if (await rdr.ReadAsync())
        {
            var udt = rdr.GetString(0);
            var dataType = rdr.GetString(1);
            return NormalizePgTypeName(udt, dataType);
        }

        return null;
    }

    private static string NormalizePgTypeName(string udtName, string dataType)
    {
        udtName = udtName?.ToLowerInvariant() ?? "";
        dataType = dataType?.ToLowerInvariant() ?? "";
        return udtName switch
        {
            "int4" => "integer",
            "int8" => "bigint",
            "int2" => "smallint",
            "text" => "text",
            "varchar" => "character varying",
            "bool" => "boolean",
            "timestamptz" => "timestamp with time zone",
            "timestamp" => "timestamp without time zone",
            "date" => "date",
            "uuid" => "uuid",
            "json" => "json",
            "jsonb" => "jsonb",
            "float4" => "real",
            "float8" => "double precision",
            "numeric" => "numeric",
            _ => dataType ?? udtName
        };
    }
}

public record SubstituteValuesToSqlRequestDto(Guid DbConnectionId, string Sql);