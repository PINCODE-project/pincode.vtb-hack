using SqlAnalyzerLib.Common;
using SqlAnalyzerLib.ExplainAnalysis.Enums;
using PlanFinding = SqlAnalyzerLib.ExplainAnalysis.Models.PlanFinding;
using ExplainAnalysisResult = SqlAnalyzerLib.Recommendation.Models.Explain.ExplainAnalysisResult;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.Recommendation.Models.Explain;

namespace SqlAnalyzerLib.Facade.Mappers;

public static class ExplainAnalysisMapper
{
    public static ExplainAnalysisResult ToExplainAnalysisResult(
        this IReadOnlyList<PlanFinding> findings,
        string sql)
    {
        return new ExplainAnalysisResult
        {
            Sql = sql,
            Issues = findings.Select(f => new ExplainIssue
            {
                Rule = f.Code,
                Severity = MapSeverity(f.Severity),
                Description = f.Message,
                Recommendation = $"Category: {f.Category}, Objects: {string.Join(", ", f.AffectedObjects)}"
            }).ToList()
        };
    }
    
    private static AnalysisSeverity MapSeverity(Severity s) => s switch
    {
        Severity.Low => AnalysisSeverity.Info,
        Severity.Medium => AnalysisSeverity.Warning,
        Severity.High => AnalysisSeverity.Critical,
        _ => AnalysisSeverity.Warning
    };
}