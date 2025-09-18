using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет наличие подзапросов в SELECT, что может приводить к выполнению подзапроса
/// для каждой строки и значительно снижать производительность.
/// </summary>
public sealed class SubqueryInSelectRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRules Code => StaticRules.SubqueryInSelect;

    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"SELECT\s+\(SELECT", RegexOptions.IgnoreCase))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.SubqueryInSelectProblemDescription,
                StaticRuleRecommendations.SubqueryInSelectRecommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
