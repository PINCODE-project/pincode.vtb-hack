using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет узлы выполнения коррелированных подзапросов, которые выполняются многократно и замедляют выполнение.
/// </summary>
public sealed class CorrelatedSubqueryExecRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.CorrelatedSubqueryExec;
    /// <inheritdoc />
    public string Category => "Rewrite";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null) return Task.FromResult<PlanFinding?>(null);

        if (node.NodeType.Contains("Subquery Scan", StringComparison.OrdinalIgnoreCase) &&
            node.ActualLoops.HasValue && node.ActualLoops.Value > 1)
        {
            var metadata = new Dictionary<string, object?>
            {
                ["NodeType"] = node.NodeType,
                ["ActualLoops"] = node.ActualLoops,
                ["ActualRows"] = node.ActualRows
            };

            var message = $"Коррелированный подзапрос выполняется {node.ActualLoops} раз, что может сильно замедлять выполнение. Рассмотрите JOIN или CTE.";

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
