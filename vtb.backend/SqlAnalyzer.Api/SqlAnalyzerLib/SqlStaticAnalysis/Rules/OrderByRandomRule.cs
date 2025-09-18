using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование ORDER BY RANDOM(), которое крайне неэффективно
/// на больших таблицах и может привести к полному сканированию.
/// </summary>
public sealed class OrderByRandomRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRules Code => StaticRules.OrderByRandom;
    
    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (query.Text.Contains("ORDER BY RANDOM()", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.OrderByRandomProblemDescription,
                StaticRuleRecommendations.OrderByRandomRecommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
