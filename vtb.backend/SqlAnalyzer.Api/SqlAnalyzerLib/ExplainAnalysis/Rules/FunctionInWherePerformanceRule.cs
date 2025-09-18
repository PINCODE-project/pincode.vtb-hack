using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет случаи, когда фильтрация выполняется через функцию в WHERE или JOIN,
/// что делает индекс недействующим и замедляет выполнение.
/// </summary>
public sealed class FunctionInWherePerformanceRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.FunctionInWherePerformance;

    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null) return Task.FromResult<PlanFinding?>(null);

        if (
            (
                node.NodeType.Contains("Seq Scan", StringComparison.OrdinalIgnoreCase) ||
                node.NodeType.Contains("Bitmap Heap Scan", StringComparison.OrdinalIgnoreCase)
            ) &&
            node.NodeSpecific != null &&
            node.NodeSpecific.Values.Any(v =>
                v.ToString()?.Contains("function", StringComparison.OrdinalIgnoreCase) == true)
        )
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.FunctionInWherePerformance, node.GetRelationName()),
                ExplainRuleRecommendations.FunctionInWherePerformance
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}