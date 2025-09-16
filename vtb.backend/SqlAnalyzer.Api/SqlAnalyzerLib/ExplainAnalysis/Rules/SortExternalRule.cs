using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules
{
    /// <summary>
    /// P30: External sort — сортировка использует диск или метод external.
    /// </summary>
    public sealed class SortExternalRule : IPlanRule
    {
        public ExplainIssueRule Code => ExplainIssueRule.SortExternal;
        public string Category => "Sort";
        public Severity DefaultSeverity => Severity.High;

        public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
        {
            if (node == null || string.IsNullOrEmpty(node.NodeType)) return Task.FromResult<PlanFinding?>(null);
            if (!node.NodeType.Contains("Sort", StringComparison.OrdinalIgnoreCase) &&
                !(node.ShortNodeType?.Contains("Sort", StringComparison.OrdinalIgnoreCase) ?? false))
                return Task.FromResult<PlanFinding?>(null);

            if (node.NodeSpecific != null)
            {
                var sortMethod = node.NodeSpecific.TryGetValue("Sort Method", out var sm) ? sm?.ToString() : null;
                var sortSpaceType = node.NodeSpecific.TryGetValue("Sort Space Type", out var sst) ? sst?.ToString() : null;

                if (!string.IsNullOrEmpty(sortMethod) && sortMethod.IndexOf("external", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var meta = new Dictionary<string, object> { ["SortMethod"] = sortMethod! };
                    var msg = "Сортировка использует внешнюю память (external) — возможен spill. Рассмотрите увеличение work_mem или создание индекса, покрывающего ORDER BY.";
                    return Task.FromResult<PlanFinding?>(new PlanFinding(Code, msg, Category, DefaultSeverity, new List<string>(), meta));
                }

                if (!string.IsNullOrEmpty(sortSpaceType) && sortSpaceType.IndexOf("disk", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var meta = new Dictionary<string, object> { ["SortSpaceType"] = sortSpaceType! };
                    var msg = "Sort Space Type указывает на диск — сортировка выполняется с использованием временных файлов. Увеличьте work_mem или уменьшите объём сортируемых данных.";
                    return Task.FromResult<PlanFinding?>(new PlanFinding(Code, msg, Category, DefaultSeverity, new List<string>(), meta));
                }
            }

            if (node.Buffers != null && node.Buffers.TempWritten > 0)
            {
                var meta = new Dictionary<string, object> { ["TempWritten"] = node.Buffers.TempWritten };
                var msg = "Сортировка записывает временные блоки на диск (TempWritten > 0). Увеличьте work_mem или уменьшите сортируемый набор; рассмотрите индекс для ORDER BY.";
                return Task.FromResult<PlanFinding?>(new PlanFinding(Code, msg, Category, DefaultSeverity, new List<string>(), meta));
            }

            return Task.FromResult<PlanFinding?>(null);
        }
    }
}
