using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Обнаруживает узлы Sort, которые переполняют память и записывают данные на диск (spill),
/// что может сильно замедлять выполнение запросов.
/// </summary>
public sealed class SortSpillToDiskRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.SortSpillToDisk;

    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Sort", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        var tempWritten = node.Buffers?.TempWritten ?? 0;
        if (tempWritten > 0)
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.SortSpillToDisk, tempWritten),
                ExplainRuleRecommendations.SortSpillToDisk
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
    
}
