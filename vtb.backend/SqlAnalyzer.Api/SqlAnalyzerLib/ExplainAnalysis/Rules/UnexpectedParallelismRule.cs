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
    public ExplainIssueRule Code => ExplainIssueRule.UnexpectedParallelism;
    /// <inheritdoc />
    public string Category => "Parallelism";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.ActualLoops.HasValue != true) return Task.FromResult<PlanFinding?>(null);

        if (node.ActualLoops > 1 && node.NodeType?.Contains("Parallel") == false)
        {
            var metadata = new Dictionary<string, object?>
            {
                ["NodeType"] = node.NodeType,
                ["ActualLoops"] = node.ActualLoops,
                ["PlanRows"] = node.PlanRows,
                ["ActualRows"] = node.ActualRows
            };

            var message = $"Узел '{node.NodeType}' исполнялся параллельно (loops={node.ActualLoops}), но не является Parallel узлом. Возможно неэффективная параллельная обработка.";

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
