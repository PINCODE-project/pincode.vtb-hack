using SqlAnalyzerLib.ExplainAnalysis.Models;
using SqlAnalyzerLib.SqlStaticAnalysis.Constants;

namespace SqlAnalyzerLib.ExplainAnalysis.Interfaces;

/// <summary>
/// Контракт правила, применимого к узлу плана.
/// </summary>
public interface IPlanRule
{
    /// <summary>
    /// Уникальный код правила (например, P10).
    /// </summary>
    string Code { get; }

    /// <summary>
    /// Категория (человеко-читаемая).
    /// </summary>
    string Category { get; }

    /// <summary>
    /// Уровень серьёзности по умолчанию.
    /// </summary>
    Severity DefaultSeverity { get; }

    /// <summary>
    /// Проверить правило на заданном узле и контексте.
    /// </summary>
    /// <param name="node">Узел плана.</param>
    /// <param name="rootPlan">Корневой план для контекста.</param>
    /// <returns>PlanFinding при срабатывании правила или null.</returns>
    Task<PlanFinding?> EvaluateAsync(PlanNode node, ExplainRootPlan rootPlan);
}