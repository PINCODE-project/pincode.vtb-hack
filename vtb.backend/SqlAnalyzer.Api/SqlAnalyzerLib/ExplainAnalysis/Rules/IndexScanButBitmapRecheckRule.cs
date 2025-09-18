using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет Index Scan, который сопровождается Bitmap Heap Recheck, что снижает эффективность.
/// </summary>
public sealed class IndexScanButBitmapRecheckRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.IndexScanButBitmapRecheck;

    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Index Scan", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        if (node.ContainsInNodeSpecific("Recheck Cond"))
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.IndexScanButBitmapRecheck, node.GetRelationName()),
                ExplainRuleRecommendations.IndexScanButBitmapRecheck
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}
