using System.Text.RegularExpressions;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет наличие JOIN с условием неравенства (<> или !=),
/// что может приводить к полному сканированию и картезианскому произведению.
/// </summary>
public sealed class JoinOnInequalityRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.JoinOnInequality;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Performance;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"JOIN\s+\w+.*(<>|!=)", RegexOptions.IgnoreCase))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "JOIN выполняется по условию неравенства, что может привести к полному сканированию.",
                Category,
                DefaultSeverity,
                new List<string> { "JOIN inequality" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
