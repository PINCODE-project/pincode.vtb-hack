using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет Seq Scan узлы с высоким количеством временных файлов.
/// </summary>
public sealed class SeqScanWithHighTempWritesRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.SeqScanWithHighTempWrites;

    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (!node.NodeType.Contains("Seq Scan", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<PlanFinding?>(null);
        }

        if (node.Buffers is { TempWritten: > 50 })
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.SeqScanWithHighTempWrites, node.GetRelationName(), node.Buffers.TempWritten),
                ExplainRuleRecommendations.SeqScanWithHighTempWrites
            ));
        }

        return Task.FromResult<PlanFinding?>(null);
    }           
}