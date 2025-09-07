using SqlAnalyzerLib.Common;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.Recommendation.Models.Query;

// <summary>
/// Найденная проблема в SQL-запросе
/// </summary>
public class QueryIssue
{
    public QueryIssueRule Rule { get; set; }

    public AnalysisSeverity Severity { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Recommendation { get; set; } = string.Empty;
}