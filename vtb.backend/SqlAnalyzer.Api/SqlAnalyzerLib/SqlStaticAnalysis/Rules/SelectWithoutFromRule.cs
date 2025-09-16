using System.Text.RegularExpressions;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет запросы вида SELECT без FROM (например, SELECT 1),
/// которые могут быть артефактами отладки или не иметь смысла в продакшене.
/// </summary>
public sealed class SelectWithoutFromRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.SelectWithoutFrom;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Safety;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Low;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"SELECT\s+\d+\s*;?", RegexOptions.IgnoreCase) &&
            !query.Text.Contains("FROM", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "Обнаружен SELECT без FROM — вероятно, это артефакт отладки.",
                Category,
                DefaultSeverity,
                new List<string> { "SELECT without FROM" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
