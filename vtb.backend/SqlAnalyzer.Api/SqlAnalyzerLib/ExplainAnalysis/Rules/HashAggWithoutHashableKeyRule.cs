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
    public ExplainIssueRule Code => ExplainIssueRule.HashAggWithoutHashableKey;
    /// <inheritdoc />
    public string Category => "Aggregate";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("HashAggregate", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        if (node.NodeSpecific != null && node.NodeSpecific.TryGetValue("Hash Key Type", out var keyTypeObj) &&
            keyTypeObj?.ToString()?.Contains("non-hashable", StringComparison.OrdinalIgnoreCase) == true)
        {
            var metadata = new Dictionary<string, object?>
            {
                ["HashKeyType"] = keyTypeObj,
                ["NodeType"] = node.NodeType
            };

            var message = $"Hash Aggregate узел '{node.NodeType}' использует неподдерживаемый для хэширования ключ ({keyTypeObj}). Рассмотрите Sort Aggregate или преобразование ключа.";

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
