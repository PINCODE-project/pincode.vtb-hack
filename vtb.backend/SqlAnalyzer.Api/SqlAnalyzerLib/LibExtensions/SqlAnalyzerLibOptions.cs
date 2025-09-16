using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.Recommendation.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;

namespace SqlAnalyzerLib.LibExtensions;

public class SqlAnalyzerLibOptions
{
    public IReadOnlyCollection<Type> SqlStaticAnalysisRules { get; set; } = new List<Type>();

    public IReadOnlyCollection<Type> ExplainAnalysisRules { get; set; } = new List<Type>();

    public IReadOnlyCollection<Type> RecommendationProviders { get; set; } = new List<Type>();
}