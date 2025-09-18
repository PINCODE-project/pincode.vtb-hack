using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет случаи, когда один и тот же Seq Scan повторяется несколько раз в пределах одного плана,
/// что может быть признаком отсутствия кэширования, дублирующихся подзапросов или плохой структуры запроса.
/// </summary>
public sealed class RepeatedSeqScanRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.RepeatedSeqScan;
    
    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        var seqScans = new List<PlanNode>();
        CollectSeqScans(rootPlan.RootNode, seqScans);

        var duplicates = seqScans
            .GroupBy(n => n.GetRelationName())
            .Where(g => g.Count() > 1)
            .SelectMany(g => g)
            .ToList();

        var affected = duplicates
            .Select(n => n.GetRelationName())
            .Distinct()
            .ToList();
        if (affected.Count > 0)
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.RepeatedSeqScan, string.Join(", ", affected)),
                ExplainRuleRecommendations.RepeatedSeqScan
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }

    private static void CollectSeqScans(PlanNode node, List<PlanNode> collector)
    {
        if (node.NodeType != null && node.NodeType.Contains("Seq Scan", StringComparison.OrdinalIgnoreCase))
            collector.Add(node);

        if (node.Children != null)
            foreach (var c in node.Children)
                CollectSeqScans(c, collector);
    }}
