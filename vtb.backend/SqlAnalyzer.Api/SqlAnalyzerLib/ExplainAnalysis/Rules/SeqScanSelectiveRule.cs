using System.Globalization;
using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
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
        public ExplainRules Code => ExplainRules.SeqScanSelective;

        public Severity Severity => Severity.Critical;

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
            var shortNode = node.ShortNodeType ?? node.NodeType;
            if (string.IsNullOrEmpty(shortNode)) return Task.FromResult<PlanFinding?>(null);
            if (!shortNode.Equals("SeqScan", StringComparison.OrdinalIgnoreCase) &&
                !shortNode.Equals("Seq Scan", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult<PlanFinding?>(null);

            var nodeSpec = node.NodeSpecific;
            if (nodeSpec == null) return Task.FromResult<PlanFinding?>(null);

            // Try to get useful numbers
            var rowsRemovedByFilter = TryGetDoubleFromNodeSpecific(nodeSpec, "Rows Removed by Filter");
            var actualRows = node.ActualRows ?? TryGetDoubleFromNodeSpecific(nodeSpec, "Actual Rows");
            var planRows = node.PlanRows ?? TryGetDoubleFromNodeSpecific(nodeSpec, "Plan Rows");

            // relation name for diagnostics
            var tableName = node.GetRelationName();
            if (rowsRemovedByFilter.HasValue && actualRows.HasValue)
            {
                var totalConsidered = actualRows.Value + rowsRemovedByFilter.Value;
                if (totalConsidered > 0)
                {
                    var removedFraction = rowsRemovedByFilter.Value / totalConsidered;
                    if (removedFraction >= RemovedFractionThreshold)
                    {
                        return Task.FromResult<PlanFinding?>(new PlanFinding(
                            Code,
                            Severity,
                            string.Format(ExplainRulePromblemDescriptions.SeqScanFractionRemoved, tableName,
                                removedFraction.ToString("F1")),
                            ExplainRuleRecommendations.SeqScanFractionRemoved
                        ));
                    }
                }
            }

            if (node.Buffers != null && (node.Buffers.SharedRead > 0 || node.Buffers.TempRead > 0))
            {
                return Task.FromResult<PlanFinding?>(new PlanFinding(
                    Code,
                    Severity,
                    string.Format(ExplainRulePromblemDescriptions.SeqScanIOHeave, tableName),
                    ExplainRuleRecommendations.SeqScanIOHeavy
                ));
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