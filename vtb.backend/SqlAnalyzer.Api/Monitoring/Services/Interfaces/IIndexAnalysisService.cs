namespace SqlAnalyzer.Api.Monitoring.Services.Interfaces;

public interface IIndexAnalysisService
{
    Task<IndexAnalysisResult> GetFullAnalysisAsync(Guid dbConnectionId, DateTime periodStart, DateTime periodEnd);
}