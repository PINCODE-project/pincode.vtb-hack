namespace SqlAnalyzerLib.ExplainAnalysis.Models;

/// <summary>
/// Корневой план EXPLAIN, содержит метрики времени и общий узел плана.
/// </summary>
public record ExplainRootPlan
{
    /// <summary>
    /// Команда: SELECT/UPDATE/INSERT/DELETE и т.д., если доступно.
    /// </summary>
    public string? CommandType { get; init; }

    /// <summary>
    /// Дерево корневого узла плана.
    /// </summary>
    public PlanNode RootNode { get; init; } = null!;

    /// <summary>
    /// Время планирования в миллисекундах (если предоставлено).
    /// </summary>
    public double? PlanningTimeMs { get; init; }

    /// <summary>
    /// Время выполнения в миллисекундах (если предоставлено).
    /// </summary>
    public double? ExecutionTimeMs { get; init; }

    /// <summary>
    /// Дополнительные настройки/параметры из EXPLAIN.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Settings { get; init; }
}