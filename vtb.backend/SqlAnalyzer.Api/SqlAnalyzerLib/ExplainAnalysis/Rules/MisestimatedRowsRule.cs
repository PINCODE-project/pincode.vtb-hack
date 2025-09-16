using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет узлы, где фактическое количество строк значительно отличается от оценочного,
/// что может говорить о старой статистике или неправильной селективности.
/// </summary>
public sealed class MisestimatedRowsRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.MisestimatedRows;
    /// <inheritdoc />
    public string Category => "Statistics";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.PlanRows.HasValue != true || node.ActualRows.HasValue != true)
            return Task.FromResult<PlanFinding?>(null);

        var planRows = node.PlanRows.Value;
        var actualRows = node.ActualRows.Value;

        if (planRows == 0) return Task.FromResult<PlanFinding?>(null);

        double ratio = actualRows / planRows;
        if (ratio > 3 || ratio < 0.33) // более чем 3x отклонение
        {
            var relation = TryGetNodeSpecificString(node, "Relation Name");
            var metadata = new Dictionary<string, object?>
            {
                ["PlanRows"] = planRows,
                ["ActualRows"] = actualRows,
                ["Ratio"] = ratio
            };

            var affected = relation != null ? new List<string> { relation } : [];
            var message = relation != null
                ? $"Фактическое количество строк ({actualRows}) сильно отличается от оценочного ({planRows}) в таблице '{relation}'. Проверьте статистику."
                : $"Фактическое количество строк ({actualRows}) сильно отличается от оценочного ({planRows}). Проверьте статистику.";

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
