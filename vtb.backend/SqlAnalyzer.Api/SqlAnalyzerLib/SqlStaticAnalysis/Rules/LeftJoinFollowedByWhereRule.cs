using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование LEFT JOIN с условием в WHERE, что фактически превращает его в INNER JOIN.
/// </summary>
public sealed class LeftJoinFollowedByWhereRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.LeftJoinFollowedByWhere;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Rewrite;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("LEFT JOIN") && sql.Contains(" WHERE ") && sql.Contains(" = "))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "LEFT JOIN используется вместе с условием в WHERE, что эквивалентно INNER JOIN.",
                Category,
                DefaultSeverity,
                new List<string> { "LEFT JOIN" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
