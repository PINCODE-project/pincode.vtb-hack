using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет узлы, где фильтр (Filter/WHERE) применяется после агрегирования, что может быть неэффективно.
/// </summary>
public sealed class FilterAfterAggregateRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.FilterAfterAggregate;
    
    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Aggregate", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        if (node.NodeSpecific != null && node.NodeSpecific.ContainsKey("Filter"))
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.FilterAfterAggregate, node.NodeType),
                ExplainRuleRecommendations.FilterAfterAggregate
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}
