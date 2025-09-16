using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет случаи, когда один и тот же Seq Scan повторяется несколько раз в пределах одного плана,
/// что может быть признаком отсутствия кэширования, дублирующихся подзапросов или плохой структуры запроса.
/// </summary>
public sealed class RepeatedSeqScanRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.RepeatedSeqScan;
    /// <inheritdoc />
    public string Category => "Performance";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node == null || rootPlan == null) return Task.FromResult<PlanFinding?>(null);

        var seqScans = new List<PlanNode>();
        CollectSeqScans(rootPlan.RootNode, seqScans);

        var duplicates = seqScans
            .GroupBy(n => TryGetNodeSpecificString(n, "Relation Name"))
            .Where(g => g.Count() > 1 && g.Key != null)
            .SelectMany(g => g)
            .ToList();

        if (duplicates.Count > 0)
        {
            var metadata = new Dictionary<string, object?>
            {
                ["RepeatedSeqScans"] = duplicates.Count
            };

            var affected = duplicates
                .Select(n => TryGetNodeSpecificString(n, "Relation Name"))
                .Where(n => n != null)
                .Distinct()
                .ToList();

            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                $"Найдены повторяющиеся Seq Scan для таблиц: {string.Join(", ", affected)}. Рассмотрите возможность оптимизации запросов или использования CTE.",
                Category,
                DefaultSeverity,
                affected,
                metadata
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }

    private static void CollectSeqScans(PlanNode node, List<PlanNode> collector)
    {
        if (node == null) return;

        if (node.NodeType != null && node.NodeType.Contains("Seq Scan", StringComparison.OrdinalIgnoreCase))
            collector.Add(node);

        if (node.Children != null)
            foreach (var c in node.Children)
                CollectSeqScans(c, collector);
    }

    private static string? TryGetNodeSpecificString(PlanNode node, string key)
        => node.NodeSpecific != null && node.NodeSpecific.TryGetValue(key, out var v) && v != null ? v.ToString() : null;
}
