using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет наличие неявных преобразований типов в условиях JOIN или WHERE,
/// что делает индексы неэффективными.
/// </summary>
public sealed class ImplicitCastInJoinOrWhereRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRules Code => StaticRules.ImplicitCastInJoinOrWhere;
    
    /// <inheritdoc />
    public Severity Severity => Severity.Critical;

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"JOIN.*ON.*::", RegexOptions.IgnoreCase) ||
            Regex.IsMatch(query.Text, @"WHERE.*::", RegexOptions.IgnoreCase))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.ImplicitCastInJoinOrWhereProblemDescription,
                StaticRuleRecommendations.ImplicitCastInJoinOrWhereRecommendation
            ));
        }

        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}
