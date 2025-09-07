using System.Text.RegularExpressions;
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
    public StaticRuleCodes Code => StaticRuleCodes.NonSargableExpression;

    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Index;

    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    private static readonly Regex ArithmeticOnColumn = new(@"\b\w+\.\w+\s*[\+\-\*\/%]\s*[\w'\""]|\b\w+\s*[\+\-\*\/%]\s*['""]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex IntervalArithmetic = new(@"\b\w+\s*[\+|\-]\s*INTERVAL\s+'[^']+'", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (ArithmeticOnColumn.IsMatch(query.Text) || IntervalArithmetic.IsMatch(query.Text))
        {
            var msg = "В выражении используются арифметические операции над колонками (например, col + 1), что делает предикат non-sargable. Рассмотрите рефакторинг: вычисляемые столбцы/индексы по выражению или переписать условие.";
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(Code, msg, Category, DefaultSeverity, new List<string>()));
        }
        return Task.FromResult<StaticCheckFinding?>(null);
    }
}