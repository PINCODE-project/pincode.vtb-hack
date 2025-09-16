using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет Nested Loop Join по большим таблицам, что часто приводит к экспоненциальному росту количества строк и времени выполнения.
/// </summary>
public sealed class NestedLoopOnLargeTablesRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.NestedLoopOnLargeTables;
    /// <inheritdoc />
    public string Category => "Join";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Nested Loop", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        double planRows = node.PlanRows ?? 0d;
        if (planRows > 10000) // порог большой таблицы
        {
            var affected = node.Children?
                .Select(c => TryGetNodeSpecificString(c, "Relation Name"))
                .Where(n => n != null)
                .ToList() ?? [];

            var metadata = new Dictionary<string, object?>
            {
                ["PlanRows"] = planRows,
                ["ActualRows"] = node.ActualRows
            };

            var message = $"Nested Loop Join на больших таблицах (PlanRows={planRows}) может быть крайне неэффективным. Рассмотрите Hash Join или Merge Join.";

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
