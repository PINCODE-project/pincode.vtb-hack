namespace SqlAnalyzer.Api.Monitoring.Services.Interfaces;

public interface IIndexAnalysisService
{
    Task<IndexAnalysisResult> GetFullAnalysisAsync(DateTime startDate, DateTime endDate);
}