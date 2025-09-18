using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование множества условий OR, которые могут приводить к полному сканированию.
/// Рекомендуется заменить на IN или объединение запросов.
/// </summary>
public sealed class MultipleOrConditionsRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRules Code => StaticRules.MultipleOrConditions;
    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var count = Regex.Matches(query.Text, @"\bOR\b", RegexOptions.IgnoreCase).Count;

        if (count >= 3)
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.MultipleOrConditionsProblemDescription,
                StaticRuleRecommendations.MultipleOrConditionsRecommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
