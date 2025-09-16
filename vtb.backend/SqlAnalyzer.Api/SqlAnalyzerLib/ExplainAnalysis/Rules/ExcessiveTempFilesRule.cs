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
    public ExplainIssueRule Code => ExplainIssueRule.ExcessiveTempFiles;
    /// <inheritdoc />
    public string Category => "Performance";
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node?.Buffers == null) return Task.FromResult<PlanFinding?>(null);

        var totalTemp = node.Buffers.TempRead + node.Buffers.TempWritten;
        if (totalTemp > 100) // порог произвольный
        {
            var metadata = new Dictionary<string, object?>
            {
                ["TempRead"] = node.Buffers.TempRead,
                ["TempWritten"] = node.Buffers.TempWritten,
                ["PlanRows"] = node.PlanRows,
                ["ActualRows"] = node.ActualRows
            };

            var message = $"Узел '{node.NodeType}' создает большое количество временных файлов (TempRead+TempWritten={totalTemp}). Рассмотрите увеличение work_mem или оптимизацию запроса.";

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
