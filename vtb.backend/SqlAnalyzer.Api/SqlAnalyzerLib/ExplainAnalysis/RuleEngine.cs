using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;

namespace SqlAnalyzerLib.ExplainAnalysis;

/// <summary>
/// Простая реализация IRuleEngine: рекурсивно обходит дерево и применяет все правила к каждому узлу.
/// </summary>
public sealed class RuleEngine : IRuleEngine
{
    private readonly IReadOnlyList<IPlanRule> _rules;

    /// <summary>
    /// Создаёт экземпляр движка правил.
    /// </summary>
    /// <param name="rules">Набор правил для применения.</param>
    public RuleEngine(IEnumerable<IPlanRule> rules)
    {
        _rules = rules?.ToList() ?? throw new ArgumentNullException(nameof(rules));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlanFinding>> EvaluateAllAsync(ExplainRootPlan rootPlan)
    {
        var findings = new List<PlanFinding>();
        await Traverse(rootPlan.RootNode, rootPlan, findings).ConfigureAwait(false);
        return findings;
    }

    private async Task Traverse(PlanNode node, ExplainRootPlan rootPlan, List<PlanFinding> findings)
    {
        foreach (var rule in _rules)
        {
            try
            {
                var res = await rule.EvaluateAsync(node, rootPlan).ConfigureAwait(false);
                if (res != null) findings.Add(res);
            }
            catch
            {
                // intentionally swallow rule exceptions to keep analysis robust
            }
        }

        if (node.Children != null)
        {
            foreach (var child in node.Children)
            {
                await Traverse(child, rootPlan, findings).ConfigureAwait(false);
            }
        }
    }
}