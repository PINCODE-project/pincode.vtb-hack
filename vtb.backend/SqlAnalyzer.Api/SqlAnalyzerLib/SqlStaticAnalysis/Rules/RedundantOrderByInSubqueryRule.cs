using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

public sealed class RedundantOrderByInSubqueryRule : IStaticRule
{
    public StaticRules Code => StaticRules.RedundantOrderByInSubquery;

    public Severity Severity => Severity.Info;

    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("(SELECT") && sql.Contains("ORDER BY") && !sql.Contains("LIMIT"))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.RedundantOrderByInSubqueryProblemDescription,
                StaticRuleRecommendations.RedundantOrderByInSubqueryRecommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
