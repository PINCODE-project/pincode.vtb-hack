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
    public StaticRuleCodes Code => StaticRuleCodes.CrossJoinWithoutCondition;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Performance;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (query.Text.Contains("CROSS JOIN", StringComparison.OrdinalIgnoreCase) &&
            !query.Text.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "CROSS JOIN без условия приводит к декартовому произведению.",
                Category,
                DefaultSeverity,
                new List<string> { "CROSS JOIN" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
