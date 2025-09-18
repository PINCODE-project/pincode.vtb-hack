using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет узлы агрегации, которые потребляют большое количество памяти.
/// </summary>
public sealed class LargeAggregateMemoryRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.LargeAggregateMemory;
    
    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (!node.NodeType.Contains("Aggregate", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        var memoryUsage = node.TryGetNodeSpecificString("Memory Usage");
        if (double.TryParse(memoryUsage, out var memMb) && memMb > 50) 
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.LargeAggregateMemory, node.NodeType, memMb.ToString("F1")),
                ExplainRuleRecommendations.LargeAggregateMemory
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}
