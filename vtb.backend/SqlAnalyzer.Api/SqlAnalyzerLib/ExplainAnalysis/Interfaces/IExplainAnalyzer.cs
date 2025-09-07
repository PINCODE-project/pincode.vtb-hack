using SqlAnalyzerLib.ExplainAnalysis.Models;

namespace SqlAnalyzerLib.ExplainAnalysis.Interfaces;

/// <summary>
/// Интерфейс анализатора EXPLAIN плана — оркестратор применения правил и сборщик находок.
/// </summary>
public interface IExplainAnalyzer
{
    /// <summary>
    /// Анализирует JSON плана и возвращает структурированный результат с набором находок.
    /// </summary>
    /// <param name="queryText">Исходный SQL (используется для hash и контекста).</param>
    /// <param name="explainJson">JSON от EXPLAIN (FORMAT JSON).</param>
    /// <param name="ct">Токен отмены.</param>
    Task<ExplainAnalysisResult> AnalyzeAsync(string queryText, string explainJson, CancellationToken ct = default);
}