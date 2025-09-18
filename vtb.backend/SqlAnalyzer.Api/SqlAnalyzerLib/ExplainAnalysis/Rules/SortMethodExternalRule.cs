using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет узлы Sort, которые используют внешний метод сортировки на диске.
/// </summary>
public sealed class SortMethodExternalRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.SortMethodExternal;

    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Sort", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        var methodObj = node.TryGetNodeSpecificString("Sort Method");
        if (methodObj?.Contains("external", StringComparison.OrdinalIgnoreCase) == true)
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.SortMethodExternal, node.NodeType, methodObj),
                ExplainRuleRecommendations.SortMethodExternal
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}
