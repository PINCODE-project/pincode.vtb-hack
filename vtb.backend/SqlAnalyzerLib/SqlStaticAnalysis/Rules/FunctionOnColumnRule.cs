using System.Text.RegularExpressions;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверка наличия функций на индексируемых колонках (например, LOWER(col)).
/// Ищет частые функции, которые делают выражение не sargable.
/// </summary>
public sealed class FunctionOnColumnRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.FunctionOnColumn;

    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Index;

    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    private static readonly Regex Pattern = new(@"\b(?:LOWER|UPPER|DATE_TRUNC|CAST|EXTRACT|TO_CHAR|TO_TIMESTAMP|COALESCE)\s*\(", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Pattern.IsMatch(query.Text))
        {
            var msg = "Функция применяется к колонке в выражении (WHERE/ON/Join), что делает индекс менее эффективным. Рассмотрите индекс по выражению или generated column.";
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(Code, msg, Category, DefaultSeverity, new List<string>()));
        }
        return Task.FromResult<StaticCheckFinding?>(null);
    }
}