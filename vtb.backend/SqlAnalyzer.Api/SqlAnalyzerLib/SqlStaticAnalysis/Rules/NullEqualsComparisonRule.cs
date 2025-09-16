using System.Text.RegularExpressions;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет наличие сравнений с NULL через = или !=,
/// что всегда возвращает UNKNOWN и может быть логической ошибкой.
/// </summary>
public sealed class NullEqualsComparisonRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.NullEqualsComparison;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Safety;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"=\s*NULL|!=\s*NULL", RegexOptions.IgnoreCase))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "Сравнение с NULL через = или != некорректно. Используйте IS NULL / IS NOT NULL.",
                Category,
                DefaultSeverity,
                new List<string> { "NULL comparison" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
