using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// P50: Ошибка оценки кардинальности (Plan Rows vs Actual Rows).
/// Если отношение оценки/факта > 4 или < 0.25 — предложение пересчитать статистику/создать extended statistics.
/// </summary>
public sealed class CardinalityMismatchRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.CardinalityMismatch;

    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (!node.PlanRows.HasValue || !node.ActualRows.HasValue) return Task.FromResult<PlanFinding?>(null);

        var est = node.PlanRows.Value;
        var act = node.ActualRows.Value * Math.Max(1, node.ActualLoops ?? 1);
        if (act <= 0 || est <= 0)
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        var ratio = est / act;
        if (ratio is > 4 or < 0.25)
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                ExplainRulePromblemDescriptions.CardinalityMismatch,
                ExplainRuleRecommendations.CardinalityMismatch
            )); 
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}