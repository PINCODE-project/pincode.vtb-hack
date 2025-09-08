using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Extensions;
using SqlAnalyzer.Api.Services.SubstituteValuesToSql.Interfaces;

namespace SqlAnalyzer.Api.Services.SubstituteValuesToSql;

public class SubstituteValuesToSqlServer : ISubstituteValuesToSqlServer
{
    private readonly DataContext _db;
    public SubstituteValuesToSqlServer(DataContext db)
    {
        _db = db;
    }
    
    public async Task<string> SubstituteValuesToSql(SubstituteValuesToSqlRequestDto dto)
    {
        var dbConnection = await _db
            .DbConnections.FirstOrDefaultAsync(x => x.Id == dto.DbConnectionId);

        if (dbConnection == null)
        {
            throw new InvalidOperationException("DbConnection not found");
        }
        
        var prepareName = "genc_" + Guid.NewGuid().ToString("N");
        await using var conn = new NpgsqlConnection(dbConnection.GetConnectionString());
        await conn.OpenAsync();
         try
        {
            // 1) PREPARE the statement on the server side (server will infer types when possible)
            var prepareCmdText = $"PREPARE {prepareName} AS {dto.Sql}";
            await using (var cmd = new NpgsqlCommand(prepareCmdText, conn))
            {
                // If PREPARE fails (syntax, permissions), bubble exception up
                await cmd.ExecuteNonQueryAsync();
            }

            // 2) Read parameter types from pg_prepared_statements
            var paramTypes = new List<string>();
            var q = "SELECT parameter_types::text[] FROM pg_prepared_statements WHERE name = @name";
            await using (var cmd = new NpgsqlCommand(q, conn))
            {
                cmd.Parameters.AddWithValue("name", prepareName);
                await using var rdr = await cmd.ExecuteReaderAsync();
                if (await rdr.ReadAsync())
                {
                    // Npgsql returns string[] for text[]
                    var arr = rdr.IsDBNull(0) ? Array.Empty<string>() : (string[])rdr.GetValue(0);
                    paramTypes.AddRange(arr.Select(s => s ?? "unknown"));
                }
            }

            // If no parameters - simple return
            if (paramTypes.Count == 0)
            {
                // Deallocate prepared statement
                await using (var dealloc = new NpgsqlCommand($"DEALLOCATE {prepareName}", conn))
                    await dealloc.ExecuteNonQueryAsync();

                return dto.Sql;
            }

            // 3) For any 'unknown' param types, attempt heuristic inference from SQL (simple patterns)
            var resolvedTypes = new string[paramTypes.Count];
            for (int i = 0; i < paramTypes.Count; i++)
                resolvedTypes[i] = paramTypes[i];

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

            // 4) Generate random literals for types
            var paramLiterals = new Dictionary<int, (string PgType, string Literal)>();
            for (int i = 0; i < resolvedTypes.Length; i++)
            {
                var pgType = resolvedTypes[i] ?? "text";
                var literal = GenerateRandomLiteralForPgType(pgType);
                paramLiterals[i + 1] = (pgType, literal); // params are 1-based
            }

            // 5) Replace $n tokens in original SQL with generated literals (careful: $10 vs $1)
            var finalSql = Regex.Replace(dto.Sql, @"\$(\d+)", match =>
            {
                var idx = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                return paramLiterals.TryGetValue(idx, out var v) ? v.Literal : match.Value;
            });

            // 6) Deallocate prepared statement
            await using (var dealloc = new NpgsqlCommand($"DEALLOCATE {prepareName}", conn))
                await dealloc.ExecuteNonQueryAsync();

            return finalSql;
        }
        catch
        {
            // Try to cleanup in case of errors
            try
            {
                await using var cleanupConn = new NpgsqlConnection(dbConnection.GetConnectionString());
                await cleanupConn.OpenAsync();
                await using var cleanupCmd = new NpgsqlCommand($"DEALLOCATE ALL", cleanupConn);
                await cleanupCmd.ExecuteNonQueryAsync();
            }
            catch { /* ignore */ }

            throw;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }
    
    private static async Task<Dictionary<int, string>> TryInferUnknownParameterTypesFromSql(NpgsqlConnection conn, string sql, int[] unknownIndexes)
    {
        // result: index -> pg_type_name
        var result = new Dictionary<int, string>();

        // 1) gather table aliases: FROM schema.table AS alias OR FROM table alias
        // very simple regex-based extractor (won't cover all SQL; for full coverage use libpg_query).
        var aliasMap = new Dictionary<string, (string Schema, string Table)>(StringComparer.OrdinalIgnoreCase);

        // Match FROM ... (simple) - grabs sequences like "schema.table AS t" or "table t"
        var fromMatches = Regex.Matches(sql, @"FROM\s+([^\s,()]+)(?:\s+(?:AS\s+)?(\w+))?", RegexOptions.IgnoreCase);
        foreach (Match m in fromMatches)
        {
            var full = m.Groups[1].Value.Trim();
            var alias = m.Groups[2].Success ? m.Groups[2].Value : null;
            if (alias == null)
            {
                // if no alias, try to use the table name as its own alias
                alias = full.Contains('.') ? full.Split('.').Last() : full;
            }
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

        // 2) Find occurrences like "alias.col = $N" or "col = $N" and map param number -> column
        var condMatches = Regex.Matches(sql, @"([A-Za-z_][\w\.]*)\s*([=!<>]+\s*)?\s*\$\s*(\d+)", RegexOptions.IgnoreCase);
        foreach (Match m in condMatches)
        {
            var left = m.Groups[1].Value; // could be alias.col or col
            var paramNum = int.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture);
            if (!unknownIndexes.Contains(paramNum - 1)) continue; // our unknowns are 0-based

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
                // Unqualified column. Try single table in FROM
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
                // Query information_schema for type
                var pgType = await GetColumnPgType(conn, schema, table, column);
                if (!string.IsNullOrEmpty(pgType))
                    result[paramNum - 1] = pgType;
            }
        }

        var funcMatches = Regex.Matches(sql, @"percentile_(cont|disc)\s*\(\s*\$(\d+)", RegexOptions.IgnoreCase);
        foreach (Match m in funcMatches)
        {
            var paramNum = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
            if (unknownIndexes.Contains(paramNum - 1))
            {
                result[paramNum - 1] = "double precision";
            }
        }
        return result;
    }
    
