using SqlAnalyzer.Api.Dal.Constants;
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
    public ExplainRules Code => ExplainRules.HashAggOnLargeTable;

    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("HashAggregate", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        var planRows = node.PlanRows ?? 0d;
        if (planRows > 10000)
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.HashAggOnLargeTable, planRows),
                ExplainRuleRecommendations.HashAggOnLargeTable
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}
