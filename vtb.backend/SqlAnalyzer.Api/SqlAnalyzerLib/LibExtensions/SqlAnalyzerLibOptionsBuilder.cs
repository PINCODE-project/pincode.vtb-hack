using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Rules;
using SqlAnalyzerLib.Recommendation;
using SqlAnalyzerLib.Recommendation.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Rules;

namespace SqlAnalyzerLib.LibExtensions;

public class SqlAnalyzerLibOptionsBuilder
{
    private readonly SqlAnalyzerLibOptions _options = new();

    public SqlAnalyzerLibOptionsBuilder WithAllSqlStaticAnalyzerRules()
    {
        var staticRules = new IStaticRule[]
        {
            new FunctionOnColumnRule(),
            new LeadingWildcardLikeRule(),
            new SelectStarRule(),
            new OffsetPaginationRule(),
            new TypeMismatchComparisonRule(),
            new NotInNullsRule(),
            new CartesianJoinRule(),
            new NonSargableExpressionRule(),
            new SubqueryInsteadOfJoinRule(),
            new MissingWhereDeleteRule()
        };

        _options.SqlStaticAnalysisRules = staticRules;

        return this;
    }
    
    public SqlAnalyzerLibOptionsBuilder WithAllExplainAnalyzerRules()
    {
        var planRules = new IPlanRule[]
        {
            new SeqScanSelectiveRule(removedFractionThreshold: 0.3),
            new SortExternalRule(),
            new TempFilesRule(),
            new CardinalityMismatchRule(),
            new HashSpillRule(),
            new IndexFilterMismatchRule(),
            new IndexOnlyHeapFetchRule(),
            new ParallelismRule(),
            new NestedLoopHeavyInnerRule()
        };

        _options.ExplainAnalysisRules = planRules;

        return this;
    }
    
    public SqlAnalyzerLibOptionsBuilder WithAllRecommendationProviders()
    {
        var recommendationProviders = new IRecommendationProvider[]
        {
            new StaticQueryRecommendationProvider(),
            new ExplainRecommendationProvider()
        };

        _options.RecommendationProviders = recommendationProviders;

        return this;
    }

    public SqlAnalyzerLibOptions Build()
    {
        return _options;
    }
}