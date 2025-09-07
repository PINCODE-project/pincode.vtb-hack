namespace SqlAnalyzerLib.Recommendation.Models.Explain;

/// <summary>
/// Результат анализа плана выполнения SQL-запроса
/// </summary>
public class ExplainAnalysisResult
{
    public string Sql { get; set; } = string.Empty;

    public List<ExplainIssue> Issues { get; set; } = new();
}