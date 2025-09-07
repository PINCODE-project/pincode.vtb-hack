namespace SqlAnalyzerLib.SqlStaticAnalysis.Models;

/// <summary>
/// Итоговый результат статического анализа.
/// </summary>
public record StaticAnalysisResult(
    string QueryHash,
    SqlQuery Query,
    IReadOnlyList<StaticCheckFinding> Findings,
    DateTime AnalyzedAt);