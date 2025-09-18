using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
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
    public StaticRules Code => StaticRules.BetweenWithNulls;
    
    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"\bBETWEEN\b", RegexOptions.IgnoreCase) &&
            query.Text.Contains("NULL", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.BetweenWithNullsProblemDescription,
                StaticRuleRecommendations.BetweenWithNullsRecommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
