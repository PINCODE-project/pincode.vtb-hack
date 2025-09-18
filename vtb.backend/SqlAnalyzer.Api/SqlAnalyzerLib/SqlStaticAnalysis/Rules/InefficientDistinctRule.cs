using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование DISTINCT без необходимости,
/// когда в запросе нет агрегатов и JOIN, что может зря нагружать планировщик.
/// </summary>
public sealed class InefficientDistinctRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRules Code => StaticRules.InefficientDistinct;
    /// <inheritdoc />
    public Severity Severity => Severity.Info;

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("DISTINCT") && !sql.Contains("JOIN") && !sql.Contains("GROUP BY"))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.InefficientDistinctProblemDescription,
                StaticRuleRecommendations.InefficientDistinctRecommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
