using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование HAVING без GROUP BY.
/// Такое условие избыточно и может быть заменено на WHERE.
/// </summary>
public sealed class HavingWithoutGroupByRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRules Code => StaticRules.HavingWithoutGroupBy;
    /// <inheritdoc />
    public Severity Severity => Severity.Info;

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("HAVING") && !sql.Contains("GROUP BY"))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.HavingWithoutGroupByProblemDescription,
                StaticRuleRecommendations.HavingWithoutGroupByRecommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
