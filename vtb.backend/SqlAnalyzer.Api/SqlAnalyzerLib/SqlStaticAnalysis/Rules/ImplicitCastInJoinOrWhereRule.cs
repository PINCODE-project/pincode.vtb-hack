using System.Text.RegularExpressions;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет наличие неявных преобразований типов в условиях JOIN или WHERE,
/// что делает индексы неэффективными.
/// </summary>
public sealed class ImplicitCastInJoinOrWhereRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.ImplicitCastInJoinOrWhere;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Index;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.High;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"JOIN.*ON.*::", RegexOptions.IgnoreCase) ||
            Regex.IsMatch(query.Text, @"WHERE.*::", RegexOptions.IgnoreCase))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "Обнаружено неявное приведение типов в JOIN или WHERE — индекс может не использоваться.",
                Category,
                DefaultSeverity,
                new List<string> { "Implicit cast" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
