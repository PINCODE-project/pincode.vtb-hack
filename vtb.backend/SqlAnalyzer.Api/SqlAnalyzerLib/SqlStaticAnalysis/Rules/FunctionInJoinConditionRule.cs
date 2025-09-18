using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование функций в условии JOIN,
/// что делает невозможным использование индекса.
/// </summary>
public sealed class FunctionInJoinConditionRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRules Code => StaticRules.FunctionInJoinCondition;

    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"JOIN\s+\w+.*ON.*\w+\(.*\)", RegexOptions.IgnoreCase))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.FunctionInJoinConditionProblemDescription,
                StaticRuleRecommendations.FunctionInJoinConditionRecommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
