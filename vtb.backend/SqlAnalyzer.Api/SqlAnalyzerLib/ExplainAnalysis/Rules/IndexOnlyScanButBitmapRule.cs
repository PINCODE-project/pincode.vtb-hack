using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Обнаруживает узлы Index Only Scan, которые по факту используют Bitmap Heap Scan,
/// что говорит о том, что индекс не полностью покрывает запрос или нужен дополнительный fetch.
/// </summary>
public sealed class IndexOnlyScanButBitmapRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.IndexOnlyScanButBitmap;
    /// <inheritdoc />
    public string Category => "Index";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null) return Task.FromResult<PlanFinding?>(null);

        if (node.NodeType.Contains("Index Only Scan", StringComparison.OrdinalIgnoreCase))
        {
            if (node.Children?.Any(c => c.NodeType.Contains("Bitmap Heap Scan", StringComparison.OrdinalIgnoreCase)) == true)
            {
                var relation = TryGetNodeSpecificString(node, "Relation Name");
                var metadata = new Dictionary<string, object?>
                {
                    ["NodeType"] = node.NodeType,
                    ["PlanRows"] = node.PlanRows,
                    ["ActualRows"] = node.ActualRows
                };

                var affected = relation != null ? new List<string> { relation } : [];
                var message = relation != null
                    ? $"Index Only Scan по таблице '{relation}' использует Bitmap Heap Scan, возможно индекс не покрывает все необходимые колонки."
                    : $"Index Only Scan использует Bitmap Heap Scan, возможно индекс не покрывает все необходимые колонки.";

                return Task.FromResult<PlanFinding?>(new PlanFinding(
                    Code,
                    message,
                    Category,
                    DefaultSeverity,
                    affected,
                    metadata
                ));
            }
        }

        return Task.FromResult<PlanFinding?>(null);
    }

    private static string? TryGetNodeSpecificString(PlanNode node, string key)
        => node.NodeSpecific != null && node.NodeSpecific.TryGetValue(key, out var v) && v != null ? v.ToString() : null;
}
