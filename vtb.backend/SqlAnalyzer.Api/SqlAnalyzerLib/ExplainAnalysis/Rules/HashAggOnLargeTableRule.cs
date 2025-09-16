using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Обнаруживает Hash Aggregate узлы на больших таблицах, что может приводить к большим расходам памяти и медленной агрегации.
/// </summary>
public sealed class HashAggOnLargeTableRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.HashAggOnLargeTable;
    /// <inheritdoc />
    public string Category => "Aggregation";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("HashAggregate", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        var planRows = node.PlanRows ?? 0d;
        if (planRows > 10000)
        {
            var metadata = new Dictionary<string, object?>
            {
                ["PlanRows"] = planRows,
                ["ActualRows"] = node.ActualRows,
                ["NodeType"] = node.NodeType
            };

            var message = $"Hash Aggregate узел на большой таблице (PlanRows={planRows}). Рассмотрите использование группировок с предварительной фильтрацией или увеличением work_mem.";

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
