using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

public sealed class ExistsVsInRule : IStaticRule
{
    public StaticRuleCodes Code => StaticRuleCodes.ExistsVsIn;
    public RecommendationCategory Category => RecommendationCategory.Rewrite;
    public Severity DefaultSeverity => Severity.Low;

    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("IN (SELECT"))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "Используется IN с подзапросом — рассмотрите замену на EXISTS для больших наборов данных.",
                Category,
                DefaultSeverity,
                new List<string> { "IN-subquery" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
