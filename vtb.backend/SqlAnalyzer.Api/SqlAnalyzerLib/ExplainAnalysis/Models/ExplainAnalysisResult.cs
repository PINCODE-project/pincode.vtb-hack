namespace SqlAnalyzerLib.ExplainAnalysis.Models;

/// <summary>
/// Описание результата разбора EXPLAIN (FORMAT JSON).
/// </summary>
public record ExplainAnalysisResult(
    IReadOnlyList<PlanFinding> Findings,
    DateTime AnalyzedAt);