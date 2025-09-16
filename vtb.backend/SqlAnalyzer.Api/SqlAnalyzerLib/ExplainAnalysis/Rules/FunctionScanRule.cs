using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Обнаруживает узлы типа Function Scan, которые могут быть источником медленных операций или проблем с планированием.
/// </summary>
public sealed class FunctionScanRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.FunctionScan;
    /// <inheritdoc />
    public string Category => "Performance";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Function Scan", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        var functionName = TryGetNodeSpecificString(node, "Function Name");

        var metadata = new Dictionary<string, object?>
        {
            ["PlanRows"] = node.PlanRows,
            ["ActualRows"] = node.ActualRows,
            ["FunctionName"] = functionName
        };

        var affected = functionName != null ? new List<string> { functionName } : [];
        var message = functionName != null
            ? $"Function Scan по '{functionName}' может быть медленным. Рассмотрите оптимизацию функции или материализацию данных."
            : $"Function Scan может быть медленным. Рассмотрите оптимизацию функции или материализацию данных.";

        return Task.FromResult<PlanFinding?>(new PlanFinding(
            Code,
            message,
            Category,
            DefaultSeverity,
            affected,
            metadata
        ));
    }

    private static string? TryGetNodeSpecificString(PlanNode node, string key)
        => node.NodeSpecific != null && node.NodeSpecific.TryGetValue(key, out var v) && v != null ? v.ToString() : null;
}
