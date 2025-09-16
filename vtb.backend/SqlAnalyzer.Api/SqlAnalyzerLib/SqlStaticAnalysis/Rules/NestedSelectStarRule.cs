using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование SELECT * внутри подзапросов, что приводит к неоптимальному плану
/// и усложняет индексацию и анализ.
/// </summary>
public sealed class NestedSelectStarRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.NestedSelectStar;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Rewrite;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("(SELECT *"))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "Используется SELECT * внутри подзапроса — укажите только необходимые колонки.",
                Category,
                DefaultSeverity,
                new List<string> { "*" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
