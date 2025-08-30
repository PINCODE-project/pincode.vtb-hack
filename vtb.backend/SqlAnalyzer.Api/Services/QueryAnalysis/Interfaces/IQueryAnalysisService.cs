using SqlAnalyzer.Api.Dto.QueryAnalysis;

namespace SqlAnalyzer.Api.Services.QueryAnalysis.Interfaces;

public interface IQueryAnalysisService
{
    Task<QueryAnalysisResultDto> AnalyzeAsync(QueryAnalysisDto request);
}