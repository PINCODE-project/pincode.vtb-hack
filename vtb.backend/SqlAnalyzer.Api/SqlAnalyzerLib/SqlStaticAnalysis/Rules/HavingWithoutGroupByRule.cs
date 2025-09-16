using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование HAVING без GROUP BY.
/// Такое условие избыточно и может быть заменено на WHERE.
/// </summary>
public sealed class HavingWithoutGroupByRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.HavingWithoutGroupBy;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Rewrite;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Low;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("HAVING") && !sql.Contains("GROUP BY"))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "Используется HAVING без GROUP BY — условие избыточно и может быть вынесено в WHERE.",
                Category,
                DefaultSeverity,
                new List<string> { "HAVING" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
