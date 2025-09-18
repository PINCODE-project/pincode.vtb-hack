namespace SqlAnalyzerLib.SqlStaticAnalysis.Models;

/// <summary>
/// Итоговый результат статического анализа.
/// </summary>
public record StaticAnalysisResult(
    string Query,
    IReadOnlyList<StaticAnalysisPoint> Findings,
    DateTime AnalyzedAt);