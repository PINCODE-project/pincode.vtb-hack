using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Обнаруживает узлы с большим количеством повторных итераций (loops),
/// что может указывать на Nested Loop по большим таблицам или неэффективные соединения.
/// </summary>
public sealed class LargeNumberOfLoopsRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.LargeNumberOfLoops;
    
    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node.ActualLoops.HasValue == false)
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        if (node.ActualLoops.Value > 1000)
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.LargeNumberOfLoops, node.NodeType, node.GetRelationName(), node.ActualLoops),
                ExplainRuleRecommendations.LargeNumberOfLoops
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}
