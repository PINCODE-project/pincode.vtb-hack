using SqlAnalyzer.Api.Dto.QueryAnalysis;

namespace SqlAnalyzer.Api.Services.Recomedation.Interfaces;

public interface IQueryRecommendationService
{
    Task<QueryAnalysisResultDto> GetRecommendations(Guid queryAnalysisId);
}