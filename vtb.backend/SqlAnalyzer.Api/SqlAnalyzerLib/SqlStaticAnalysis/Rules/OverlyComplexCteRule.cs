using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
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
    public StaticRules Code => StaticRules.OverlyComplexCte;

    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"WITH\s+\w+\s+AS\s*\(\s*SELECT.*SELECT", RegexOptions.IgnoreCase))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.OverlyComplexCteProblemDescription,
                StaticRuleRecommendations.OverlyComplexCteRecommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
