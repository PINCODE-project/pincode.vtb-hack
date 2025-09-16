using System.Globalization;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules
{
    /// <summary>
    /// P10: Последовательное сканирование со значительным количеством отфильтрованных строк -> рекомендовать индекс/анализ статистики.
    /// Улучшена детекция и метаданные.
    /// </summary>
    public sealed class SeqScanSelectiveRule : IPlanRule
    {
        public ExplainIssueRule Code => ExplainIssueRule.SeqScanSelective;
        public string Category => "Scan";
        public Severity DefaultSeverity => Severity.High;

        /// <summary>Порог доли удалённых строк, при котором считаем проблему критичной.</summary>
        public double RemovedFractionThreshold { get; }

        /// <summary>Порог плановых/фактических строк, при котором считаем таблицу большой.</summary>
        public long LargeTableRowsThreshold { get; }

        public SeqScanSelectiveRule(double removedFractionThreshold = 0.5, long largeTableRowsThreshold = 100_000)
        {
            RemovedFractionThreshold = removedFractionThreshold;
            LargeTableRowsThreshold = largeTableRowsThreshold;
        }

        public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
        {
            if (node == null) return Task.FromResult<PlanFinding?>(null);

            var shortNode = node.ShortNodeType ?? node.NodeType;
            if (string.IsNullOrEmpty(shortNode)) return Task.FromResult<PlanFinding?>(null);
            if (!shortNode.Equals("SeqScan", StringComparison.OrdinalIgnoreCase) &&
                !shortNode.Equals("Seq Scan", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult<PlanFinding?>(null);

            var nodeSpec = node.NodeSpecific;
            if (nodeSpec == null) return Task.FromResult<PlanFinding?>(null);

            // Try to get useful numbers
            double? rowsRemovedByFilter = TryGetDoubleFromNodeSpecific(nodeSpec, "Rows Removed by Filter");
            double? actualRows = node.ActualRows ?? TryGetDoubleFromNodeSpecific(nodeSpec, "Actual Rows");
            double? planRows = node.PlanRows ?? TryGetDoubleFromNodeSpecific(nodeSpec, "Plan Rows");

            // relation name for diagnostics
            var tableName = nodeSpec.TryGetValue("Relation Name", out var rn) ? (rn?.ToString() ?? "unknown") : "unknown";

            // 1) High fraction of rows removed by filter -> suggests index on predicate
            if (rowsRemovedByFilter.HasValue && actualRows.HasValue)
            {
                var totalConsidered = actualRows.Value + rowsRemovedByFilter.Value;
                if (totalConsidered > 0)
                {
                    var removedFraction = rowsRemovedByFilter.Value / totalConsidered;
                    if (removedFraction >= RemovedFractionThreshold)
                    {
                        var metadata = new Dictionary<string, object>
                        {
                            ["Table"] = tableName,
                            ["RemovedFraction"] = removedFraction,
                            ["RowsRemovedByFilter"] = rowsRemovedByFilter.Value,
                            ["ActualRows"] = actualRows.Value,
                            ["PlanRows"] = planRows ?? (object)"unknown"
                        };
                        var msg = $"Seq Scan на {tableName} отбрасывает большую долю строк ({removedFraction:P1}). " +
                                  "Рекомендуется создать индекс по предикату (или пересмотреть WHERE/JOIN), обновить статистику (ANALYZE) или использовать предагрегацию.";
                        return Task.FromResult<PlanFinding?>(new PlanFinding(Code, msg, Category, DefaultSeverity, new List<string>(), metadata));
                    }
                }
            }

            // 2) IO heavy seq scan -> hint about indexes or statistics
            if (node.Buffers != null && (node.Buffers.SharedRead > 0 ||
                                         node.Buffers.TempRead > 0))
            {
                var meta = new Dictionary<string, object>
                {
                    ["Table"] = tableName,
                    ["SharedRead"] = node.Buffers.SharedRead,
                    ["TempRead"] = node.Buffers.TempRead,
                    ["PlanRows"] = planRows ?? (object)"unknown",
                    ["ActualRows"] = actualRows ?? (object)"unknown"
                };
                var msg = $"Seq Scan на {tableName} читает много блоков (IO). Проверьте селективность условий и наличие индексов; рассмотрите CREATE INDEX CONCURRENTLY или предагрегацию.";
                return Task.FromResult<PlanFinding?>(new PlanFinding(Code, msg, Category, Severity.Medium, new List<string>(), meta));
            }

            // 3) Very large table scanned (plan rows or actual rows exceed threshold)
            if (planRows.HasValue && planRows.Value >= LargeTableRowsThreshold ||
                actualRows.HasValue && actualRows.Value >= LargeTableRowsThreshold)
            {
                var meta = new Dictionary<string, object>
                {
                    ["Table"] = tableName,
                    ["PlanRows"] = planRows ?? (object)"unknown",
                    ["ActualRows"] = actualRows ?? (object)"unknown"
                };
                var msg = $"Seq Scan на большую таблицу {tableName} (rows >= {LargeTableRowsThreshold:N0}). " +
                          "Рекомендуется индекс по нужным колонкам, пересмотреть JOIN/WHERE (EXISTS/INNER JOIN) или использовать предагрегированные/материализованные данные.";
                return Task.FromResult<PlanFinding?>(new PlanFinding(Code, msg, Category, Severity.Medium, new List<string>(), meta));
            }

            return Task.FromResult<PlanFinding?>(null);
        }

        private static double? TryGetDoubleFromNodeSpecific(IReadOnlyDictionary<string, object> spec, string key)
        {
            if (spec == null) return null;
            if (!spec.TryGetValue(key, out var val)) return null;
            if (val == null) return null;
            if (val is double d) return d;
            if (val is float f) return Convert.ToDouble(f);
            if (val is long l) return Convert.ToDouble(l);
            if (val is int i) return Convert.ToDouble(i);
            if (double.TryParse(val.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                return parsed;
            return null;
        }
    }
}
