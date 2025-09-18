using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет случаи, когда Bitmap Heap Scan считывает значительно больше строк,
/// чем было оценено по Bitmap Index Scan. Это может говорить о плохой селективности индекса или необходимости пересмотра фильтров.
/// </summary>
public sealed class BitmapHeapOverfetchRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.BitmapHeapOverfetch;

    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null) return Task.FromResult<PlanFinding?>(null);

        if (!node.NodeType.Contains("Bitmap Heap Scan", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        if (node is { ActualRows: not null, PlanRows: not null } && node.ActualRows.Value > node.PlanRows.Value * 3)
        {
            var relation = TryGetNodeSpecificString(node, "Relation Name");
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.BitmapHeapOverfetch, relation ?? "unknown"),
                ExplainRuleRecommendations.BitmapHeapOverfetch
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }

    private static string? TryGetNodeSpecificString(PlanNode node, string key)
    {
        if (node.NodeSpecific != null && node.NodeSpecific.TryGetValue(key, out var v) && v != null)
            return v.ToString();
        return null;
    }
}
