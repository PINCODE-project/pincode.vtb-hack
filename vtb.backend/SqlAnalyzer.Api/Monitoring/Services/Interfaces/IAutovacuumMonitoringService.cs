using SqlAnalyzer.Api.Dal.Entities.DbConnection;

namespace SqlAnalyzer.Api.Monitoring.Services.Interfaces;

public interface IAutovacuumMonitoringService
{
    Task<bool> SaveAutovacuumMetricsAsync(DbConnection connectionString);
}