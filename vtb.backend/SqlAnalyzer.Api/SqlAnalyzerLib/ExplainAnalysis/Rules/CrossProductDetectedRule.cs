using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет узлы Nested Loop без join condition (кросс-произведение), что может сильно замедлять выполнение.
/// </summary>
public sealed class CrossProductDetectedRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.CrossProductDetected;
    /// <inheritdoc />
    public string Category => "Rewrite";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Critical;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Nested Loop", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        if (node.NodeSpecific != null && !node.NodeSpecific.ContainsKey("Join Filter"))
        {
            var metadata = new Dictionary<string, object?>
            {
                ["NodeType"] = node.NodeType
            };

            var message = $"Nested Loop без join condition обнаружен на узле '{node.NodeType}'. Это кросс-произведение, которое может резко замедлить выполнение.";

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
