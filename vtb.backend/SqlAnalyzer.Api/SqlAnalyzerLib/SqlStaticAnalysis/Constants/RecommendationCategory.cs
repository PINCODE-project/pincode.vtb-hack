namespace SqlAnalyzerLib.SqlStaticAnalysis.Constants;

/// <summary>
/// Категории рекомендаций, применимые при анализе SQL-запросов.
/// </summary>
public enum RecommendationCategory
{
    Index,
    Rewrite,
    Statistics,
    Safety,
    DML,
    Partitioning,
    Performance,
    Correctness
}