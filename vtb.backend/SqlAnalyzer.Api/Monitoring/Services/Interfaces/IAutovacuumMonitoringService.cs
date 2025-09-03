namespace SqlAnalyzer.Api.Monitoring.Services.Interfaces;

public interface IAutovacuumMonitoringService
{
    Task<bool> SaveAutovacuumMetricsAsync(string connectionString);
}