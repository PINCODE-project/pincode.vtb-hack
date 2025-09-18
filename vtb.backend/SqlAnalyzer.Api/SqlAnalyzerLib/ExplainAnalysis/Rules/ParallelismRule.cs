using System.Globalization;
using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.ExplainAnalysis.Entensions;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// P40/P41: Параллелизм — запланировано >0 workers, но launched == 0 или дисбаланс между workers.
/// </summary>
public sealed class ParallelismRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainRules Code => ExplainRules.Parallelism;

    /// <inheritdoc />
    public Severity Severity => Severity.Info;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node.NodeSpecific == null) return Task.FromResult<PlanFinding?>(null);
        if (!node.NodeType.Contains("Gather", StringComparison.OrdinalIgnoreCase) &&
            !node.NodeType.Contains("Gather Merge", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<PlanFinding?>(null);

        var workersPlanned = node.TryGetNodeSpecificString("Workers Planned");
        var workersLaunched = node.TryGetNodeSpecificString("Workers Launched");
        if (workersPlanned is not null &&
            workersLaunched is not null &&
            TryGetInt(workersPlanned, out var wp) &&
            TryGetInt(workersLaunched, out var wl) &&
            wp > 0 && wl == 0)
        {
            return Task.FromResult<PlanFinding?>(new PlanFinding(
                Code,
                Severity,
                ExplainRulePromblemDescriptions.Parallelism,
                ExplainRuleRecommendations.Parallelism
            ));
        }

        return Task.FromResult<PlanFinding?>(null);

        static bool TryGetInt(object? o, out int val)
        {
            val = 0;
            if (o == null) return false;
            if (o is int i)
            {
                val = i;
                return true;
            }

            if (o is long l)
            {
                val = (int)l;
                return true;
            }

            if (int.TryParse(o.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            {
                val = parsed;
                return true;
            }

            return false;
        }
    }
}