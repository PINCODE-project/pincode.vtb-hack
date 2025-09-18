using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет Materialize узлы, которые могут быть лишними и замедлять выполнение, если данные можно было бы вычислить напрямую.
/// </summary>
public sealed class MaterializeNodeRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.MaterializeNode;
    
    /// <inheritdoc />
    public Severity Severity => Severity.Info;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (!node.NodeType.Contains("Materialize", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        return Task.FromResult<PlanFinding?>(new PlanFinding(
            Code,
            Severity,
            ExplainRulePromblemDescriptions.MaterializeNode,
            ExplainRuleRecommendations.MaterializeNode
        ));
    }
}
