using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет использование агрегатных функций (MIN, MAX, COUNT) без индекса,
/// что приводит к полному сканированию таблицы.
/// </summary>
public sealed class AggregateOnUnindexedRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRules Code => StaticRules.AggregateOnUnindexed;

    /// <inheritdoc />
    public Severity Severity => Severity.Info;

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"(MIN|MAX|COUNT)\s*\(", RegexOptions.IgnoreCase) &&
            !Regex.IsMatch(query.Text, @"USING\s+INDEX", RegexOptions.IgnoreCase))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.AggregateOnUnindexedProblemDescription,
                StaticRuleRecommendations.AggregateOnUnindexedRecommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
