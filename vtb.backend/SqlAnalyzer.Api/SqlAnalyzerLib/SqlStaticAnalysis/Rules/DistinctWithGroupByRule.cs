using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование DISTINCT вместе с GROUP BY, что является избыточным и не влияет на результат.
/// </summary>
public sealed class DistinctWithGroupByRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.DistinctWithGroupBy;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Rewrite;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Low;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("DISTINCT") && sql.Contains("GROUP BY"))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "Используется DISTINCT вместе с GROUP BY — DISTINCT лишний и может быть удалён.",
                Category,
                DefaultSeverity,
                new List<string> { "DISTINCT", "GROUP BY" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
