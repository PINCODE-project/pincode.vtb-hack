using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
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
    public StaticRules Code => StaticRules.LeadingWildcardLike;
    

    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    private static readonly Regex Pattern = new(@"\b(?:LIKE|ILIKE)\s+'%[^']*'", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Pattern.IsMatch(query.Text))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.LeadingWildcardLikeProblemDescription,
                StaticRuleRecommendations.LeadingWildcardLikeRecommendation
            ));
        }
        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}