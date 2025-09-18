using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// Выявляет узлы, создающие чрезмерное количество временных файлов (Temp Written/Read),
/// что может замедлять выполнение и указывает на необходимость настройки work_mem или оптимизации запроса.
/// </summary>
public sealed class ExcessiveTempFilesRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.ExcessiveTempFiles;

    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.Buffers == null) return Task.FromResult<PlanFinding?>(null);

        var totalTemp = node.Buffers.TempRead + node.Buffers.TempWritten;
        if (totalTemp > 100) 
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                string.Format(ExplainRulePromblemDescriptions.ExcessiveTempFiles, node.NodeType, totalTemp),
                ExplainRuleRecommendations.ExcessiveTempFiles
            )); 
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}
