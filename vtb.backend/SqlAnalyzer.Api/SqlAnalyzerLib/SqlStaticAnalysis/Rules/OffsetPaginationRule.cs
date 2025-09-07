using System.Text.RegularExpressions;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверка OFFSET (пагинация). Предупреждает об эффективности при больших смещениях.
/// </summary>
public sealed class OffsetPaginationRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.OffsetPagination;

    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Rewrite;

    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    private static readonly Regex Pattern = new(@"\bOFFSET\s+\d+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Pattern.IsMatch(query.Text))
        {
            var msg = "OFFSET в пагинации приводит к пропуску строк и росту затрат при больших смещениях. Рассмотрите keyset-pagination (seek-pagination) с WHERE по последнему ключу.";
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(Code, msg, Category, DefaultSeverity, new List<string>()));
        }
        return Task.FromResult<StaticCheckFinding?>(null);
    }
}