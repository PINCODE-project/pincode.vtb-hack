using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет узлы, где фильтр (Filter/WHERE) применяется после агрегирования, что может быть неэффективно.
/// </summary>
public sealed class FilterAfterAggregateRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.FilterAfterAggregate;
    /// <inheritdoc />
    public string Category => "Rewrite";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.NodeType == null || !node.NodeType.Contains("Aggregate", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        if (node.NodeSpecific != null && node.NodeSpecific.ContainsKey("Filter"))
        {
            var metadata = new Dictionary<string, object?>
            {
                ["NodeType"] = node.NodeType,
                ["Filter"] = node.NodeSpecific["Filter"]
            };

            var message = $"Фильтр применяется после агрегирования на узле '{node.NodeType}'. Рассмотрите возможность переноса фильтра до агрегирования для оптимизации.";

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
