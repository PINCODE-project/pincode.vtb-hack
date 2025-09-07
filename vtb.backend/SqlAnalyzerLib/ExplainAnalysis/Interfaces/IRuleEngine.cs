using SqlAnalyzerLib.ExplainAnalysis.Models;

namespace SqlAnalyzerLib.ExplainAnalysis.Interfaces;

/// <summary>
/// Регистратор/движок правил: хранит правила и умеет применить их ко всем узлам плана.
/// </summary>
public interface IRuleEngine
{
    /// <summary>
    /// Применяет все правила к дереву плана и возвращает коллекцию найденных проблем.
    /// </summary>
    /// <param name="rootPlan">Корневой план.</param>
    /// <returns>Список PlanFinding.</returns>
    Task<IReadOnlyList<PlanFinding>> EvaluateAllAsync(ExplainRootPlan rootPlan);
}