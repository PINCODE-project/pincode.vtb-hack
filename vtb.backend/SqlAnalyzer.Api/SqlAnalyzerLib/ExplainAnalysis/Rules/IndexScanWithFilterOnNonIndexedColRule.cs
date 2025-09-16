using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет Index Scan, где фильтр применяется на колонке без индекса, что снижает эффективность.
/// </summary>
public sealed class IndexScanWithFilterOnNonIndexedColRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.IndexScanWithFilterOnNonIndexedCol;
    /// <inheritdoc />
    public string Category => "Index";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Index Scan", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        if (node.NodeSpecific != null && node.NodeSpecific.TryGetValue("Filter", out var filter) &&
            !string.IsNullOrEmpty(filter?.ToString()))
        {
            var metadata = new Dictionary<string, object?>
            {
                ["Filter"] = filter,
                ["NodeType"] = node.NodeType
            };

            var message = $"Index Scan использует фильтр '{filter}', который может быть по неиндексированной колонке. Рассмотрите добавление индекса.";

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
