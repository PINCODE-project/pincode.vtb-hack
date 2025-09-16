using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет Index Scan, который сопровождается Bitmap Heap Recheck, что снижает эффективность.
/// </summary>
public sealed class IndexScanButBitmapRecheckRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.IndexScanButBitmapRecheck;
    /// <inheritdoc />
    public string Category => "Index";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Index Scan", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        if (node.NodeSpecific != null && node.NodeSpecific.ContainsKey("Recheck Cond"))
        {
            var relation = TryGetNodeSpecificString(node, "Relation Name");
            var metadata = new Dictionary<string, object?>
            {
                ["RecheckCond"] = node.NodeSpecific["Recheck Cond"],
                ["NodeType"] = node.NodeType
            };

            var affected = relation != null ? new List<string> { relation } : [];
            var message = relation != null
                ? $"Index Scan на таблице '{relation}' требует Bitmap Heap Recheck. Рассмотрите оптимизацию индекса."
                : $"Index Scan требует Bitmap Heap Recheck. Рассмотрите оптимизацию индекса.";

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
