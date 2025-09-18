using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет Index Scan, где фильтр применяется на колонке без индекса, что снижает эффективность.
/// </summary>
public sealed class IndexScanWithFilterOnNonIndexedColRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.IndexScanWithFilterOnNonIndexedCol;
    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Index Scan", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        var filter = node.TryGetNodeSpecificString("Filter");
        if (string.IsNullOrEmpty(filter) == false)
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.IndexScanWithFilterOnNonIndexedCol, filter),
                ExplainRuleRecommendations.IndexScanWithFilterOnNonIndexedCol
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}
