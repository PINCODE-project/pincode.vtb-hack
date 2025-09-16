using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет случаи, когда Parallel Seq Scan используется, но фактическая параллельность низкая или неэффективная.
/// </summary>
public sealed class ParallelSeqScanIneffectiveRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.ParallelSeqScanIneffective;
    /// <inheritdoc />
    public string Category => "Parallelism";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Parallel Seq Scan", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        if ((node.ActualRows ?? 0) > 0 && (node.ActualLoops ?? 1) <= 1)
        {
            var relation = TryGetNodeSpecificString(node, "Relation Name");
            var metadata = new Dictionary<string, object?>
            {
                ["NodeType"] = node.NodeType,
                ["ActualRows"] = node.ActualRows,
                ["ActualLoops"] = node.ActualLoops
            };

            var affected = relation != null ? new List<string> { relation } : [];
            var message = relation != null
                ? $"Parallel Seq Scan по таблице '{relation}' оказался неэффективным (ActualLoops={node.ActualLoops}). Рассмотрите настройки parallel_workers."
                : $"Parallel Seq Scan оказался неэффективным (ActualLoops={node.ActualLoops}). Рассмотрите настройки parallel_workers.";

            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                message,
                Category,
                DefaultSeverity,
                affected,
                metadata
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }

    private static string? TryGetNodeSpecificString(PlanNode node, string key)
        => node.NodeSpecific != null && node.NodeSpecific.TryGetValue(key, out var v) && v != null ? v.ToString() : null;
}
