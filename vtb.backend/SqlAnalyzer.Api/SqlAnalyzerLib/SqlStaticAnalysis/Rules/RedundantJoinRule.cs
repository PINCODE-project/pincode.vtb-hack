using System.Text.RegularExpressions;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверяет наличие JOIN на таблицу, которая не используется в SELECT или WHERE,
/// что делает соединение избыточным.
/// </summary>
public sealed class RedundantJoinRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRuleCodes Code => StaticRuleCodes.RedundantJoin;
    /// <inheritdoc />
    public RecommendationCategory Category => RecommendationCategory.Performance;
    /// <inheritdoc />
    public Severity DefaultSeverity => Severity.Medium;

    /// <inheritdoc />
    public Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Regex.IsMatch(query.Text, @"JOIN\s+\w+", RegexOptions.IgnoreCase) &&
            !Regex.IsMatch(query.Text, @"\w+\.", RegexOptions.IgnoreCase))
        {
            return Task.FromResult<StaticCheckFinding?>(new StaticCheckFinding(
                Code,
                "JOIN на таблицу, не используемую в SELECT или WHERE, является избыточным.",
                Category,
                DefaultSeverity,
                new List<string> { "JOIN" }
            ));
        }

        return Task.FromResult<StaticCheckFinding?>(null);
    }
}
