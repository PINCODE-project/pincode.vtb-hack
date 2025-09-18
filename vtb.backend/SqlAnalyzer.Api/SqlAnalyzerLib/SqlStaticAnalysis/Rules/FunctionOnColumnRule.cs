using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
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
    public StaticRules Code => StaticRules.FunctionOnColumn;
    

    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    private static readonly Regex Pattern = new(@"\b(?:LOWER|UPPER|DATE_TRUNC|CAST|EXTRACT|TO_CHAR|TO_TIMESTAMP|COALESCE)\s*\(", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Pattern.IsMatch(query.Text))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.FunctionOnColumnProblemDescription,
                StaticRuleRecommendations.FunctionOnColumnRecommendation
            ));
        }
        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}