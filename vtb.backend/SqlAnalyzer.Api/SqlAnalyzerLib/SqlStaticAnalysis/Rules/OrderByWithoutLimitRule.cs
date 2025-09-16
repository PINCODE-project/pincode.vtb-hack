using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <inheritdoc />
public sealed class OrderByWithoutLimitRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.OrderByWithoutLimit;

    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Performance;

    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var sql = query.Text.ToUpperInvariant();

        if (sql.Contains("ORDER BY") && !sql.Contains("LIMIT"))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "Используется ORDER BY без LIMIT — сортировка может выполняться по всей таблице.",
                Category,
                DefaultSeverity,
                Array.Empty<string>()
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
