using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет узлы с большим расхождением фактических и оценочных строк (>10x), что может указывать на проблемы со статистикой или селективностью.
/// </summary>
public sealed class ActualVsEstimatedLargeDiffRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.ActualVsEstimatedLargeDiff;
    /// <inheritdoc />
    public string Category => "Statistics";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (!node.PlanRows.HasValue || !node.ActualRows.HasValue) return Task.FromResult<PlanFinding?>(null);

        double ratio = Math.Max(node.ActualRows.Value, 1) / Math.Max(node.PlanRows.Value, 1);
        if (ratio > 10 || ratio < 0.1)
        {
            var metadata = new Dictionary<string, object?>
            {
                ["PlanRows"] = node.PlanRows,
                ["ActualRows"] = node.ActualRows,
                ["Ratio"] = ratio
            };

            var message = $"Фактическое количество строк сильно отличается от оценочного (Actual/Plan={ratio:F1}). Проверьте статистику и селективность условий.";

            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                message,
                Category,
                DefaultSeverity,
                Array.Empty<string>(),
                metadata
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}
