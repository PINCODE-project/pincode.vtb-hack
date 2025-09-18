using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// P60/P61: Временные файлы/Temp blocks — признак спиллов/высокой записи в tmp.
/// </summary>
public sealed class TempFilesRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.TempFileSortSpill;

    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node.Buffers == null) return Task.FromResult<PlanFinding?>(null);
        if (node.Buffers.TempWritten > 0 || node.Buffers.TempRead > 0)
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                ExplainRulePromblemDescriptions.TempFileSortSpill,
                ExplainRuleRecommendations.TempFileSortSpill
            ));
            
        }
        return Task.FromResult<PlanFinding?>(null);
    }
}