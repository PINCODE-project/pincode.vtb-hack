using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет узлы Sort, которые используют внешний метод сортировки на диске.
/// </summary>
public sealed class SortMethodExternalRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.SortMethodExternal;
    /// <inheritdoc />
    public string Category => "Performance";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Sort", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        if (node.NodeSpecific != null &&
            node.NodeSpecific.TryGetValue("Sort Method", out var methodObj) &&
            methodObj?.ToString()?.Contains("external", StringComparison.OrdinalIgnoreCase) == true)
        {
            var metadata = new Dictionary<string, object?>
            {
                ["SortMethod"] = methodObj,
                ["NodeType"] = node.NodeType
            };

            var message = $"Sort узел '{node.NodeType}' spill’ит на диск (Sort Method={methodObj}). Рассмотрите увеличение work_mem или оптимизацию запроса.";

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
