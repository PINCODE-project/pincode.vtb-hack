using System.Text.RegularExpressions;
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
    public StaticRuleCodes Code => StaticRuleCodes.CartesianJoin;

    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Safety;

    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    private static readonly Regex FromCommaPattern = new(@"\bFROM\s+[^;]+,", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex JoinKeyword = new(@"\bJOIN\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex WhereEqualityBetweenAliases = new(@"\bWHERE\b[\s\S]*\w+\.\w+\s*=\s*\w+\.\w+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (FromCommaPattern.IsMatch(query.Text) && !JoinKeyword.IsMatch(query.Text) && !WhereEqualityBetweenAliases.IsMatch(query.Text))
        {
            var msg = "В FROM используется перечисление таблиц через запятую без JOIN/ON — возможно Cartesian product. Проверьте наличие условий соединения (ON/WHERE) или используйте явные JOIN.";
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(Code, msg, Category, DefaultSeverity, new List<string>()));
        }
        return Task.FromResult<StaticCheckFinding?>(null);
    }
}