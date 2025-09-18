using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование COUNT(*) в запросах с JOIN, что может приводить к завышенному результату
/// из-за дублирования строк.
/// </summary>
public sealed class CountStarWithJoinRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRules Code => StaticRules.CountStarWithJoin;
  
    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("COUNT(*)") && sql.Contains("JOIN"))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.CountStarWithJoinProblemDescription,
                StaticRuleRecommendations.CountStarWithJoinRecommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
