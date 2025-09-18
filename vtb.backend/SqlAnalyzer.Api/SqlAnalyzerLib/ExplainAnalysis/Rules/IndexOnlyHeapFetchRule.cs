using System.Globalization;
using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
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
    public ExplainRules Code => ExplainRules.IndexOnlyHeapFetch;

    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node.ShortNodeType == null)
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        if (!node.ShortNodeType.Equals("IndexScan", StringComparison.OrdinalIgnoreCase) &&
            !node.NodeType.Contains("Index Only Scan", StringComparison.OrdinalIgnoreCase)
           )
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        var hfObj = node.TryGetNodeSpecificString("Heap Fetches");
        if (hfObj is not null && TryParseDouble(hfObj, out var hf) && hf > 0)
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                ExplainRulePromblemDescriptions.IndexOnlyHeapFetch,
                ExplainRuleRecommendations.IndexOnlyHeapFetch
            ));
        }

        return Task.FromResult<PlanFinding?>(null);

        static bool TryParseDouble(object? o, out double d)
        {
            d = 0;
            if (o == null) return false;
            if (o is double dd)
            {
                d = dd;
                return true;
            }

            if (o is long ll)
            {
                d = ll;
                return true;
            }

            return double.TryParse(o.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out d);
        }
    }
}