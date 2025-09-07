using SqlAnalyzerLib.ExplainAnalysis.Interfaces;
using SqlAnalyzerLib.ExplainAnalysis.Models;

namespace SqlAnalyzerLib.ExplainAnalysis;

/// <summary>
/// Оркестратор анализа EXPLAIN JSON: парсит JSON, применяет правила через IRuleEngine и возвращает ExplainAnalysisResult.
/// </summary>
public sealed class ExplainAnalyzer : IExplainAnalyzer
{
    private readonly IExplainParser _parser;
    private readonly IRuleEngine _ruleEngine;

    /// <summary>
    /// Создаёт экземпляр ExplainAnalyzer.
    /// </summary>
    /// <param name="parser">Парсер EXPLAIN JSON.</param>
    /// <param name="ruleEngine">Движок правил.</param>
    public ExplainAnalyzer(IExplainParser parser, IRuleEngine ruleEngine)
    {
        _parser = parser;
        _ruleEngine = ruleEngine;
    }

    /// <inheritdoc />
    public async Task<ExplainAnalysisResult> AnalyzeAsync(string queryText, string explainJson, CancellationToken ct = default)
    {
        var rootPlan = _parser.Parse(explainJson);
        var findings = await _ruleEngine.EvaluateAllAsync(rootPlan).ConfigureAwait(false);
        return new ExplainAnalysisResult(
            QueryHash: HashQuery(queryText),
            RootPlan: rootPlan,
            Findings: findings.ToList(),
            AnalyzedAt: DateTime.UtcNow
        );
    }

    private static string HashQuery(string sql)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(sql.Trim().ToLowerInvariant());
        return Convert.ToHexString(sha.ComputeHash(bytes));
    }
}
