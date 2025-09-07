using SqlAnalyzerLib.ExplainAnalysis.Models;

namespace SqlAnalyzerLib.ExplainAnalysis.Interfaces;

/// <summary>
/// Контракт JSON-парсера EXPLAIN (FORMAT JSON).
/// </summary>
public interface IExplainParser
{
    /// <summary>
    /// Парсит JSON-строку, полученную из EXPLAIN (FORMAT JSON), и возвращает ExplainRootPlan.
    /// </summary>
    /// <param name="explainJson">Текст JSON, как вернул PostgreSQL.</param>
    /// <returns>Структурное представление плана.</returns>
    ExplainRootPlan Parse(string explainJson);
}