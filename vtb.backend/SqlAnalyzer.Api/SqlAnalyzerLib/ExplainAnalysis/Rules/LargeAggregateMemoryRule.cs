using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет узлы агрегации, которые потребляют большое количество памяти.
/// </summary>
public sealed class LargeAggregateMemoryRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.LargeAggregateMemory;
    /// <inheritdoc />
    public string Category => "Memory";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Aggregate", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        if (node.NodeSpecific != null &&
            node.NodeSpecific.TryGetValue("Memory Usage", out var memObj) &&
            double.TryParse(memObj.ToString(), out var memMb) && memMb > 50) // порог памяти в МБ
        {
            var metadata = new Dictionary<string, object?>
            {
                ["MemoryUsageMB"] = memMb,
                ["NodeType"] = node.NodeType
            };

            var message = $"Агрегатный узел '{node.NodeType}' использует много памяти ({memMb:F1} MB). Рассмотрите оптимизацию агрегатов или увеличение work_mem.";

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
