using SqlAnalyzerLib.SqlStaticAnalysis.Constants;
using SqlAnalyzerLib.SqlStaticAnalysis.Models;

namespace SqlAnalyzerLib.SqlStaticAnalysis.Interfaces;

/// <summary>
/// Контракт для правила статического анализа.
/// </summary>
public interface IStaticRule
{
    /// <summary>
    /// Код правила (см. StaticRuleCodes).
    /// </summary>
    StaticRuleCodes Code { get; }

    /// <summary>
    /// Категория рекомендации, к которой относится правило.
    /// </summary>
    RecommendationCategory Category { get; }

    /// <summary>
    /// Уровень серьёзности по умолчанию.
    /// </summary>
    Severity DefaultSeverity { get; }

    /// <summary>
    /// Оценить правило по тексту запроса.
    /// </summary>
    /// <param name="query">SQL-запрос.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Нахождение (или null, если правило не сработало).</returns>
    Task<StaticCheckFinding?> EvaluateAsync(SqlQuery query, CancellationToken ct = default);
}