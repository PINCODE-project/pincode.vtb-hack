using System.Text.RegularExpressions;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверка NOT IN — рекомендует NOT EXISTS для корректной работы с NULL и предотвращения неожиданных результатов.
/// </summary>
public sealed class NotInNullsRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.NotInNulls;

    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Rewrite;

    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    private static readonly Regex PatternNotIn = new(@"\bNOT\s+IN\s*\(", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex PatternInSelect = new(@"\bIN\s*\(\s*SELECT\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (PatternNotIn.IsMatch(query.Text))
        {
            var msg = "Используется NOT IN, который некорректно работает при наличии NULL в подзапросе. Рекомендуется заменить на NOT EXISTS для корректной семантики.";
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(Code, msg, Category, DefaultSeverity, new List<string>()));
        }

        if (PatternInSelect.IsMatch(query.Text) && !PatternNotIn.IsMatch(query.Text))
        {
            // Если IN (SELECT ...) — это отдельное правило (S10), но здесь даём подсказку про NULL в IN.
            return Task.FromResult<StaticCheckFinding?>(null);
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}