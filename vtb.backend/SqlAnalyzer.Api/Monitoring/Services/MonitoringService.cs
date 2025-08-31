using Npgsql;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Dal.Entities.Monitoring;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

namespace SqlAnalyzer.Api.Monitoring.Services;

/// <inheritdoc />
internal class MonitoringService : IMonitoringService
{
    private readonly ILogger<MonitoringService> _logger;
    private readonly string _monitoringConnectionString;
    private readonly string _targetConnectionString;
    private readonly DataContext _db;

    // Конструктор для получения конфигурации и логгера
    public MonitoringService(ILogger<MonitoringService> logger, IConfiguration configuration, DataContext db)
    {
        _logger = logger;
        _db = db;
        _monitoringConnectionString = configuration.GetConnectionString("DefaultConnection");
        _targetConnectionString = configuration.GetConnectionString("TargetConnection");
    }

    /// <inheritdoc />
    public async Task<bool> SaveTempFilesMetricsAsync()
    {
        try
        {
            await using var targetConn = new NpgsqlConnection(_targetConnectionString);
            await using var monitoringConn = new NpgsqlConnection(_monitoringConnectionString);
            await targetConn.OpenAsync();
            await monitoringConn.OpenAsync();

            // Получаем текущие значения из целевой БД
            var currentStats = await GetCurrentStats(targetConn);

            // Сохраняем в мониторинговую БД
            await SaveStatsToMonitoringDb(monitoringConn, currentStats);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении метрик временных файлов");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SaveCacheHitMetricsAsync()
    {
        try
        {
            await using var targetConn = new NpgsqlConnection(_targetConnectionString);
            await targetConn.OpenAsync();

            // Получаем текущие значения кэша
            var cacheHitStats = await GetCacheHitStats(targetConn);

            // Сохраняем в мониторинговую БД
            await SaveCacheHitStatsToMonitoringDb(cacheHitStats);

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

    private async Task SaveCacheHitStatsToMonitoringDb(CacheHitStats stats)
    {
        _db.CacheHitStats.Add(stats);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Получение текущего состояния бд
    /// </summary>
    /// <param name="connection">Строка подключения к бд, которую мониторим</param>
    /// <returns></returns>
    private async Task<(long tempFiles, long tempBytes)> GetCurrentStats(NpgsqlConnection connection)
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

    /// <summary>
    /// Сохранить состояние бд для отслеживания
    /// </summary>
    /// <param name="connection">Строка подключения для бд - коллектора</param>
    /// <param name="stats">собраная статистика</param>
    private async Task SaveStatsToMonitoringDb(NpgsqlConnection connection, (long tempFiles, long tempBytes) stats)
    {
        var query = @"
                INSERT INTO temp_files_stats (
                    measurement_time, 
                    temp_files, 
                    temp_bytes
                ) VALUES (
                    @measurement_time, 
                    @temp_files, 
                    @temp_bytes
                )";

        await using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("measurement_time", DateTime.UtcNow);
        cmd.Parameters.AddWithValue("temp_files", stats.tempFiles);
        cmd.Parameters.AddWithValue("temp_bytes", stats.tempBytes);

        await cmd.ExecuteNonQueryAsync();
    }
}