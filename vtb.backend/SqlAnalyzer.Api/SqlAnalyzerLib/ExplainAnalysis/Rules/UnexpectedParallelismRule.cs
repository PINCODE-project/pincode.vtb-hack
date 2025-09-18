using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Обнаруживает случаи неожиданной или чрезмерной параллельности, которая не даёт прироста производительности.
/// </summary>
public sealed class UnexpectedParallelismRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.UnexpectedParallelism;
    
    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.ActualLoops.HasValue != true) return Task.FromResult<PlanFinding?>(null);

        if (node.ActualLoops > 1 && node.NodeType?.Contains("Parallel") == false)
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.UnexpectedParallelism, node.NodeType, node.ActualLoops),
                ExplainRuleRecommendations.UnexpectedParallelism
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}
