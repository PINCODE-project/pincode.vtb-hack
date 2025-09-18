using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
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
    public StaticRules Code => StaticRules.SubqueryInsteadOfJoin;

    /// <inheritdoc />
    public Severity Severity => Severity.Info;

    private static readonly Regex InSelectPattern = new(@"\bIN\s*\(\s*SELECT\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ExistsSelectPattern = new(@"\bEXISTS\s*\(\s*SELECT\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (InSelectPattern.IsMatch(query.Text))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.SubqueryInInsteadOfJoinProblemDescription,
                StaticRuleRecommendations.SubqueryInInsteadOfJoinRecommendation
            ));}

        if (ExistsSelectPattern.IsMatch(query.Text))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.SubqueryExistsInsteadOfJoinProblemDescription,
                StaticRuleRecommendations.SubqueryExistsInsteadOfJoinRecommendation
            ));  }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}