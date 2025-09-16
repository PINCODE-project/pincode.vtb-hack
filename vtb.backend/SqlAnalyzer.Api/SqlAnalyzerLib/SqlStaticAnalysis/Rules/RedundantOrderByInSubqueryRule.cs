using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

public sealed class RedundantOrderByInSubqueryRule : IStaticRule
{
    public StaticRuleCodes Code => StaticRuleCodes.RedundantOrderByInSubquery;
    public RecommendationCategory Category => RecommendationCategory.Performance;
    public Severity DefaultSeverity => Severity.Low;

    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("(SELECT") && sql.Contains("ORDER BY") && !sql.Contains("LIMIT"))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "ORDER BY внутри подзапроса без LIMIT не имеет смысла и может замедлять выполнение.",
                Category,
                DefaultSeverity,
                Array.Empty<string>()
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
