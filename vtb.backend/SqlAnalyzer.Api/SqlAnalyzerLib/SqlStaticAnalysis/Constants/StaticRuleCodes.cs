namespace SqlAnalyzerLib.SqlStaticAnalysis.Constants;

/// <summary>
/// Коды статических правил анализа SQL.
/// </summary>
public enum StaticRuleCodes
{
    FunctionOnColumn,
    TypeMismatchComparison,
    LeadingWildcardLike,
    SelectStar,
    NotInNulls,
    OffsetPagination,
    CartesianJoin,
    MissingWhereDelete,
    NonSargableExpression,
    SubqueryInsteadOfJoin,
}