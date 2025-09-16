using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет случаи, когда Bitmap Heap Scan считывает значительно больше строк,
/// чем было оценено по Bitmap Index Scan. Это может говорить о плохой селективности индекса или необходимости пересмотра фильтров.
/// </summary>
public sealed class BitmapHeapOverfetchRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.BitmapHeapOverfetch;
    /// <inheritdoc />
    public string Category => "Index";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null) return Task.FromResult<PlanFinding?>(null);

        if (!node.NodeType.Contains("Bitmap Heap Scan", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        if (node.ActualRows.HasValue && node.PlanRows.HasValue && node.ActualRows.Value > node.PlanRows.Value * 3)
        {
            var relation = TryGetNodeSpecificString(node, "Relation Name");

            var metadata = new Dictionary<string, object?>
            {
                ["NodeType"] = node.NodeType,
                ["PlanRows"] = node.PlanRows,
                ["ActualRows"] = node.ActualRows,
                ["Buffers"] = node.Buffers,
                ["IndexCond"] = node.NodeSpecific != null && node.NodeSpecific.TryGetValue("Index Cond", out var cond) ? cond : null
            };

            var affected = relation != null ? new List<string> { relation } : [];

            var message = relation != null
                ? $"Bitmap Heap Scan по таблице '{relation}' возвращает значительно больше строк, чем оценено по Bitmap Index Scan. Проверьте селективность индекса и фильтры."
                : $"Bitmap Heap Scan возвращает значительно больше строк, чем оценено по Bitmap Index Scan. Проверьте селективность индекса и фильтры.";

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
    {
        if (node.NodeSpecific != null && node.NodeSpecific.TryGetValue(key, out var v) && v != null)
            return v.ToString();
        return null;
    }
}
