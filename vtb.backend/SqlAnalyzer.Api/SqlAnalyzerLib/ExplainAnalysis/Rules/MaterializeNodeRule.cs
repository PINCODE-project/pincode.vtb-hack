using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет Materialize узлы, которые могут быть лишними и замедлять выполнение, если данные можно было бы вычислить напрямую.
/// </summary>
public sealed class MaterializeNodeRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.MaterializeNode;
    /// <inheritdoc />
    public string Category => "Performance";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Low;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Materialize", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        var metadata = new Dictionary<string, object?>
        {
            ["PlanRows"] = node.PlanRows,
            ["ActualRows"] = node.ActualRows
        };

        var message = $"Materialize узел может быть лишним и замедлять выполнение запроса. Рассмотрите использование прямого вычисления или CTE.";

        return Task.FromResult<PlanFinding?>(new PlanFinding(
            Code,
            message,
            Category,
            DefaultSeverity,
            Array.Empty<string>(),
            metadata
        ));
    }
}
