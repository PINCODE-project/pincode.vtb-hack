using System.Text.RegularExpressions;
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
    public StaticRuleCodes Code => StaticRuleCodes.FunctionInJoinCondition;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Index;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"JOIN\s+\w+.*ON.*\w+\(.*\)", RegexOptions.IgnoreCase))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "В условии JOIN используются функции — индекс не будет применён.",
                Category,
                DefaultSeverity,
                new List<string> { "Function in JOIN" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
