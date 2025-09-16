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
    public ExplainIssueRule Code => ExplainIssueRule.ParallelWorkersTooMany;
    /// <inheritdoc />
    public string Category => "Parallelism";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Low;

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
            var metadata = new Dictionary<string, object?>
            {
                ["WorkersPlanned"] = planned,
                ["WorkersLaunched"] = launched,
                ["NodeType"] = node.NodeType
            };

            var message = $"Узел '{node.NodeType}' имеет слишком много воркеров (Launched={launched}, Planned={planned}). Может быть overhead.";

            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                message,
                Category,
                DefaultSeverity,
                Array.Empty<string>(),
                metadata
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}
