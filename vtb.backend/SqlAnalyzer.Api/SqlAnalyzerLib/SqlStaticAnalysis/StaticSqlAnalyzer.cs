using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis;

/// <summary>
/// Базовая реализация статического анализатора SQL.
/// </summary>
public sealed class StaticSqlAnalyzer : IStaticSqlAnalyzer
{
    private readonly IEnumerable<IStaticRule> _rules;

    /// <summary>
    /// Создаёт экземпляр анализатора с набором правил.
    /// </summary>
    /// <param name="rules">Коллекция правил, применяемых при анализе.</param>
    public StaticSqlAnalyzer(IEnumerable<IStaticRule> rules)
    {
        _rules = rules;
    }

    /// <inheritdoc />
    public async Task<StaticAnalysisResult> AnalyzeAsync(SqlQuery query, CancellationToken ct = default)
    {
        var findings = new List<StaticAnalysisPoint>();

        foreach (var rule in _rules)
        {
            var finding = await rule.EvaluateAsync(query, ct).ConfigureAwait(false);
            if (finding != null)
            {
                findings.Add(finding);
            }
        }

        return new StaticAnalysisResult(query.Text, findings, DateTime.UtcNow);
    }
}