using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверка использования SELECT * — рекомендует выбирать явные колонки.
/// </summary>
public sealed class SelectStarRule //: IStaticRule
{
    /// <inheritdoc />
    public StaticRules Code => StaticRules.SelectStar;

    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Rewrite;

    /// <inheritdoc />
    public Severity Severity => Severity.Info;

    private static readonly Regex Pattern = new(@"\bSELECT\s+\*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Pattern.IsMatch(query.Text))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.SelectStarProblemDescription,
                StaticRuleRecommendations.SelectStarRecommendation
            ));   }
        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}