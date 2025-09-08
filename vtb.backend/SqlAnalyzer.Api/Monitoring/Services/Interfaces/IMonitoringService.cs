using SqlAnalyzer.Api.Dal.Entities.DbConnection;

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
    Task<bool> SaveTempFilesMetricsAsync(DbConnection monitoringConnectionString);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<bool> SaveCacheHitMetricsAsync(DbConnection monitoringConnectionString);

    /// <summary>
    /// Сохранить метрики таблиц
    /// </summary>
    Task SaveTableStatisticsListAsync(DbConnection monitoringConnectionString);

    /// <summary>
    /// сохранить статистику индексов по таблицам
    /// </summary>
    Task SaveEfficiencyIndexListAsync(DbConnection connectionString);
}