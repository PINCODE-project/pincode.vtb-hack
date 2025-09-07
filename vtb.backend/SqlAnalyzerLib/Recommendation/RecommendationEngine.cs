using SqlAnalyzerLib.Recommendation.Interfaces;
using SqlAnalyzerLib.Recommendation.Models.Explain;
using SqlAnalyzerLib.Recommendation.Models.Query;

namespace SqlAnalyzerLib.Recommendation;

using Models;

/// <summary>
/// Движок построения финального списка рекомендаций по SQL-запросу
/// </summary>
public class RecommendationEngine
{
    private readonly IEnumerable<IRecommendationProvider> _providers;

    public RecommendationEngine(IEnumerable<IRecommendationProvider> providers)
    {
        _providers = providers;
    }

    public IReadOnlyCollection<Recommendation> BuildRecommendations(QueryAnalysisResult queryResult, ExplainAnalysisResult explainResult)
    {
        var recommendations = new List<Recommendation>();

        foreach (var provider in _providers)
        {
            recommendations.AddRange(provider.BuildRecommendations(queryResult));
            recommendations.AddRange(provider.BuildRecommendations(explainResult));
        }

        return recommendations.OrderByDescending(r => r.Severity).ToList();
    }
}