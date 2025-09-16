using SqlAnalyzerLib.ExplainAnalysis.Enums;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Models;

/// <summary>
/// Описание одного найденного "находки" (issue) по плану.
/// </summary>
public record PlanFinding(
    ExplainIssueRule Code,
    string Message,
    string Category,
    Severity Severity,
    IReadOnlyList<string> AffectedObjects,
    IReadOnlyDictionary<string, object>? Metadata);