using System.Text.RegularExpressions;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование функций на индексируемых колонках, что делает индекс бесполезным.
/// </summary>
public sealed class FunctionOnIndexColumnRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.FunctionOnIndexColumn;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Index;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"WHERE\s+\w+\s*::", RegexOptions.IgnoreCase) ||
            Regex.IsMatch(query.Text, @"WHERE\s+\w+\s*\(", RegexOptions.IgnoreCase))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "Используются функции на индексируемых колонках — индекс не будет применён.",
                Category,
                DefaultSeverity,
                new List<string> { "Function on column" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
