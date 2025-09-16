using System.Text.RegularExpressions;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование BETWEEN в условиях, где возможны NULL,
/// что может привести к неожиданным результатам.
/// </summary>
public sealed class BetweenWithNullsRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.BetweenWithNulls;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Safety;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"\bBETWEEN\b", RegexOptions.IgnoreCase) &&
            query.Text.Contains("NULL", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "Используется BETWEEN c возможными NULL — результат может быть непредсказуемым.",
                Category,
                DefaultSeverity,
                new List<string> { "BETWEEN" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
