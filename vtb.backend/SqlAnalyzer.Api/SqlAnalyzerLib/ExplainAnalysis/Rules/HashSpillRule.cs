using System.Globalization;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
    /// P20: Hash / Hash Join spill на диск — высокие Batches или Disk usage в NodeSpecific.
    /// </summary>
    public sealed class HashSpillRule : IPlanRule
    {
        /// <inheritdoc />
        public string Code => "P20";

        /// <inheritdoc />
        public string Category => "Hash";

        /// <inheritdoc />
        public Severity DefaultSeverity => Severity.High;

        /// <inheritdoc />
        public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
        {
            if (node.NodeType == null) return Task.FromResult<PlanFinding?>(null);
            if (!node.NodeType.Contains("Hash", StringComparison.OrdinalIgnoreCase) && !node.NodeType.Contains("Hash Join", StringComparison.OrdinalIgnoreCase)) return Task.FromResult<PlanFinding?>(null);

            if (node.NodeSpecific != null)
            {
                if (node.NodeSpecific.TryGetValue("Batches", out var batchesObj) && TryGetLong(batchesObj, out var batches) && batches > 1)
                {
                    var metadata = new Dictionary<string, object> { ["Batches"] = batches };
                    var msg = "Hash operator использует несколько батчей (Batches > 1), вероятен spill на диск. Рассмотрите увеличение work_mem или уменьшение размера build-side.";
                    return Task.FromResult<PlanFinding?>(new PlanFinding(Code, msg, Category, DefaultSeverity, new List<string>(), metadata));
                }

                if (node.NodeSpecific.TryGetValue("Disk Usage", out var diskObj) && TryGetLong(diskObj, out var disk) && disk > 0)
                {
                    var metadata = new Dictionary<string, object> { ["DiskUsage"] = disk };
                    var msg = "Hash operator использует диск для хранения промежуточных данных. Увеличение work_mem может помочь, либо изменение плана на Nested Loop/ Merge Join.";
                    return Task.FromResult<PlanFinding?>(new PlanFinding(Code, msg, Category, DefaultSeverity, new List<string>(), metadata));
                }
            }

            if (node.Buffers != null && node.Buffers.TempWritten > 0)
            {
                var metadata = new Dictionary<string, object> { ["TempWritten"] = node.Buffers.TempWritten };
                var msg = "Hash operator/Hash Join записывает временные файлы на диск (TempWritten). Это индикатор spill'а.";
                return Task.FromResult<PlanFinding?>(new PlanFinding(Code, msg, Category, DefaultSeverity, new List<string>(), metadata));
            }

            return Task.FromResult<PlanFinding?>(null);

            static bool TryGetLong(object? o, out long val)
            {
                val = 0;
                if (o == null) return false;
                if (o is long l) { val = l; return true; }
                if (o is int i) { val = i; return true; }
                if (long.TryParse(o.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)) { val = parsed; return true; }
                return false;
            }
        }
    }