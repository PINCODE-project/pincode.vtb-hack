using System.Globalization;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// P12: Index Only Scan, но есть Heap Fetches — проверяем потерю выгод Index Only scan.
/// </summary>
public sealed class IndexOnlyHeapFetchRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.IndexOnlyHeapFetch;

    /// <inheritdoc />
    public string Category => "IndexOnly";

    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node.ShortNodeType == null) return Task.FromResult<PlanFinding?>(null);
        if (!node.ShortNodeType.Equals("IndexScan", StringComparison.OrdinalIgnoreCase) && !node.NodeType.Contains("Index Only Scan", StringComparison.OrdinalIgnoreCase)) return Task.FromResult<PlanFinding?>(null);

        if (node.NodeSpecific != null && node.NodeSpecific.TryGetValue("Heap Fetches", out var hfObj))
        {
            if (TryParseDouble(hfObj, out var hf) && hf > 0)
            {
                var metadata = new Dictionary<string, object> { ["HeapFetches"] = hf };
                var msg = "Index Only Scan выполняет heap fetches — visibility map вероятно не покрывает строки. Рекомендуется VACUUM/ANALYZE или CLUSTER для улучшения visibility.";
                return Task.FromResult<PlanFinding?>(new PlanFinding(Code, msg, Category, DefaultSeverity, new List<string>(), metadata));
            }
        }

        return Task.FromResult<PlanFinding?>(null);

        static bool TryParseDouble(object? o, out double d)
        {
            d = 0;
            if (o == null) return false;
            if (o is double dd) { d = dd; return true; }
            if (o is long ll) { d = ll; return true; }
            return double.TryParse(o.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out d);
        }
    }
}