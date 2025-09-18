using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет последовательные сканы (Seq Scan) по больших таблицах без индекса.
/// </summary>
public sealed class SeqScanOnLargeTableRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.SeqScanOnLargeTable;
    
    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (!node.NodeType.Contains("Seq Scan", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        var planRows = node.PlanRows ?? 0d;
        if (planRows > 10000) 
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.SeqScanOnLargeTable, node.GetRelationName(), planRows),
                ExplainRuleRecommendations.SeqScanOnLargeTable
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
    
}
