using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;

namespace SqlAnalyzerLib.ExplainAnalysis;

/// <summary>
/// Оркестратор анализа EXPLAIN JSON: парсит JSON, применяет правила через IRuleEngine и возвращает ExplainAnalysisResult.
/// </summary>
public sealed class ExplainAnalyzer : IExplainAnalyzer
{
    private readonly IRuleEngine _ruleEngine;

    /// <summary>
    /// Создаёт экземпляр ExplainAnalyzer.
    /// </summary>
    /// <param name="parser">Парсер EXPLAIN JSON.</param>
    /// <param name="ruleEngine">Движок правил.</param>
    public ExplainAnalyzer(IRuleEngine ruleEngine)
    {
        _ruleEngine = ruleEngine;
    }

    /// <inheritdoc />
    public async Task<ExplainAnalysisResult> AnalyzeAsync(string queryText, ExplainRootPlan explainJson, CancellationToken ct = default)
    {
        var findings = await _ruleEngine.EvaluateAllAsync(explainJson).ConfigureAwait(false);
        return new ExplainAnalysisResult(
            Findings: findings.ToList(),
            AnalyzedAt: DateTime.UtcNow
        );
    }
}
