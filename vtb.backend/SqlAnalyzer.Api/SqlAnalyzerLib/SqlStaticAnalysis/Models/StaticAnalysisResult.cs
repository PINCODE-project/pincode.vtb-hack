namespace SqlAnalyzerLib.SqlStaticAnalysis.Models;

/// <summary>
/// Итоговый результат статического анализа.
/// </summary>
public record StaticAnalysisResult(
    string Query,
    ICollection<StaticAnalysisPoint> Findings,
    DateTime AnalyzedAt);