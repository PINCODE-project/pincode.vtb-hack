using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет Bitmap Index Scan на маленькой таблице, где может быть эффективнее Seq Scan.
/// </summary>
public sealed class BitmapIndexScanOnSmallTableRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.BitmapIndexScanOnSmallTable;
    /// <inheritdoc />
    public string Category => "Index";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Bitmap Index Scan", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        if (node.PlanRows.HasValue && node.PlanRows.Value < 1000)
        {
            var relation = TryGetNodeSpecificString(node, "Relation Name");
            var metadata = new Dictionary<string, object?>
            {
                ["PlanRows"] = node.PlanRows,
                ["NodeType"] = node.NodeType
            };

            var affected = relation != null ? new List<string> { relation } : [];
            var message = relation != null
                ? $"Bitmap Index Scan на маленькой таблице '{relation}' (PlanRows={node.PlanRows}). Возможно, эффективнее Seq Scan."
                : $"Bitmap Index Scan на маленькой таблице (PlanRows={node.PlanRows}). Возможно, эффективнее Seq Scan.";

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
