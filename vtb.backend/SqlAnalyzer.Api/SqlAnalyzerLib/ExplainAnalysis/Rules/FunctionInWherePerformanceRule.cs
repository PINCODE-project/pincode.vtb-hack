using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет случаи, когда фильтрация выполняется через функцию в WHERE или JOIN,
/// что делает индекс недействующим и замедляет выполнение.
/// </summary>
public sealed class FunctionInWherePerformanceRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.FunctionInWherePerformance;
    /// <inheritdoc />
    public string Category => "Rewrite";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null) return Task.FromResult<PlanFinding?>(null);

        if ((node.NodeType.Contains("Seq Scan", StringComparison.OrdinalIgnoreCase) ||
             node.NodeType.Contains("Bitmap Heap Scan", StringComparison.OrdinalIgnoreCase)) &&
            node.NodeSpecific != null && node.NodeSpecific.Values.Any(v => v?.ToString()?.Contains("function", StringComparison.OrdinalIgnoreCase) == true))
        {
            var relation = TryGetNodeSpecificString(node, "Relation Name");
            var metadata = new Dictionary<string, object?>
            {
                ["NodeType"] = node.NodeType,
                ["PlanRows"] = node.PlanRows,
                ["ActualRows"] = node.ActualRows
            };

            var affected = relation != null ? new List<string> { relation } : [];
            var message = relation != null
                ? $"Функция используется в фильтре по таблице '{relation}', индексы не применяются. Рассмотрите переписывание условия."
                : "Функция используется в фильтре, индексы не применяются. Рассмотрите переписывание условия.";

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
