using SqlAnalyzer.Api.Dal.Constants;
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
    StaticRules Code { get; }

    /// <summary>
    /// Уровень серьёзности по умолчанию.
    /// </summary>
    Severity Severity { get; }

    /// <summary>
    /// Оценить правило по тексту запроса.
    /// </summary>
    /// <param name="query">SQL-запрос.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Нахождение (или null, если правило не сработало).</returns>
    Task<StaticAnalysisPoint?> EvaluateAsync(SqlQuery query, CancellationToken ct = default);
}