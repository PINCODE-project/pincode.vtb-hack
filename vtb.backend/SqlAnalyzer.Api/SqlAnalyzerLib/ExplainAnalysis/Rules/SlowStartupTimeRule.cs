using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет узлы с высоким фактическим стартовым временем, что может сигнализировать о долгой подготовке или инициализации.
/// </summary>
public sealed class SlowStartupTimeRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.SlowStartupTime;
    /// <inheritdoc />
    public string Category => "Performance";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (!node.ActualStartupTimeMs.HasValue) return Task.FromResult<PlanFinding?>(null);

        if (node.ActualStartupTimeMs.Value > 50) // порог в миллисекундах
        {
            var metadata = new Dictionary<string, object?>
            {
                ["ActualStartupTimeMs"] = node.ActualStartupTimeMs,
                ["NodeType"] = node.NodeType
            };

            var message = $"Узел '{node.NodeType}' имеет длительное стартовое время ({node.ActualStartupTimeMs:F1} мс). Рассмотрите оптимизацию запроса или структуры данных.";

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
