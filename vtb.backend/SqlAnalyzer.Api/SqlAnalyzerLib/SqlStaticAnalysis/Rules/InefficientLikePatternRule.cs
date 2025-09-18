using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование шаблонов LIKE, которые не могут использовать индекс (например, %abc%).
/// </summary>
public sealed class InefficientLikePatternRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRules Code => StaticRules.InefficientLikePattern;
    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"LIKE\s+'%.*%'", RegexOptions.IgnoreCase))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.InefficientLikePatternProblemDescription,
                StaticRuleRecommendations.InefficientLikePatternRecommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
