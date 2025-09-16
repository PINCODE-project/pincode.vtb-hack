using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование LIMIT без ORDER BY, что делает результат непредсказуемым
/// и может возвращать разные строки при каждом выполнении.
/// </summary>
public sealed class LimitWithoutOrderByRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.LimitWithoutOrderBy;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Safety;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("LIMIT") && !sql.Contains("ORDER BY"))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "Используется LIMIT без ORDER BY — результат выборки может быть непредсказуемым.",
                Category,
                DefaultSeverity,
                new List<string> { "LIMIT" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
