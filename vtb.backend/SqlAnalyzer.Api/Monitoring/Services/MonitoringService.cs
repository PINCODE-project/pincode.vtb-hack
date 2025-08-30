using Npgsql;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

namespace SqlAnalyzer.Api.Monitoring.Services;

/// <inheritdoc />
internal class MonitoringService : IMonitoringService
{
    private readonly ILogger<MonitoringService> _logger;

    // Конструктор для получения конфигурации и логгера
    public MonitoringService(ILogger<MonitoringService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> SaveTempFilesMetricsAsync()
    {
        // TODO перенести в конфиг
        // Строка подключения к мониторинговой БД
        var monitoringConnectionString = "Host=localhost;Username=postgres;Password=1;Database=monitoring";
        // Строка подключения к целевой БД
        var targetConnectionString = "Host=localhost;Username=postgres;Password=1;Database=tenant";

        try
        {
            await using var targetConn = new NpgsqlConnection(targetConnectionString);
            await using var monitoringConn = new NpgsqlConnection(monitoringConnectionString);
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
        // TODO перенести в конфиг
        // Строка подключения к мониторинговой БД
        var monitoringConnectionString = "Host=localhost;Username=postgres;Password=1;Database=monitoring";
        // Строка подключения к целевой БД
        var targetConnectionString = "Host=localhost;Username=postgres;Password=1;Database=tenant";

        try
        {
            await using var targetConn = new NpgsqlConnection(targetConnectionString);
            await using var monitoringConn = new NpgsqlConnection(monitoringConnectionString);
            await targetConn.OpenAsync();
            await monitoringConn.OpenAsync();

            // Получаем текущие значения кэша
            var cacheStats = await GetCacheStats(targetConn);

            // Сохраняем в мониторинговую БД
            await SaveCacheStatsToMonitoringDb(monitoringConn, cacheStats);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении метрик кэша");
            return false;
        }
    }

    private async Task<CacheStats> GetCacheStats(NpgsqlConnection connection)
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
            return new CacheStats
            {
                BlksHit = reader.GetInt64(0),
                BlksRead = reader.GetInt64(1),
                CacheHitRatio = reader.GetDecimal(2)
            };
        }

        return new CacheStats();
    }

    private async Task SaveCacheStatsToMonitoringDb(NpgsqlConnection connection, CacheStats stats)
    {
        var query = @"
                INSERT INTO cache_hit_stats (
                    measurement_time, 
                    blks_hit, 
                    blks_read,
                    cache_hit_ratio
                ) VALUES (
                    @measurement_time, 
                    @blks_hit, 
                    @blks_read,
                    @cache_hit_ratio
                )";

        await using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("measurement_time", DateTime.UtcNow);
        cmd.Parameters.AddWithValue("blks_hit", stats.BlksHit);
        cmd.Parameters.AddWithValue("blks_read", stats.BlksRead);
        cmd.Parameters.AddWithValue("cache_hit_ratio", stats.CacheHitRatio);

        await cmd.ExecuteNonQueryAsync();
    }

    public class CacheStats
    {
        public long BlksHit { get; set; }
        public long BlksRead { get; set; }
        public decimal CacheHitRatio { get; set; }
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