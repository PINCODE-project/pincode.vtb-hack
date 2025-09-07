using SqlAnalyzerLib.Common;
using SqlAnalyzerLib.Recommendation.Models.Query;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.Facade.Mappers;

public static class StaticAnalysisMapper
{
    public static QueryAnalysisResult ToQueryAnalysisResult(this StaticAnalysisResult result)
    {
        return new QueryAnalysisResult
        {
            Sql = result.Query.Text,
            Issues = result.Findings.Select(f => new QueryIssue
            {
                Rule = MapRule(f.Code),
                Severity = MapSeverity(f.Severity),
                Description = f.Message,
                Recommendation = $"Category: {f.Category}, Columns: {string.Join(", ", f.AffectedColumns)}"
            }).ToList()
        };
    }
    
    private static QueryIssueRule MapRule(StaticRuleCodes code) => code switch
    {
        StaticRuleCodes.SelectStar => QueryIssueRule.SelectStarUsage,
        StaticRuleCodes.CartesianJoin => QueryIssueRule.CartesianJoin,
        StaticRuleCodes.FunctionOnColumn => QueryIssueRule.FunctionOnIndexedColumn,
        StaticRuleCodes.TypeMismatchComparison => QueryIssueRule.ImplicitConversion,
        StaticRuleCodes.LeadingWildcardLike => QueryIssueRule.LikeWithoutIndex,
        StaticRuleCodes.NotInNulls => QueryIssueRule.NotInUsage,
        StaticRuleCodes.MissingWhereDelete => QueryIssueRule.MissingWhereClause,
        StaticRuleCodes.NonSargableExpression => QueryIssueRule.UnnecessarySubquery,
        StaticRuleCodes.SubqueryInsteadOfJoin => QueryIssueRule.UnnecessarySubquery,
        StaticRuleCodes.OffsetPagination => QueryIssueRule.UnnecessarySubquery, // или добавить новый enum
        _ => QueryIssueRule.UnnecessarySubquery
    };

    private static AnalysisSeverity MapSeverity(Severity s) => s switch
    {
        Severity.Low => AnalysisSeverity.Info,
        Severity.Medium => AnalysisSeverity.Warning,
        Severity.High => AnalysisSeverity.Critical,
        _ => AnalysisSeverity.Warning
    };
    
}