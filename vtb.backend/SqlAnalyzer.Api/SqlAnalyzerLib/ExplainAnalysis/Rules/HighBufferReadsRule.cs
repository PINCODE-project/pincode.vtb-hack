using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет узлы, которые читают очень много блоков буферов (Shared/Local),
/// что может быть индикатором плохо оптимизированного запроса или отсутствия индексов.
/// </summary>
public sealed class HighBufferReadsRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.HighBufferReads;
    /// <inheritdoc />
    public string Category => "Performance";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.Buffers == null) return Task.FromResult<PlanFinding?>(null);

        long totalReads = node.Buffers.SharedRead + node.Buffers.LocalRead + node.Buffers.TempRead;
        if (totalReads > 1000) // порог в блоках
        {
            var relation = TryGetNodeSpecificString(node, "Relation Name");

            var metadata = new Dictionary<string, object?>
            {
                ["TotalBufferReads"] = totalReads,
                ["SharedRead"] = node.Buffers.SharedRead,
                ["LocalRead"] = node.Buffers.LocalRead,
                ["TempRead"] = node.Buffers.TempRead
            };

            var affected = relation != null ? new List<string> { relation } : [];
            var message = relation != null
                ? $"Узел '{node.NodeType}' по таблице '{relation}' выполняет большое количество чтений буферов ({totalReads} блоков). Рассмотрите индексы и фильтры."
                : $"Узел '{node.NodeType}' выполняет большое количество чтений буферов ({totalReads} блоков). Рассмотрите индексы и фильтры.";

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
