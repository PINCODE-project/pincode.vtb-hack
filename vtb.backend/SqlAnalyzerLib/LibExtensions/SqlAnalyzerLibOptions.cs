using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.Recommendation.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;

namespace SqlAnalyzerLib.LibExtensions;

public class SqlAnalyzerLibOptions
{
    public IReadOnlyCollection<IStaticRule> SqlStaticAnalysisRules { get; set; } = new List<IStaticRule>();

    public IReadOnlyCollection<IPlanRule> ExplainAnalysisRules { get; set; } = new List<IPlanRule>();

    public IReadOnlyCollection<IRecommendationProvider> RecommendationProviders { get; set; } = new List<IRecommendationProvider>();
}