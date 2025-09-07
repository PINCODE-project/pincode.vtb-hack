using SqlAnalyzer.Api.Dto.QueryAnalysis;
using SqlAnalyzerLib.Recommendation.Models;

namespace SqlAnalyzer.Api.Services.QueryAnalysis.Interfaces;

public interface IQueryAnalysisService
{
    Task<IReadOnlyCollection<Recommendation>> AnalyzeAsync(QueryAnalysisDto request);
}