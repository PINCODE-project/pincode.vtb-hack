using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование LIMIT без ORDER BY, что делает результат непредсказуемым
/// и может возвращать разные строки при каждом выполнении.
/// </summary>
public sealed class LimitWithoutOrderByRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRules Code => StaticRules.LimitWithoutOrderBy;
    
    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("LIMIT") && !sql.Contains("ORDER BY"))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.LimitWithoutOrderByProblemDescription,
                StaticRuleRecommendations.LimitWithoutOrderByRecommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
