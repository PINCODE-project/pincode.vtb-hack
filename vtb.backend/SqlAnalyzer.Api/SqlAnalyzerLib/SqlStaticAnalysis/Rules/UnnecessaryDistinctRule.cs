using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

public sealed class UnnecessaryDistinctRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRules Code => StaticRules.UnnecessaryDistinct;

    /// <inheritdoc />
    public Severity Severity => Severity.Info;

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("SELECT DISTINCT") && !sql.Contains("JOIN") && !sql.Contains("GROUP BY"))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.UnnecessaryDistinctProblemDescription,
                StaticRuleRecommendations.UnnecessaryDistinctRecommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
