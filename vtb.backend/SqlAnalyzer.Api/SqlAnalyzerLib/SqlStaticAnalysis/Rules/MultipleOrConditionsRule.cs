using System.Text.RegularExpressions;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование множества условий OR, которые могут приводить к полному сканированию.
/// Рекомендуется заменить на IN или объединение запросов.
/// </summary>
public sealed class MultipleOrConditionsRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.MultipleOrConditions;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Performance;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var count = Regex.Matches(query.Text, @"\bOR\b", RegexOptions.IgnoreCase).Count;

        if (count >= 3)
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                $"Обнаружено {count} условий OR — это может замедлить выполнение. Рассмотрите IN или UNION.",
                Category,
                DefaultSeverity,
                new List<string> { "OR" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
