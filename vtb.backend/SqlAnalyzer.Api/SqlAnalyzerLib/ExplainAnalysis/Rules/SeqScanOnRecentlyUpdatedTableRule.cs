using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет Seq Scan на таблицах с частыми обновлениями, где может быть полезен индекс или VACUUM.
/// </summary>
public sealed class SeqScanOnRecentlyUpdatedTableRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.SeqScanOnRecentlyUpdatedTable;

    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (!node.NodeType.Contains("Seq Scan", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        var recentUpdates = node.TryGetNodeSpecificString("Recent Updates");
        if (int.TryParse(recentUpdates, out var updates) && updates > 1000)
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.SeqScanOnRecentlyUpdatedTable, node.GetRelationName(), updates),
                ExplainRuleRecommendations.SeqScanOnRecentlyUpdatedTable
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
    
}
