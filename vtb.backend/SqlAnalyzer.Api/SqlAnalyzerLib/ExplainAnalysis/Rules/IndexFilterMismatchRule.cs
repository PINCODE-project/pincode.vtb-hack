using System.Globalization;
using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// P11: Index Scan, но фильтр в Filter (после Index Cond) отбрасывает много строк -> попробовать индекс по выражению или INCLUDE.
/// Условие: NodeType содержит 'Index Scan' или 'Bitmap Heap Scan', есть Index Cond и есть Filter с высокой отсечкой.
/// </summary>
public sealed class IndexFilterMismatchRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.IndexFilterMismatch;

    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        var s = node.ShortNodeType;
        if (node.NodeSpecific == null || s == null)
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        if (s.Equals("IndexScan", StringComparison.OrdinalIgnoreCase) == false &&
            s.Equals("BitmapHeapScan", StringComparison.OrdinalIgnoreCase) == false
           )
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        var spec = node.NodeSpecific;
        if (!spec.ContainsKey("Index Cond") &&
            !spec.ContainsKey("Index Conds") &&
            !spec.ContainsKey("IndexName") &&
            !spec.ContainsKey("Index Name")
           )
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        var filter = spec.ContainsKey("Filter") ? spec["Filter"]?.ToString() : null;
        var rowsRemoved = TryGetDoubleFromNodeSpecific(spec, "Rows Removed by Filter") ??
                          TryGetDoubleFromNodeSpecific(spec, "Rows Removed");
        var actualRows = node.ActualRows ?? 0;

        if (!string.IsNullOrEmpty(filter) && rowsRemoved.HasValue)
        {
            var total = actualRows + rowsRemoved.Value;
            if (total > 0 && rowsRemoved.Value / total >= 0.5)
            {
                return Task.FromResult<PlanFinding?>(new PlanFinding(
                    Code,
                    Severity,
                    ExplainRulePromblemDescriptions.IndexFilterMismatch,
                    ExplainRuleRecommendations.IndexFilterMismatch
                ));
            }
        }

        return Task.FromResult<PlanFinding?>(null);

        static double? TryGetDoubleFromNodeSpecific(IReadOnlyDictionary<string, object> spec, string key)
        {
            if (!spec.TryGetValue(key, out var v)) return null;
            if (v == null) return null;
            if (v is long ll) return Convert.ToDouble(ll);
            if (v is double dd) return dd;
            if (double.TryParse(v.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                return parsed;
            return null;
        }
    }
}