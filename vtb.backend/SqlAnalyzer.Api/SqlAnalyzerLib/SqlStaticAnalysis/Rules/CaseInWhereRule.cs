using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование CASE в WHERE, что делает условия несаргируемыми
/// и приводит к полному сканированию.
/// </summary>
public sealed class CaseInWhereRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRules Code => StaticRules.CaseInWhere;
    
    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"WHERE.*CASE", RegexOptions.IgnoreCase))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.CaseInWhereProblemDescription,
                StaticRuleRecommendations.CaseInWhereRecommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
