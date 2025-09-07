using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Models;

/// <summary>
/// Результат одного статического правила.
/// </summary>
public record StaticCheckFinding(
    StaticRuleCodes Code,
    string Message,
    RecommendationCategory Category,
    Severity Severity,
    IReadOnlyList<string> AffectedColumns);