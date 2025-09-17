namespace SqlAnalyzer.Api.Monitoring.Services.Interfaces;

public interface ILockAnalyzer
{
    Task<LockAnalysisResult> AnalyzeLocksAsync(Guid dbConnectionId, DateTime periodStart, DateTime periodEnd);
}