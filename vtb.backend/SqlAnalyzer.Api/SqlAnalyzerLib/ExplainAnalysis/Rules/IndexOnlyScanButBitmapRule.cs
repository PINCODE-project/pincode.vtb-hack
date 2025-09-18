using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Обнаруживает узлы Index Only Scan, которые по факту используют Bitmap Heap Scan,
/// что говорит о том, что индекс не полностью покрывает запрос или нужен дополнительный fetch.
/// </summary>
public sealed class IndexOnlyScanButBitmapRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.IndexOnlyScanButBitmap;

    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null) return Task.FromResult<PlanFinding?>(null);

        if (node.NodeType.Contains("Index Only Scan", StringComparison.OrdinalIgnoreCase))
        {
            if (node.Children?.Any(c => c.NodeType.Contains("Bitmap Heap Scan", StringComparison.OrdinalIgnoreCase)) == true)
            {
                return Task.FromResult<PlanFinding?>(new PlanFinding(
                    Code,
                    Severity,
                    string.Format(ExplainRulePromblemDescriptions.IndexOnlyScanButBitmap, node.GetRelationName()),
                    ExplainRuleRecommendations.IndexOnlyScanButBitmap
                ));
            }
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}
