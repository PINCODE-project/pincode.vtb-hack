using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет узлы, где фактическое количество строк значительно отличается от оценочного,
/// что может говорить о старой статистике или неправильной селективности.
/// </summary>
public sealed class MisestimatedRowsRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.MisestimatedRows;

    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.PlanRows.HasValue != true || node.ActualRows.HasValue != true)
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        var planRows = node.PlanRows.Value;
        var actualRows = node.ActualRows.Value;

        if (planRows == 0)
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        var ratio = actualRows / planRows;
        if (ratio is > 3 or < 0.33) 
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.MisestimatedRows, actualRows, planRows, node.GetRelationName()),
                ExplainRuleRecommendations.MisestimatedRows
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
    
}
