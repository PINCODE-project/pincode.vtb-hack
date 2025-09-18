using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// P22: Nested Loop с дорогой внутренней стороной — внутренний узел выполняется много раз при высоком ActualLoops и высокой стоимости.
/// </summary>
public sealed class NestedLoopHeavyInnerRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.NestedLoopHeavyInner;

    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node.ShortNodeType == null ||
            !node.ShortNodeType.Equals("NestedLoop", StringComparison.OrdinalIgnoreCase) ||
            node.Children == null || node.Children.Count < 2
           )
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        var inner = node.Children[1];
        var loops = node.ActualLoops ?? 1;
        var innerTime = inner.ActualTotalTimeMs ?? 0;
        
        if (loops > 10 && innerTime > 5.0) 
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                ExplainRulePromblemDescriptions.NestedLoopHeavyInner,
                ExplainRuleRecommendations.NestedLoopHeavyInner
            ));}

        return Task.FromResult<PlanFinding?>(null);
    }
}