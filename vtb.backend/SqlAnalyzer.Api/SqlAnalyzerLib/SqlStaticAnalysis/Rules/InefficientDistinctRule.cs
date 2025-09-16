using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование DISTINCT без необходимости,
/// когда в запросе нет агрегатов и JOIN, что может зря нагружать планировщик.
/// </summary>
public sealed class InefficientDistinctRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.InefficientDistinct;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Performance;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Low;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("DISTINCT") && !sql.Contains("JOIN") && !sql.Contains("GROUP BY"))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "DISTINCT используется без JOIN или GROUP BY — возможно, он лишний.",
                Category,
                DefaultSeverity,
                new List<string> { "DISTINCT" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
