using SqlAnalyzerLib.Recommendation.Models.Explain;
using SqlAnalyzerLib.Recommendation.Models.Query;

namespace SqlAnalyzerLib.Recommendation.Interfaces;

using Models;

/// <summary>
/// Контракт для модуля построения рекомендаций
/// </summary>
public interface IRecommendationProvider
{
    IEnumerable<Recommendation> BuildRecommendations(QueryAnalysisResult analysisResult);
    IEnumerable<Recommendation> BuildRecommendations(ExplainAnalysisResult analysisResult);
}