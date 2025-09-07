namespace SqlAnalyzerLib.SqlStaticAnalysis.Constants;

/// <summary>
/// Типы ошибок и антипаттернов, выявляемых статическим анализатором SQL-запросов
/// </summary>
public enum QueryIssueRule
{
    MissingWhereClause,
    SelectStarUsage,
    CartesianJoin,
    GroupByWithoutAggregation,
    OrderByWithoutIndex,
    LikeWithoutIndex,
    NotInUsage,
    DistinctWithoutNeed,
    FunctionOnIndexedColumn,
    ImplicitConversion,
    UnnecessarySubquery,
    UnusedCte
}