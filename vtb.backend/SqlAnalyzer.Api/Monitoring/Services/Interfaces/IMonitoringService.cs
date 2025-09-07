namespace SqlAnalyzer.Api.Monitoring.Services.Interfaces;

/// <summary>
/// Сервис для мониторинга состояния бд
/// </summary>
public interface IMonitoringService
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<bool> SaveTempFilesMetricsAsync(string monitoringConnectionString);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<bool> SaveCacheHitMetricsAsync(string monitoringConnectionString);

    /// <summary>
    /// Сохранить метрики таблиц
    /// </summary>
    Task SaveTableStatisticsListAsync(string monitoringConnectionString);

    /// <summary>
    /// сохранить статистику индексов по таблицам
    /// </summary>
    Task SaveEfficiencyIndexListAsync(string connectionString);
}