using SqlAnalyzer.Api.Dal.Constants;
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
    public ExplainRules Code => ExplainRules.SlowStartupTime;

    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (!node.ActualStartupTimeMs.HasValue)
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        if (node.ActualStartupTimeMs.Value > 50) 
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.SlowStartupTime, node.NodeType, node.ActualStartupTimeMs),
                ExplainRuleRecommendations.SlowStartupTime
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}
