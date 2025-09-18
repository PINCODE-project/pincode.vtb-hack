using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет узлы с сильными расхождениями между оценочными и фактическими строками, что может быть вызвано устаревшей или отсутствующей статистикой.
/// </summary>
public sealed class MissingStatisticsRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.MissingStatistics;
    
    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (!node.PlanRows.HasValue || !node.ActualRows.HasValue) return Task.FromResult<PlanFinding?>(null);

        var ratio = node.ActualRows.Value / (node.PlanRows.Value + 1);
        if (ratio is > 5 or < 0.2)
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.MissingStatistics, node.GetRelationName()),
                ExplainRuleRecommendations.MissingStatistics
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}
