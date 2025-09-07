using System.Text.RegularExpressions;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверка LIKE/ILIKE с ведущим подстановочным знаком.
/// Предлагает trigram/Gin индекс для подстрочных поисков.
/// </summary>
public sealed class LeadingWildcardLikeRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.LeadingWildcardLike;

    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Index;

    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    private static readonly Regex Pattern = new(@"\b(?:LIKE|ILIKE)\s+'%[^']*'", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Pattern.IsMatch(query.Text))
        {
            var msg = "LIKE/ILIKE начинается с '%', обычный btree-индекс не поможет. Рассмотрите pg_trgm + GIN/GiST индекс или полнотекстовый подход.";
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(Code, msg, Category, DefaultSeverity, new List<string>()));
        }
        return Task.FromResult<StaticCheckFinding?>(null);
    }
}