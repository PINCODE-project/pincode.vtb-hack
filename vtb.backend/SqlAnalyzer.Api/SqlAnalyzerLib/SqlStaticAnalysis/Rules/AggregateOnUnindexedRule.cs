using System.Text.RegularExpressions;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование агрегатных функций (MIN, MAX, COUNT) без индекса,
/// что приводит к полному сканированию таблицы.
/// </summary>
public sealed class AggregateOnUnindexedRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.AggregateOnUnindexed;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Index;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"(MIN|MAX|COUNT)\s*\(", RegexOptions.IgnoreCase) &&
            !Regex.IsMatch(query.Text, @"USING\s+INDEX", RegexOptions.IgnoreCase))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "MIN/MAX/COUNT могут работать быстрее с индексом, но индекс не найден.",
                Category,
                DefaultSeverity,
                new List<string> { "Aggregate" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
