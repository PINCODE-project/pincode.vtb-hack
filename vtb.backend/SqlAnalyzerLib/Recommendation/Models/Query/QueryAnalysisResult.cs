using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.Recommendation.Models.Query;

/// <summary>
/// Результат статического анализа SQL-запроса
/// </summary>
public class QueryAnalysisResult
{
    public string Sql { get; set; } = string.Empty;

    public List<QueryIssue> Issues { get; set; } = new();
}