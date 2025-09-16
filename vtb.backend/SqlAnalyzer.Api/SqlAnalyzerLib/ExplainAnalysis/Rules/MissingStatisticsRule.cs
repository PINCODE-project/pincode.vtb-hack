using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет узлы с сильными расхождениями между оценочными и фактическими строками, что может быть вызвано устаревшей или отсутствующей статистикой.
/// </summary>
public sealed class MissingStatisticsRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.MissingStatistics;
    /// <inheritdoc />
    public string Category => "Statistics";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (!node.PlanRows.HasValue || !node.ActualRows.HasValue) return Task.FromResult<PlanFinding?>(null);

        double ratio = node.ActualRows.Value / (node.PlanRows.Value + 1); // +1 чтобы избежать деления на ноль
        if (ratio > 5 || ratio < 0.2)
        {
            var relation = TryGetNodeSpecificString(node, "Relation Name");
            var metadata = new Dictionary<string, object?>
            {
                ["PlanRows"] = node.PlanRows,
                ["ActualRows"] = node.ActualRows,
                ["Ratio"] = ratio
            };

            var affected = relation != null ? new List<string> { relation } : [];
            var message = relation != null
                ? $"Таблица '{relation}' имеет большие расхождения оценочных и фактических строк. Проверьте статистику."
                : "Узел имеет большие расхождения оценочных и фактических строк. Проверьте статистику.";

            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                message,
                Category,
                DefaultSeverity,
                affected,
                metadata
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }

    private static string? TryGetNodeSpecificString(PlanNode node, string key)
        => node.NodeSpecific != null && node.NodeSpecific.TryGetValue(key, out var v) && v != null ? v.ToString() : null;
}
