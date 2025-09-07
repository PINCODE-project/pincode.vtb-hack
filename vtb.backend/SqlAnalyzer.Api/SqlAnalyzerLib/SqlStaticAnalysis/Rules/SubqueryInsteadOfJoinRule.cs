using System.Text.RegularExpressions;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверка использования подзапросов IN/EXISTS в местах, где может быть предпочтителен JOIN.
/// Правило является эвристическим: предупреждает при наличии IN (SELECT ...) или EXISTS (SELECT ...).
/// </summary>
public sealed class SubqueryInsteadOfJoinRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.SubqueryInsteadOfJoin;

    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Rewrite;

    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Low;

    private static readonly Regex InSelectPattern = new(@"\bIN\s*\(\s*SELECT\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ExistsSelectPattern = new(@"\bEXISTS\s*\(\s*SELECT\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (InSelectPattern.IsMatch(query.Text))
        {
            var msg = "Найдено IN (SELECT ...). В некоторых случаях использование JOIN/LEFT JOIN экономичнее по производительности и понятнее по плану выполнения. Проверьте, можно ли переписать на JOIN.";
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(Code, msg, Category, DefaultSeverity, new List<string>()));
        }

        if (ExistsSelectPattern.IsMatch(query.Text))
        {
            var msg = "Найдено EXISTS (SELECT ...). Это может быть оправдано, но если подзапрос не коррелирован, JOIN может быть эффективнее. Оцените возможность переписывания.";
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(Code, msg, Category, DefaultSeverity, new List<string>()));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}