namespace SqlAnalyzer.Api.Monitoring.Services.Interfaces;

/// <summary>
/// Сервис для мониторинга состояния бд
/// </summary>
public interface IMonitoringService
{
    Task<bool> SaveTempFilesMetricsAsync();
}