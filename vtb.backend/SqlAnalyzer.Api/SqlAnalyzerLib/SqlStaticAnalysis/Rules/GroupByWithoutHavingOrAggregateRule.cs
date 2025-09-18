using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// 
/// </summary>
public sealed class GroupByWithoutHavingOrAggregateRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRules Code => StaticRules.GroupByWithoutHavingOrAggregate;

    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("GROUP BY") && !sql.Contains("COUNT(") &&
            !sql.Contains("SUM(") && !sql.Contains("AVG(") &&
            !sql.Contains("MIN(") && !sql.Contains("MAX("))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.GroupByWithoutHavingOrAggregateProblemDescription,
                StaticRuleRecommendations.GroupByWithoutHavingOrAggregateRecommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
