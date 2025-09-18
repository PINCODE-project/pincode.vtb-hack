using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверка несажабельных выражений (non-sargable) — арифметика/операции на колонках.
/// Детектирует выражения вида col + 1, col - interval '1 day', col * 2 и подобное.
/// </summary>
public sealed class NonSargableExpressionRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRules Code => StaticRules.NonSargableExpression;

    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    private static readonly Regex ArithmeticOnColumn = new(@"\b\w+\.\w+\s*[\+\-\*\/%]\s*[\w'\""]|\b\w+\s*[\+\-\*\/%]\s*['""]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex IntervalArithmetic = new(@"\b\w+\s*[\+|\-]\s*INTERVAL\s+'[^']+'", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (ArithmeticOnColumn.IsMatch(query.Text) || IntervalArithmetic.IsMatch(query.Text))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.NonSargableExpressionProblemDescription,
                StaticRuleRecommendations.NonSargableExpressionRecommendation
            ));
        }
        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}