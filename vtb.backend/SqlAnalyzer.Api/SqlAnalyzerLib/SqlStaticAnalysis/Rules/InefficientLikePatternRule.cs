using System.Text.RegularExpressions;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование шаблонов LIKE, которые не могут использовать индекс (например, %abc%).
/// </summary>
public sealed class InefficientLikePatternRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.InefficientLikePattern;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Performance;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"LIKE\s+'%.*%'", RegexOptions.IgnoreCase))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "LIKE с ведущим и замыкающим % не использует индекс — рассмотрите trigram индекс или полнотекстовый поиск.",
                Category,
                DefaultSeverity,
                new List<string> { "LIKE" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
