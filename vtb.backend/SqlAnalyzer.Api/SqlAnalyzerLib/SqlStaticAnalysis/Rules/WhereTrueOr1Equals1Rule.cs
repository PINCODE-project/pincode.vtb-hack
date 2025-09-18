using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет наличие условий вида WHERE TRUE или WHERE 1=1,
/// которые избыточны и могут скрывать потенциальные ошибки в логике.
/// </summary>
public sealed class WhereTrueOr1Equals1Rule : IStaticRule
{
    /// <inheritdoc />
    public StaticRules Code => StaticRules.WhereTrueOr1Equals1;

    /// <inheritdoc />
    public Severity Severity => Severity.Info;

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"WHERE\s+(TRUE|1\s*=\s*1)", RegexOptions.IgnoreCase))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.WhereTrueOr1Equals1ProblemDescription,
                StaticRuleRecommendations.WhereTrueOr1Equals1Recommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
