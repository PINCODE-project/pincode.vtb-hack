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
    Task<bool> SaveTempFilesMetricsAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<bool> SaveCacheHitMetricsAsync();
}