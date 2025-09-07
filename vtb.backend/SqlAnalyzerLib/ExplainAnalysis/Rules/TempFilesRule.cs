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
    public string Code => "P61";

    /// <inheritdoc />
    public string Category => "TempIO";

    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node.Buffers == null) return Task.FromResult<PlanFinding?>(null);
        if (node.Buffers.TempWritten > 0 || node.Buffers.TempRead > 0)
        {
            var metadata = new Dictionary<string, object>
            {
                ["TempWritten"] = node.Buffers.TempWritten,
                ["TempRead"] = node.Buffers.TempRead
            };
            var msg = "Узел использует временные блоки (temp files) — возможны спиллы на диск (sort/hash spill). Проверьте work_mem и порядок сортировки/размер хэша.";
            return Task.FromResult<PlanFinding?>(new PlanFinding(Code, msg, Category, DefaultSeverity, new List<string>(), metadata));
        }
        return Task.FromResult<PlanFinding?>(null);
    }
}