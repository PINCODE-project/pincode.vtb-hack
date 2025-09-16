using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Обнаруживает узлы Sort, которые переполняют память и записывают данные на диск (spill),
/// что может сильно замедлять выполнение запросов.
/// </summary>
public sealed class SortSpillToDiskRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.SortSpillToDisk;
    /// <inheritdoc />
    public string Category => "Performance";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Sort", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        var tempWritten = node.Buffers?.TempWritten ?? 0;
        if (tempWritten > 0)
        {
            var metadata = new Dictionary<string, object?>
            {
                ["TempWritten"] = tempWritten,
                ["PlanRows"] = node.PlanRows,
                ["ActualRows"] = node.ActualRows,
                ["SortMethod"] = TryGetNodeSpecificString(node, "Sort Method")
            };

            var message = $"Sort node выполняет запись во временные файлы (spill) — TempWritten={tempWritten}. Рассмотрите увеличение work_mem или индексированную сортировку.";

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

    private static string? TryGetNodeSpecificString(PlanNode node, string key)
        => node.NodeSpecific != null && node.NodeSpecific.TryGetValue(key, out var v) && v != null ? v.ToString() : null;
}
