using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет узлы Nested Loop без join condition (кросс-произведение), что может сильно замедлять выполнение.
/// </summary>
public sealed class CrossProductDetectedRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.CrossProductDetected;

    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Nested Loop", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        if (node.NodeSpecific != null && !node.NodeSpecific.ContainsKey("Join Filter"))
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.CrossProductDetected, node.NodeType),
                ExplainRuleRecommendations.CrossProductDetected
            )); 
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}
