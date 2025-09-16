using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет Seq Scan на таблицах с частыми обновлениями, где может быть полезен индекс или VACUUM.
/// </summary>
public sealed class SeqScanOnRecentlyUpdatedTableRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.SeqScanOnRecentlyUpdatedTable;
    /// <inheritdoc />
    public string Category => "Index";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Seq Scan", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        if (node.NodeSpecific != null && node.NodeSpecific.TryGetValue("Recent Updates", out var updatesObj) &&
            int.TryParse(updatesObj.ToString(), out var updates) && updates > 1000)
        {
            var relation = TryGetNodeSpecificString(node, "Relation Name");
            var metadata = new Dictionary<string, object?>
            {
                ["RecentUpdates"] = updates,
                ["NodeType"] = node.NodeType
            };

            var affected = relation != null ? new List<string> { relation } : [];
            var message = relation != null
                ? $"Seq Scan на недавно обновлённой таблице '{relation}' (updates={updates}). Рассмотрите индекс или VACUUM."
                : $"Seq Scan на недавно обновлённой таблице (updates={updates}). Рассмотрите индекс или VACUUM.";

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
