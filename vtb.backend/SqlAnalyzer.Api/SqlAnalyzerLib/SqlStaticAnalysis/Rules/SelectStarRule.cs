using System.Text.RegularExpressions;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверка использования SELECT * — рекомендует выбирать явные колонки.
/// </summary>
public sealed class SelectStarRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.SelectStar;

    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Rewrite;

    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Low;

    private static readonly Regex Pattern = new(@"\bSELECT\s+\*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Pattern.IsMatch(query.Text))
        {
            var msg = "Использование SELECT * приводит к передаче лишних данных и неопределённости. Явно указывайте список колонок.";
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(Code, msg, Category, DefaultSeverity, new List<string>()));
        }
        return Task.FromResult<StaticCheckFinding?>(null);
    }
}