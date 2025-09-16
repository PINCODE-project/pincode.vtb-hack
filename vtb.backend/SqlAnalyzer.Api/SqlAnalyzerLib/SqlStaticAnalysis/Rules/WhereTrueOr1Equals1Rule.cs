using System.Text.RegularExpressions;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет наличие условий вида WHERE TRUE или WHERE 1=1,
/// которые избыточны и могут скрывать потенциальные ошибки в логике.
/// </summary>
public sealed class WhereTrueOr1Equals1Rule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.WhereTrueOr1Equals1;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Safety;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Low;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"WHERE\s+(TRUE|1\s*=\s*1)", RegexOptions.IgnoreCase))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "Условие WHERE TRUE или WHERE 1=1 не имеет смысла и может быть ошибкой.",
                Category,
                DefaultSeverity,
                new List<string> { "WHERE TRUE", "WHERE 1=1" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
