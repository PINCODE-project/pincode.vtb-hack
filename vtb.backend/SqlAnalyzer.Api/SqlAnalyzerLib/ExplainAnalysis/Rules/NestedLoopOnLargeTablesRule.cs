using SqlAnalyzer.Api.Dal.Constants;
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
    public ExplainRules Code => ExplainRules.NestedLoopOnLargeTables;

    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Nested Loop", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        var planRows = node.PlanRows ?? 0d;
        if (planRows > 10000) 
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.NestedLoopOnLargeTables, planRows),
                ExplainRuleRecommendations.NestedLoopOnLargeTables
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}
