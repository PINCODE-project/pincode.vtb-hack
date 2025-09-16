using System.Text.RegularExpressions;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет чрезмерно сложные CTE (WITH), которые содержат вложенные подзапросы,
/// что может негативно сказаться на производительности.
/// </summary>
public sealed class OverlyComplexCteRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.OverlyComplexCte;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Performance;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"WITH\s+\w+\s+AS\s*\(\s*SELECT.*SELECT", RegexOptions.IgnoreCase))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "CTE содержит вложенные SELECT — возможно, он слишком сложен и требует упрощения.",
                Category,
                DefaultSeverity,
                new List<string> { "CTE" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
