using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование ORDER BY RANDOM(), которое крайне неэффективно
/// на больших таблицах и может привести к полному сканированию.
/// </summary>
public sealed class OrderByRandomRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.OrderByRandom;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Performance;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (query.Text.Contains("ORDER BY RANDOM()", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "Используется ORDER BY RANDOM() — крайне неэффективная операция, замените на выборку через TABLESAMPLE или индекс.",
                Category,
                DefaultSeverity,
                new List<string> { "ORDER BY RANDOM()" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
