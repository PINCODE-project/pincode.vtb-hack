using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет узлы, которые потенциально превышают значение work_mem и могут создавать временные файлы.
/// </summary>
public sealed class WorkMemExceededEstimateRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.WorkMemExceededEstimate;

    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeSpecific == null) return Task.FromResult<PlanFinding?>(null);

        var tempFilesObj = node.TryGetNodeSpecificString("TempFiles");
        if (long.TryParse(tempFilesObj, out var tempFiles) && tempFiles > 0)
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.WorkMemExceededEstimate, node.NodeType, tempFiles),
                ExplainRuleRecommendations.WorkMemExceededEstimate
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}