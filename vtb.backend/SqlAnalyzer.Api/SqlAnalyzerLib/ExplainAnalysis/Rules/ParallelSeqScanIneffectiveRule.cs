using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет случаи, когда Parallel Seq Scan используется, но фактическая параллельность низкая или неэффективная.
/// </summary>
public sealed class ParallelSeqScanIneffectiveRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.ParallelSeqScanIneffective;

    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Parallel Seq Scan", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        if ((node.ActualRows ?? 0) > 0 && (node.ActualLoops ?? 1) <= 1)
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.ParallelSeqScanIneffective, node.GetRelationName(), node.ActualLoops),
                ExplainRuleRecommendations.ParallelSeqScanIneffective
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
    
}
