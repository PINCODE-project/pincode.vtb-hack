using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет последовательные сканы (Seq Scan) по больших таблицах без индекса.
/// </summary>
public sealed class SeqScanOnLargeTableRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.SeqScanOnLargeTable;
    /// <inheritdoc />
    public string Category => "Index";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Seq Scan", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        double planRows = node.PlanRows ?? 0d;
        if (planRows > 10000) // порог большой таблицы
        {
            var relation = TryGetNodeSpecificString(node, "Relation Name");
            var metadata = new Dictionary<string, object?>
            {
                ["PlanRows"] = planRows,
                ["ActualRows"] = node.ActualRows
            };

            var affected = relation != null ? new List<string> { relation } : [];
            var message = relation != null
                ? $"Seq Scan по большой таблице '{relation}' (PlanRows={planRows}). Рассмотрите создание индекса."
                : $"Seq Scan по большой таблице (PlanRows={planRows}). Рассмотрите создание индекса.";

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
