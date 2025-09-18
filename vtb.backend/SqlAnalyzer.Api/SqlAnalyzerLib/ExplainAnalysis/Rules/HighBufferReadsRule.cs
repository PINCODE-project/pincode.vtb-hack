using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет узлы, которые читают очень много блоков буферов (Shared/Local),
/// что может быть индикатором плохо оптимизированного запроса или отсутствия индексов.
/// </summary>
public sealed class HighBufferReadsRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.HighBufferReads;

    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node.Buffers == null)
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        var totalReads = node.Buffers.SharedRead + node.Buffers.LocalRead + node.Buffers.TempRead;
        if (totalReads > 1000)
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.HighBufferReads, node.NodeType, node.GetRelationName(), totalReads),
                ExplainRuleRecommendations.HighBufferReads
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    } 
}
