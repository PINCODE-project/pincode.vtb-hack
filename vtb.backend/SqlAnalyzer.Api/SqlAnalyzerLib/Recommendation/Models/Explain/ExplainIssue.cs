using SqlAnalyzerLib.Common;
using SqlAnalyzerLib.ExplainAnalysis.Enums;

namespace SqlAnalyzerLib.Recommendation.Models.Explain;

/// <summary>
/// Найденная проблема в плане выполнения
/// </summary>
public class ExplainIssue
{
    public ExplainIssueRule Rule { get; set; }

    public AnalysisSeverity Severity { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Recommendation { get; set; } = string.Empty;
}