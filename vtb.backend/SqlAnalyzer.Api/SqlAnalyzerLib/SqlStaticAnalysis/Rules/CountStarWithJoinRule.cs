using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование COUNT(*) в запросах с JOIN, что может приводить к завышенному результату
/// из-за дублирования строк.
/// </summary>
public sealed class CountStarWithJoinRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.CountStarWithJoin;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Safety;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("COUNT(*)") && sql.Contains("JOIN"))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "COUNT(*) с JOIN может возвращать завышенное количество строк из-за дубликатов. Рассмотрите COUNT(DISTINCT ...).",
                Category,
                DefaultSeverity,
                new List<string> { "COUNT(*)" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
