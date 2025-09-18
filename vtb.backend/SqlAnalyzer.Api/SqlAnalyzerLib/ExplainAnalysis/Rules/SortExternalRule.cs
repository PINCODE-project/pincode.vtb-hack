using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
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
        public ExplainRules Code => ExplainRules.SortExternal;

        public Severity Severity => Severity.Critical;

        public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
        {
            if (string.IsNullOrEmpty(node.NodeType)) return Task.FromResult<PlanFinding?>(null);
            if (!node.NodeType.Contains("Sort", StringComparison.OrdinalIgnoreCase) &&
                !(node.ShortNodeType?.Contains("Sort", StringComparison.OrdinalIgnoreCase) ?? false))
                return Task.FromResult<PlanFinding?>(null);

            if (node.NodeSpecific != null)
            {
                var sortMethod = node.TryGetNodeSpecificString("Sort Method");
                var sortSpaceType = node.TryGetNodeSpecificString("Sort Space Type");

                if (!string.IsNullOrEmpty(sortMethod) &&
                    sortMethod.IndexOf("external", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return Task.FromResult<PlanFinding?>(new PlanFinding(
                        Code,
                        Severity,
                        ExplainRulePromblemDescriptions.SortExternal,
                        ExplainRuleRecommendations.SortExternal
                    ));
                }

                if (!string.IsNullOrEmpty(sortSpaceType) &&
                    sortSpaceType.IndexOf("disk", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return Task.FromResult<PlanFinding?>(new PlanFinding(
                        Code,
                        Severity,
                        ExplainRulePromblemDescriptions.SortExternalTempFile,
                        ExplainRuleRecommendations.SortExternalTempFile
                    ));
                }
            }

            if (node.Buffers != null && node.Buffers.TempWritten > 0)
            {
                return Task.FromResult<PlanFinding?>(new PlanFinding(
                    Code,
                    Severity,
                    ExplainRulePromblemDescriptions.SortExternalTempWritten,
                    ExplainRuleRecommendations.SortExternalTempWritten
                ));
            }

            return Task.FromResult<PlanFinding?>(null);
        }
    }
}