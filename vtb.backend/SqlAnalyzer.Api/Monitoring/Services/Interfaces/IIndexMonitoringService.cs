namespace SqlAnalyzer.Api.Monitoring.Services.Interfaces;

public interface IIndexMonitoringService
{
    Task CollectIndexStatisticsAsync(string connectionString);
}