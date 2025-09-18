using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование LEFT JOIN с условием в WHERE, что фактически превращает его в INNER JOIN.
/// </summary>
public sealed class LeftJoinFollowedByWhereRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRules Code => StaticRules.LeftJoinFollowedByWhere;
    
    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("LEFT JOIN") && sql.Contains(" WHERE ") && sql.Contains(" = "))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.LeftJoinFollowedByWhereProblemDescription,
                StaticRuleRecommendations.LeftJoinFollowedByWhereRecommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
