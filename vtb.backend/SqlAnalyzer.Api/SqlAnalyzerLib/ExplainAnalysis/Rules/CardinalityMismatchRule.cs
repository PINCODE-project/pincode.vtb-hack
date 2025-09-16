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
    public ExplainIssueRule Code => ExplainIssueRule.CardinalityMismatch;

    /// <inheritdoc />
    public string Category => "Cardinality";

    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (!node.PlanRows.HasValue || !node.ActualRows.HasValue) return Task.FromResult<PlanFinding?>(null);

        var est = node.PlanRows.Value;
        var act = node.ActualRows.Value * Math.Max(1, node.ActualLoops ?? 1);
        if (act <= 0 || est <= 0) return Task.FromResult<PlanFinding?>(null);

        var ratio = est / act;
        if (ratio > 4 || ratio < 0.25)
        {
            var metadata = new Dictionary<string, object>
            {
                ["EstimatedRows"] = est,
                ["ActualRows"] = act,
                ["Ratio"] = ratio
            };
            var msg = "Сильное расхождение между оценкой планировщика и фактическим числом строк. Рекомендуется ANALYZE, увеличение статистики по колонкам и создание CREATE STATISTICS для коррелирующих колонок.";
            return Task.FromResult<PlanFinding?>(new PlanFinding(Code, msg, Category, DefaultSeverity, new List<string>(), metadata));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}