using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Обнаруживает Seq Scan с LIKE '%...' в условии, что делает использование индекса невозможным.
/// </summary>
public sealed class LeadingWildcardLikeRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.LeadingWildcardLike;
    
    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null) return Task.FromResult<PlanFinding?>(null);

        if (node.NodeType.Contains("Seq Scan", StringComparison.OrdinalIgnoreCase) &&
            node.NodeSpecific != null && 
            node.NodeSpecific.Values.Any(v => v?.ToString()?.Contains("like", StringComparison.OrdinalIgnoreCase) == true))
        {

            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.LeadingWildcardLike, node.GetRelationName()),
                ExplainRuleRecommendations.LeadingWildcardLike
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
    
}
