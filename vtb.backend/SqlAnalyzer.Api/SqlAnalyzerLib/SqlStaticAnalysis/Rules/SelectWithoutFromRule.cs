using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
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
    public StaticRules Code => StaticRules.SelectWithoutFrom;
 
    /// <inheritdoc />
    public Severity Severity => Severity.Info;

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"SELECT\s+\d+\s*;?", RegexOptions.IgnoreCase) &&
            !query.Text.Contains("FROM", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.SelectWithoutFromProblemDescription,
                StaticRuleRecommendations.SelectWithoutFromRecommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
