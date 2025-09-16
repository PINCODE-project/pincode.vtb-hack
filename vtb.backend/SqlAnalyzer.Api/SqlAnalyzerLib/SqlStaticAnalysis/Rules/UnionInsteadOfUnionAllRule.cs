using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

public sealed class UnionInsteadOfUnionAllRule : IStaticRule
{
    public StaticRuleCodes Code => StaticRuleCodes.UnionInsteadOfUnionAll;
    public RecommendationCategory Category => RecommendationCategory.Performance;
    public Severity DefaultSeverity => Severity.Medium;

    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("UNION") && !sql.Contains("UNION ALL"))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "Используется UNION без ALL — лишнее удаление дублей может замедлять запрос.",
                Category,
                DefaultSeverity,
                Array.Empty<string>()
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
