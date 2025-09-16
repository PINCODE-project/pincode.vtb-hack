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
    public ExplainIssueRule Code => ExplainIssueRule.WorkMemExceededEstimate;
    /// <inheritdoc />
    public string Category => "Memory";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeSpecific == null) return Task.FromResult<PlanFinding?>(null);

        if (node.NodeSpecific.TryGetValue("TempFiles", out var tempFilesObj) &&
            long.TryParse(tempFilesObj.ToString(), out var tempFiles) && tempFiles > 0)
        {
            var metadata = new Dictionary<string, object?>
            {
                ["TempFiles"] = tempFiles,
                ["NodeType"] = node.NodeType
            };

            var message = $"Узел '{node.NodeType}' использует временные файлы ({tempFiles}), возможно превышение work_mem.";

            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                message,
                Category,
                DefaultSeverity,
                Array.Empty<string>(),
                metadata
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}
