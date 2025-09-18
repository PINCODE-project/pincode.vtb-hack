using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
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
    public StaticRules Code => StaticRules.NotInNulls;


    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    private static readonly Regex PatternNotIn = new(@"\bNOT\s+IN\s*\(", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex PatternInSelect = new(@"\bIN\s*\(\s*SELECT\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (PatternNotIn.IsMatch(query.Text))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.NotInNullsProblemDescription,
                StaticRuleRecommendations.NotInNullsRecommendation
            ));
            
        }

        if (PatternInSelect.IsMatch(query.Text) && !PatternNotIn.IsMatch(query.Text))
        {
            // Если IN (SELECT ...) — это отдельное правило (S10), но здесь даём подсказку про NULL в IN.
            return Task.FromResult<StaticAnalysisPoint?>(null);
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}