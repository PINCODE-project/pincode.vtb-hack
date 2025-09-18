using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование CROSS JOIN без фильтрации,
/// что приводит к декартовому произведению и взрывному росту строк.
/// </summary>
public sealed class CrossJoinWithoutConditionRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRules Code => StaticRules.CrossJoinWithoutCondition;
  
    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (query.Text.Contains("CROSS JOIN", StringComparison.OrdinalIgnoreCase) &&
            !query.Text.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.CrossJoinWithoutConditionProblemDescription,
                StaticRuleRecommendations.CrossJoinWithoutConditionRecommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
