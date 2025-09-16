using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет Seq Scan узлы с высоким количеством временных файлов.
/// </summary>
public sealed class SeqScanWithHighTempWritesRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.SeqScanWithHighTempWrites;

    /// <inheritdoc />
    public string Category => "Performance";

    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Seq Scan", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        if (node.Buffers != null && node.Buffers.TempWritten > 50)
        {
            var relation = TryGetNodeSpecificString(node, "Relation Name");
            var metadata = new Dictionary<string, object?>
            {
                ["TempWritten"] = node.Buffers.TempWritten,
                ["NodeType"] = node.NodeType
            };

            var affected = relation != null ? new List<string> { relation } : [];
            var message = relation != null
                ? $"Seq Scan на таблице '{relation}' создает большое количество временных файлов ({node.Buffers.TempWritten}). Проверьте work_mem."
                : $"Seq Scan создает большое количество временных файлов ({node.Buffers.TempWritten}). Проверьте work_mem.";

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
        => node.NodeSpecific != null && node.NodeSpecific.TryGetValue(key, out var v) && v != null
            ? v.ToString()
            : null;
}