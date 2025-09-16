using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Обнаруживает Seq Scan с LIKE '%...' в условии, что делает использование индекса невозможным.
/// </summary>
public sealed class LeadingWildcardLikeRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.LeadingWildcardLike;
    /// <inheritdoc />
    public string Category => "Index";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null) return Task.FromResult<PlanFinding?>(null);

        if (node.NodeType.Contains("Seq Scan", StringComparison.OrdinalIgnoreCase) &&
            node.NodeSpecific != null && node.NodeSpecific.Values.Any(v => v?.ToString()?.Contains("like", StringComparison.OrdinalIgnoreCase) == true))
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
                ? $"Seq Scan по таблице '{relation}' использует LIKE с ведущим %, индекс не применяется."
                : "Seq Scan использует LIKE с ведущим %, индекс не применяется.";

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
