using System.Text.RegularExpressions;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет наличие подзапросов в SELECT, что может приводить к выполнению подзапроса
/// для каждой строки и значительно снижать производительность.
/// </summary>
public sealed class SubqueryInSelectRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.SubqueryInSelect;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Performance;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"SELECT\s+\(SELECT", RegexOptions.IgnoreCase))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "Найден подзапрос внутри SELECT — это может привести к N+1 выполнению.",
                Category,
                DefaultSeverity,
                new List<string> { "Subquery in SELECT" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
