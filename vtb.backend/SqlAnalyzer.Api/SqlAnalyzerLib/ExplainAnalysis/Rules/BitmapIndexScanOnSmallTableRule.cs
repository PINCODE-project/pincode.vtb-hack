using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет Bitmap Index Scan на маленькой таблице, где может быть эффективнее Seq Scan.
/// </summary>
public sealed class BitmapIndexScanOnSmallTableRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.BitmapIndexScanOnSmallTable;

    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (!node.NodeType.Contains("Bitmap Index Scan", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        if (node.PlanRows is < 1000)
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.BitmapIndexScanOnSmallTable, node.GetRelationName(), node.PlanRows),
                ExplainRuleRecommendations.BitmapIndexScanOnSmallTable
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}
