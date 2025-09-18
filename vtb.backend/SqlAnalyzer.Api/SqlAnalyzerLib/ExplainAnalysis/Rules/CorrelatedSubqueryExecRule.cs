using SqlAnalyzer.Api.Dal.Constants;
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
    public ExplainRules Code => ExplainRules.CorrelatedSubqueryExec;
    
    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null) return Task.FromResult<PlanFinding?>(null);

        if (node.NodeType.Contains("Subquery Scan", StringComparison.OrdinalIgnoreCase) &&
            node.ActualLoops.HasValue && node.ActualLoops.Value > 1)
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.CorrelatedSubqueryExec, node.ActualLoops),
                ExplainRuleRecommendations.CorrelatedSubqueryExec
            )); 
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}
