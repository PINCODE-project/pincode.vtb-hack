using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет Hash Aggregate узлы, где ключ не поддерживает хэширование, что может привести к ошибкам или fallback на Sort Aggregate.
/// </summary>
public sealed class HashAggWithoutHashableKeyRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.HashAggWithoutHashableKey;

    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("HashAggregate", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        if (node.NodeSpecific != null &&
            node.NodeSpecific.TryGetValue("Hash Key Type", out var keyTypeObj) &&
            keyTypeObj.ToString()?.Contains("non-hashable", StringComparison.OrdinalIgnoreCase) == true
           )
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.ExcessiveTempFiles, node.NodeType, keyTypeObj),
                ExplainRuleRecommendations.ExcessiveTempFiles
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}