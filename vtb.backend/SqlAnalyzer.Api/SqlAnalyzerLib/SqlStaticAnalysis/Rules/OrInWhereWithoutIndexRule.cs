using System.Text.RegularExpressions;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет наличие нескольких условий OR в WHERE,
/// которые могут приводить к полному сканированию при отсутствии индексов.
/// </summary>
public sealed class OrInWhereWithoutIndexRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.OrInWhereWithoutIndex;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Index;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var orCount = Regex.Matches(query.Text, @"WHERE.*\bOR\b", RegexOptions.IgnoreCase).Count;

        if (orCount > 0)
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "Обнаружено использование OR в WHERE. При отсутствии составных индексов это приведёт к Seq Scan.",
                Category,
                DefaultSeverity,
                new List<string> { "OR in WHERE" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
