using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет, используется ли GROUP BY без HAVING для фильтрации агрегатов,
/// что может означать избыточную агрегацию и неоптимальный запрос.
/// </summary>
public sealed class GroupByWithoutHavingRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.GroupByWithoutHaving;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Performance;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Low;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("GROUP BY") && !sql.Contains("HAVING"))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "Используется GROUP BY без HAVING — возможно, агрегация выполняется лишний раз.",
                Category,
                DefaultSeverity,
                new List<string> { "GROUP BY" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