    private static async Task<string> GetColumnPgType(NpgsqlConnection conn, string schema, string table, string column)
    {
        // default schema to public if not set
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
            var udt = rdr.GetString(0); // udt_name often gives internal type like int4, timestamptz
            var dataType = rdr.GetString(1);
            // Map udt_name to common textual PG type for our generator
            return NormalizePgTypeName(udt, dataType);
        }

        return null;
    }
    
    private static string NormalizePgTypeName(string udtName, string dataType)
    {
        // Simple normalization
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

    private static string GenerateRandomLiteralForPgType(string pgType)
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
                return ((long)rnd.NextInt64(1, 1_000_000_000)).ToString(CultureInfo.InvariantCulture);

            case "smallint":
            case "int2":
                return ((short)rnd.Next(1, 30000)).ToString(CultureInfo.InvariantCulture);

            case "real":
            case "float4":
            case "double precision":
            case "float8":
                return (rnd.NextDouble() * 1000).ToString("G", CultureInfo.InvariantCulture);

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
                return $"'{{\"k\":\"{Guid.NewGuid().ToString()[..6]}\"}}'::jsonb";

            case "text":
            case "character varying":
            case "varchar":
            case "character":
            case "char":
            default:
                // escape single quotes
                var s = Guid.NewGuid().ToString("N").Substring(0, 10);
                var escaped = s.Replace("'", "''");
                return $"'{escaped}'";
        }
    }
}

public record SubstituteValuesToSqlRequestDto(Guid DbConnectionId, string Sql);