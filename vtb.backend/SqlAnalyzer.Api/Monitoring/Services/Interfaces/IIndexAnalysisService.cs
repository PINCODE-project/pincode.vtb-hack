namespace SqlAnalyzer.Api.Monitoring.Services.Interfaces;

public interface IIndexAnalysisService
{
    Task<IndexAnalysisReport> AnalyzeIndexesAsync(DateTime? fromDate = null, DateTime? toDate = null);
}