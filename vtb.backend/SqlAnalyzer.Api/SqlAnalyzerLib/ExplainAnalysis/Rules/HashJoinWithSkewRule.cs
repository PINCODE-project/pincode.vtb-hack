using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Обнаруживает Hash Join, где одна из таблиц сильно меньше другой (skew),
/// что может приводить к неравномерной загрузке памяти и неэффективной хэш-агрегации.
/// </summary>
public sealed class HashJoinWithSkewRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.HashJoinWithSkew;
    /// <inheritdoc />
    public string Category => "Join";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Hash Join", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        var left = node.Children?.FirstOrDefault();
        var right = node.Children?.Skip(1).FirstOrDefault();

        if (left == null || right == null) return Task.FromResult<PlanFinding?>(null);

        double leftRows = left.PlanRows ?? 0d;
        double rightRows = right.PlanRows ?? 0d;

        if (leftRows > 0 && rightRows > 0)
        {
            double ratio = Math.Max(leftRows, rightRows) / Math.Min(leftRows, rightRows);
            if (ratio > 10) // skew >10x
            {
                var affected = new List<string>();
                if (TryGetNodeSpecificString(left, "Relation Name") is string l) affected.Add(l);
                if (TryGetNodeSpecificString(right, "Relation Name") is string r) affected.Add(r);

                var metadata = new Dictionary<string, object?>
                {
                    ["LeftPlanRows"] = leftRows,
                    ["RightPlanRows"] = rightRows,
                    ["Ratio"] = ratio
                };

                var message = $"Hash Join с сильным дисбалансом между таблицами (ratio {ratio:F1}x). Возможна неэффективная хэш-агрегация.";

                return Task.FromResult<PlanFinding?>(new PlanFinding(
                    Code,
                    message,
                    Category,
                    DefaultSeverity,
                    affected,
                    metadata
                ));
            }
        }

        return Task.FromResult<PlanFinding?>(null);
    }

    private static string? TryGetNodeSpecificString(PlanNode node, string key)
        => node.NodeSpecific != null && node.NodeSpecific.TryGetValue(key, out var v) && v != null ? v.ToString() : null;
}
