using System.Globalization;
using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
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
        public ExplainRules Code => ExplainRules.HashSpill;

        /// <inheritdoc />
        public Severity Severity => Severity.Critical;

        /// <inheritdoc />
        public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
        {
            if (!node.NodeType.Contains("Hash", StringComparison.OrdinalIgnoreCase) && !node.NodeType.Contains("Hash Join", StringComparison.OrdinalIgnoreCase)) return Task.FromResult<PlanFinding?>(null);

            if (node.NodeSpecific != null)
            {
                if (node.NodeSpecific.TryGetValue("Batches", out var batchesObj) && TryGetLong(batchesObj, out var batches) && batches > 1)
                {
                     return Task.FromResult<PlanFinding?>(new PlanFinding(
                        Code,
                        Severity,
                        ExplainRulePromblemDescriptions.HashSpillBatches,
                        ExplainRuleRecommendations.HashSpillBatches
                    ));}

                if (node.NodeSpecific.TryGetValue("Disk Usage", out var diskObj) && TryGetLong(diskObj, out var disk) && disk > 0)
                {
                    return Task.FromResult<PlanFinding?>(new PlanFinding(
                        Code,
                        Severity,
                        ExplainRulePromblemDescriptions.HashSpillDisk,
                        ExplainRuleRecommendations.HashSpillDisk
                    ));}
            }

            if (node.Buffers is { TempWritten: > 0 })
            {
                return Task.FromResult<PlanFinding?>(new PlanFinding(
                    Code,
                    Severity,
                    ExplainRulePromblemDescriptions.HashSpillTempFiles,
                    ExplainRuleRecommendations.HashSpillTempFiles
                ));
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