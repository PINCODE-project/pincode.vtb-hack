using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

public sealed class NonIndexedJoinRule : IStaticRule
{
    public StaticRuleCodes Code => StaticRuleCodes.NonIndexedJoin;
    public RecommendationCategory Category => RecommendationCategory.Index;
    public Severity DefaultSeverity => Severity.Medium;

    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("JOIN ON") && !sql.Contains("ID"))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "JOIN выполняется не по ID — убедитесь, что в условии есть индексируемый столбец.",
                Category,
                DefaultSeverity,
                new List<string> { "JOIN condition" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
