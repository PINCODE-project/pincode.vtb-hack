using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Обнаруживает узлы с большим количеством повторных итераций (loops),
/// что может указывать на Nested Loop по большим таблицам или неэффективные соединения.
/// </summary>
public sealed class LargeNumberOfLoopsRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.LargeNumberOfLoops;
    /// <inheritdoc />
    public string Category => "Performance";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.ActualLoops.HasValue != true) return Task.FromResult<PlanFinding?>(null);

        if (node.ActualLoops.Value > 1000)
        {
            var relation = TryGetNodeSpecificString(node, "Relation Name");

            var metadata = new Dictionary<string, object?>
            {
                ["ActualLoops"] = node.ActualLoops,
                ["PlanRows"] = node.PlanRows,
                ["ActualRows"] = node.ActualRows
            };

            var affected = relation != null ? new List<string> { relation } : [];
            var message = relation != null
                ? $"Узел '{node.NodeType}' по таблице '{relation}' выполняется {node.ActualLoops} раз (loops), возможно Nested Loop по большой таблице или неэффективное соединение."
                : $"Узел '{node.NodeType}' выполняется {node.ActualLoops} раз (loops), возможно Nested Loop по большой таблице или неэффективное соединение.";

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
