using SqlAnalyzer.Api.Dal.Constants;
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
    public ExplainRules Code => ExplainRules.HashJoinWithSkew;

    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (!node.NodeType.Contains("Hash Join", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        var left = node.Children?.FirstOrDefault();
        var right = node.Children?.Skip(1).FirstOrDefault();

        if (left == null || right == null)
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        var leftRows = left.PlanRows ?? 0d;
        var rightRows = right.PlanRows ?? 0d;

        if (leftRows < 0 || rightRows < 0)
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        var ratio = Math.Max(leftRows, rightRows) / Math.Min(leftRows, rightRows);
        if (ratio > 10)
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.HashJoinWithSkew, ratio.ToString("F1")),
                ExplainRuleRecommendations.HashJoinWithSkew
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}