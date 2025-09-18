using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверка Cartesian join: наличие перечисления таблиц в FROM через запятую без явного JOIN.
/// Дает предупреждение, если используется синтаксис ',', отсутствуют JOIN и очевидные ON/USING условия.
/// </summary>
public sealed class CartesianJoinRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRules Code => StaticRules.CartesianJoin;

    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    private static readonly Regex FromCommaPattern = new(@"\bFROM\s+[^;]+,", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex JoinKeyword = new(@"\bJOIN\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex WhereEqualityBetweenAliases = new(@"\bWHERE\b[\s\S]*\w+\.\w+\s*=\s*\w+\.\w+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (FromCommaPattern.IsMatch(query.Text) && !JoinKeyword.IsMatch(query.Text) && !WhereEqualityBetweenAliases.IsMatch(query.Text))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.CartesianJoinProblemDescription,
                StaticRuleRecommendations.CartesianJoinRecommendation
            ));
        }
        
        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}