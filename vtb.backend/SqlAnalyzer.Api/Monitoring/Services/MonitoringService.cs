using Npgsql;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Entities.DbConnection;
using SqlAnalyzer.Api.Dal.Entities.Monitoring;
using SqlAnalyzer.Api.Dal.Extensions;
using SqlAnalyzer.Api.Dal.Helpers;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;
using SqlAnalyzer.Api.Services.DbConnection;

namespace SqlAnalyzer.Api.Monitoring.Services;

/// <inheritdoc />
internal class MonitoringService : IMonitoringService
{
    private readonly ILogger<MonitoringService> _logger;
    private readonly DataContext _db;

    // Конструктор для получения конфигурации и логгера
    public MonitoringService(ILogger<MonitoringService> logger, DataContext db)
    {
        _logger = logger;
        _db = db;
    }

    /// <inheritdoc />
    public async Task<bool> SaveTempFilesMetricsAsync(DbConnection monitoringConnectionString)
    {
        try
        {
            var connectionString = monitoringConnectionString.GetConnectionString();
            await using var targetConn = new NpgsqlConnection(connectionString);
            await targetConn.OpenAsync();

            // Получаем текущие значения из целевой БД
            var currentStats = await GetCurrentTempFilesStatsAsync(targetConn);

            // Сохраняем в мониторинговую БД
            await _db.TempFilesStats.AddAsync(new TempFilesStatsDal
            {
                Id = Guid.NewGuid(),
                TempFiles = currentStats.tempFiles,
                TempBytes = currentStats.tempBytes,
                DbConnectionId = monitoringConnectionString.Id
            });
            await _db.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении метрик временных файлов");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SaveCacheHitMetricsAsync(DbConnection monitoringConnectionString)
    {
        try
        {
            var connectionString = monitoringConnectionString.GetConnectionString();
            await using var targetConn = new NpgsqlConnection(connectionString);
            await targetConn.OpenAsync();

            // Получаем текущие значения кэша
            var cacheHitStats = await GetCacheHitStats(targetConn);

            // Сохраняем в мониторинговую БД
            await SaveCacheHitStatsToMonitoringDb(cacheHitStats, monitoringConnectionString.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении метрик кэша");
            return false;
        }
    }

    private async Task<CacheHitStats> GetCacheHitStats(NpgsqlConnection connection)
    {
        var query = @"
                SELECT 
                    blks_hit, 
                    blks_read,
                    CASE (blks_hit + blks_read)
                        WHEN 0 THEN 0
                        ELSE ROUND((blks_hit * 100.0) / (blks_hit + blks_read), 2)
                    END AS cache_hit_ratio
                FROM pg_stat_database 
                WHERE datname = current_database()";

        await using var cmd = new NpgsqlCommand(query, connection);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new CacheHitStats
            {
                Id = Guid.NewGuid(),
                BlksHit = reader.GetInt64(0),
                BlksRead = reader.GetInt64(1),
                CacheHitRatio = reader.GetDecimal(2),
            };
        }

        return new CacheHitStats();
    }
    
    public async Task SaveTableStatisticsListAsync(DbConnection monitoringConnectionString)
    {
        var connectionString = monitoringConnectionString.GetConnectionString();
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        var results = new List<TableStatictics>();
        await using var command = new NpgsqlCommand(@"SELECT
    schemaname AS schema_name,
    relname AS table_name,
    seq_scan AS count_seq_scan,
    seq_tup_read AS tuples_read_seq_scan,
    idx_scan AS count_index_scan,
    idx_tup_fetch AS tuples_fetched_index_scan,
    -- Важное соотношение: сколько раз использовали индекс vs. сканировали таблицу
    CASE WHEN seq_scan > 0
        THEN (idx_scan::float / (seq_scan + idx_scan)) * 100
        ELSE 100
    END AS index_usage_ratio
FROM pg_stat_user_tables
-- Исключаем маленькие таблицы, где Seq Scan - это норма
WHERE seq_scan + idx_scan > 1000
ORDER BY seq_tup_read DESC;", connection);
        
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new TableStatictics
            {
                Id = Guid.NewGuid(),
                SchemaName = reader.GetString(0),
                TableName = reader.GetString(1),
                CountSeqScan = reader.GetInt64(2),
                TuplesReadCountSeqScan = reader.GetInt64(3),
                IndexCountSeqScan = reader.GetInt64(4),
                TuplesFetchedIndexScan = reader.GetInt64(5),
                IndexUsageRatio = reader.GetDecimal(6),
                CreateAt = DateTime.UtcNow,
                DbConnectionId = monitoringConnectionString.Id
            });
        }
        
        await _db.TableStatictics.AddRangeAsync(results);
        await _db.SaveChangesAsync();
    }

    public async Task SaveEfficiencyIndexListAsync(DbConnection connectionString)
    {
        var connectionS = connectionString.GetConnectionString();
        await using var connection = new NpgsqlConnection(connectionS);
        await connection.OpenAsync();
        var results = new List<IndexMetric>();
        await using var command = new NpgsqlCommand(@"SELECT
    s.schemaname AS schema_name,
    s.relname AS table_name,
    s.indexrelname AS index_name,
    s.idx_scan AS index_scans,
    s.idx_tup_read AS tuples_read,
    s.idx_tup_fetch AS tuples_fetched,
    -- Ключевая метрика: сколько строк было прочитано по индексу, чтобы вернуть одну?
    CASE WHEN s.idx_tup_read > 0
        THEN (s.idx_tup_fetch::float / s.idx_tup_read) * 100
        ELSE 0
    END AS index_efficiency,
    pg_size_pretty(pg_relation_size(s.indexrelid)) AS index_size
FROM pg_stat_user_indexes s
WHERE 
    s.idx_scan > 0 -- Смотрим только на используемые индексы
ORDER BY index_efficiency ASC;", connection);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new IndexMetric
            {
                SchemaName = reader.GetString(0),
                TableName = reader.GetString(1),
                IndexName = reader.GetString(2),
                IndexScans = reader.GetInt64(3),
                TuplesRead = reader.GetInt64(4),
                TuplesFetched = reader.GetInt64(5),
                Efficiency = reader.GetDouble(6),
                IndexSize = PostgresSizeConverter.ParsePostgresSizeToBytes(reader.GetString(7)),
                DbConnectionId = connectionString.Id
            });
        }

        await _db.IndexMetrics.AddRangeAsync(results);
        await _db.SaveChangesAsync();
    }

    private async Task SaveCacheHitStatsToMonitoringDb(CacheHitStats stats, Guid dbConnectionId)
    {
        stats.DbConnectionId = dbConnectionId;
        _db.CacheHitStats.Add(stats);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Получение текущего состояния бд
    /// </summary>
    /// <param name="connection">Строка подключения к бд, которую мониторим</param>
    /// <returns></returns>
    private async Task<(long tempFiles, long tempBytes)> GetCurrentTempFilesStatsAsync(NpgsqlConnection connection)
    {
        var query = @"
                SELECT 
                    temp_files, 
                    temp_bytes 
                FROM pg_stat_database 
                WHERE datname = current_database()";

        await using (var cmd = new NpgsqlCommand(query, connection))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                var tempFiles = reader.GetInt64(0);
                var tempBytes = reader.GetInt64(1);
                return (tempFiles, tempBytes);
            }
        }

        return (0, 0);
    }
}