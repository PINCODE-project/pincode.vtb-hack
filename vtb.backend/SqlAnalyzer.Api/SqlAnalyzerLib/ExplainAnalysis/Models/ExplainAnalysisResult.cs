namespace SqlAnalyzerLib.ExplainAnalysis.Models;

/// <summary>
/// Описание результата разбора EXPLAIN (FORMAT JSON).
/// </summary>
public record ExplainAnalysisResult(
    string QueryHash,
    ExplainRootPlan RootPlan,
    IReadOnlyList<PlanFinding> Findings,
    DateTime AnalyzedAt);