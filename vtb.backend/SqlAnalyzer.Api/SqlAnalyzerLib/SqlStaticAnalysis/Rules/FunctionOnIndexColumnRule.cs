using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование функций на индексируемых колонках, что делает индекс бесполезным.
/// </summary>
public sealed class FunctionOnIndexColumnRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRules Code => StaticRules.FunctionOnIndexColumn;
    
    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"WHERE\s+\w+\s*::", RegexOptions.IgnoreCase) ||
            Regex.IsMatch(query.Text, @"WHERE\s+\w+\s*\(", RegexOptions.IgnoreCase))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.FunctionOnIndexColumnProblemDescription,
                StaticRuleRecommendations.FunctionOnIndexColumnRecommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
