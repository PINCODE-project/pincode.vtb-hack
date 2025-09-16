using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

public sealed class UnnecessaryDistinctRule : IStaticRule
{
    public StaticRuleCodes Code => StaticRuleCodes.UnnecessaryDistinct;
    public RecommendationCategory Category => RecommendationCategory.Performance;
    public Severity DefaultSeverity => Severity.Low;

    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("SELECT DISTINCT") && !sql.Contains("JOIN") && !sql.Contains("GROUP BY"))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "DISTINCT может быть избыточным — в запросе нет JOIN или GROUP BY.",
                Category,
                DefaultSeverity,
                Array.Empty<string>()
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
