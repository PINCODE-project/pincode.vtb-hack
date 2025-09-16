namespace SqlAnalyzerLib.Recommendation.Enums;

/// <summary>
/// Категории рекомендаций по оптимизации SQL-запросов
/// </summary>
public enum RecommendationCategory
{
    Indexing,
    Joins,
    Aggregations,
    Subqueries,
    Sorting,
    Filtering,
    Cardinality,
    Statistics,
    ExecutionPlan,
    General,
    Parallelism,
    Memory
}