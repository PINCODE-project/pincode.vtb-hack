using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Обнаруживает узлы типа Function Scan, которые могут быть источником медленных операций или проблем с планированием.
/// </summary>
public sealed class FunctionScanRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.FunctionScan;
    
    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Function Scan", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        return Task.FromResult<PlanFinding?>(new PlanFinding(
            Code,
            Severity,
            string.Format(ExplainRulePromblemDescriptions.FunctionScan, node.TryGetNodeSpecificString("Function Name")),
            ExplainRuleRecommendations.FunctionScan
        ));
    }
}
