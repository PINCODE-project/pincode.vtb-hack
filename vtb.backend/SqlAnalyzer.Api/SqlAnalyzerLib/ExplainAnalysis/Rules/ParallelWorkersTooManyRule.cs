using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет узлы с числом параллельных воркеров больше рекомендуемого, что может приводить к overhead.
/// </summary>
public sealed class ParallelWorkersTooManyRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.ParallelWorkersTooMany;

    /// <inheritdoc />
    public Severity Severity => Severity.Info;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeSpecific == null) return Task.FromResult<PlanFinding?>(null);

        if (node.NodeSpecific.TryGetValue("Workers Planned", out var plannedObj) &&
            node.NodeSpecific.TryGetValue("Workers Launched", out var launchedObj) &&
            int.TryParse(plannedObj.ToString(), out var planned) &&
            int.TryParse(launchedObj.ToString(), out var launched) &&
            launched > planned + 4) // произвольное превышение
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.ParallelWorkersTooMany, node.NodeType, launched, planned),
                ExplainRuleRecommendations.ParallelWorkersTooMany
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}
