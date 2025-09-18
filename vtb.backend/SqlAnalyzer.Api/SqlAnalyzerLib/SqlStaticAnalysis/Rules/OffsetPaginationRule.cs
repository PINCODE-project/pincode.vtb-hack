using System.Text.RegularExpressions;
using SqlAnalyzer.Api.Dal.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Rules;

/// <summary>
/// Проверка OFFSET (пагинация). Предупреждает об эффективности при больших смещениях.
/// </summary>
public sealed class OffsetPaginationRule : IStaticRule
{
    /// <inheritdoc />
    public StaticRules Code => StaticRules.OffsetPagination;
    
    /// <inheritdoc />
    public Severity Severity => Severity.Warning;

    private static readonly Regex Pattern = new(@"\bOFFSET\s+\d+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default)
    {
        if (Pattern.IsMatch(query.Text))
        {
            return Task.FromResult<StaticAnalysisPoint?>(new StaticAnalysisPoint(
                Code,
                Severity,
                StaticRuleProblemsDescriptions.OffsetPaginationProblemDescription,
                StaticRuleRecommendations.OffsetPaginationRecommendation
            ));
            
        }
        return Task.FromResult<StaticAnalysisPoint?>(null);
    }
}