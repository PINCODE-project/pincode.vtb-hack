using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

public sealed class ImplicitCrossJoinRule : IStaticRule
{
    public StaticRuleCodes Code => StaticRuleCodes.ImplicitCrossJoin;
    public RecommendationCategory Category => RecommendationCategory.Correctness;
    public Severity DefaultSeverity => Severity.High;

    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("FROM") && sql.Contains(",") && !sql.Contains("JOIN"))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "Используется запятая в FROM — это неявный CROSS JOIN, лучше использовать явный JOIN.",
                Category,
                DefaultSeverity,
                Array.Empty<string>()
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
