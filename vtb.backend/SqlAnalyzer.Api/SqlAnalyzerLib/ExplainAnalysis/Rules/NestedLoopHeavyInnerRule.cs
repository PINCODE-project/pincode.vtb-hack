using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Rules;

/// <summary>
/// P22: Nested Loop с дорогой внутренней стороной — внутренний узел выполняется много раз при высоком ActualLoops и высокой стоимости.
/// </summary>
public sealed class NestedLoopHeavyInnerRule : IPlanRule
{
    /// <inheritdoc />
    public ExplainIssueRule Code => ExplainIssueRule.NestedLoopHeavyInner;

    /// <inheritdoc />
    public string Category => "Join";

    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan)
    {
        if (node.ShortNodeType == null) return Task.FromResult<PlanFinding?>(null);
        if (!node.ShortNodeType.Equals("NestedLoop", StringComparison.OrdinalIgnoreCase)) return Task.FromResult<PlanFinding?>(null);

        if (node.Children == null || node.Children.Count < 2) return Task.FromResult<PlanFinding?>(null);

        var outer = node.Children[0];
        var inner = node.Children[1];

        var loops = node.ActualLoops ?? 1;
        var innerTime = inner.ActualTotalTimeMs ?? 0;
        if (loops > 10 && innerTime > 5.0) // эвристика: много итераций и каждая итерация дорогая
        {
            var metadata = new Dictionary<string, object>
            {
                ["OuterActualRows"] = outer.ActualRows ?? 0,
                ["InnerActualRows"] = inner.ActualRows ?? 0,
                ["Loops"] = loops,
                ["InnerTimeMs"] = innerTime
            };
            var msg = "Nested Loop выполняется много раз с дорогой внутренней стороной. Рассмотрите индекс по join-ключу на внутренней стороне или замену на Hash/Merge join.";
            return Task.FromResult<PlanFinding?>(new PlanFinding(Code, msg, Category, DefaultSeverity, new List<string>(), metadata));
        }

        return Task.FromResult<PlanFinding?>(null);
    }
}