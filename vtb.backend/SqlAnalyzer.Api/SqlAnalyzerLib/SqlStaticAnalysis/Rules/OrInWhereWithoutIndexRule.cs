using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
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
    public StaticRules Code => StaticRules.OrInWhereWithoutIndex;

    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        var orCount = Regex.Matches(query.Text, @"WHERE.*\bOR\b", RegexOptions.IgnoreCase).Count;

        if (orCount > 0)
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.OrInWhereWithoutIndexProblemDescription,
                StaticRuleRecommendations.OrInWhereWithoutIndexRecommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
