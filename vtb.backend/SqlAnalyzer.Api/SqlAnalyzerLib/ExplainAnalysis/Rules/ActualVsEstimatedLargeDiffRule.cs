using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет узлы с большим расхождением фактических и оценочных строк (>10x), что может указывать на проблемы со статистикой или селективностью.
/// </summary>
public sealed class ActualVsEstimatedLargeDiffRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.ActualVsEstimatedLargeDiff;

    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (!node.PlanRows.HasValue || !node.ActualRows.HasValue)
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        var ratio = Math.Max(node.ActualRows.Value, 1) / Math.Max(node.PlanRows.Value, 1);
        if (ratio is > 10 or < 0.1)
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.ActualVsEstimatedLargeDiff, ratio),
                ExplainRuleRecommendations.ActualVsEstimatedLargeDiff
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}
