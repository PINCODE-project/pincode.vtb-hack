using System.Globalization;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

 /// <summary>
    /// P10: Последовательное сканирование со значительным количеством отфильтрованных строк -> рекомендовать индекс/анализ статистики.
    /// Условие: NodeType содержит 'Seq Scan', есть Filter в NodeSpecific, и RowsRemovedByFilter большое отношение к суммарным строкам.
    /// </summary>
    public sealed class SeqScanSelectiveRule : IPlanRule
    {
        /// <inheritdoc />
        public string Code => "P10";

        /// <inheritdoc />
        public string Category => "Scan";

        /// <inheritdoc />
        public Severity DefaultSeverity => Severity.High;

        /// <summary>
        /// Порог доли удалённых строк, при котором считаем проблему критичной.
        /// </summary>
        public double RemovedFractionThreshold { get; }

        /// <summary>
        /// Создаёт правило с настраиваемым порогом.
        /// </summary>
        /// <param name="removedFractionThreshold">Доля удалённых строк, например 0.5 (50%).</param>
        public SeqScanSelectiveRule(double removedFractionThreshold = 0.5)
        {
            RemovedFractionThreshold = removedFractionThreshold;
        }

        /// <inheritdoc />
        public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
        {
            if (node.ShortNodeType == null) return Task.FromResult<PlanFinding?>(null);
            if (!node.ShortNodeType.Equals("SeqScan", StringComparison.OrdinalIgnoreCase)) return Task.FromResult<PlanFinding?>(null);

            var nodeSpec = node.NodeSpecific;
            if (nodeSpec == null) return Task.FromResult<PlanFinding?>(null);

            double? rowsRemovedByFilter = TryGetDoubleFromNodeSpecific(nodeSpec, "Rows Removed by Filter");
            double? actualRows = node.ActualRows;
            double? planRows = node.PlanRows;

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
                            ["RemovedFraction"] = removedFraction,
                            ["RowsRemovedByFilter"] = rowsRemovedByFilter.Value,
                            ["ActualRows"] = actualRows.Value
                        };
                        var msg = "Seq Scan с фильтрацией отбрасывает значительную долю строк. Рекомендуется создать индекс для предиката или улучшить статистику.";
                        return Task.FromResult<PlanFinding?>(new PlanFinding(Code, msg, Category, DefaultSeverity, new List<string>(), metadata));
                    }
                }
            }
            else if (node.Buffers != null && (node.Buffers.SharedRead > 0 || node.Buffers.TempRead > 0))
            {
                var msg = "Seq Scan читает много блоков (IO). Проверьте селективность условий и наличие индексов.";
                var metadata = new Dictionary<string, object>
                {
                    ["SharedRead"] = node.Buffers.SharedRead,
                    ["TempRead"] = node.Buffers.TempRead
                };
                return Task.FromResult<PlanFinding?>(new PlanFinding(Code, msg, Category, Severity.Medium, new List<string>(), metadata));
            }

            return Task.FromResult<PlanFinding?>(null);
        }

        private static double? TryGetDoubleFromNodeSpecific(IReadOnlyDictionary<string, object> spec, string key)
        {
            if (!spec.TryGetValue(key, out var val)) return null;
            if (val == null) return null;
            if (val is double d) return d;
            if (val is long l) return Convert.ToDouble(l);
            if (double.TryParse(val.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)) return parsed;
            return null;
        }
    }