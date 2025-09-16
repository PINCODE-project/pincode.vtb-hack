using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

public sealed class GroupByWithoutHavingOrAggregateRule : IStaticRule
{
    public StaticRuleCodes Code => StaticRuleCodes.GroupByWithoutHavingOrAggregate;
    public RecommendationCategory Category => RecommendationCategory.Correctness;
    public Severity DefaultSeverity => Severity.Medium;

    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("GROUP BY") && !sql.Contains("COUNT(") &&
            !sql.Contains("SUM(") && !sql.Contains("AVG(") &&
            !sql.Contains("MIN(") && !sql.Contains("MAX("))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "GROUP BY используется без агрегатных функций — вероятно, ошибка или лишнее.",
                Category,
                DefaultSeverity,
                Array.Empty<string>()
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
