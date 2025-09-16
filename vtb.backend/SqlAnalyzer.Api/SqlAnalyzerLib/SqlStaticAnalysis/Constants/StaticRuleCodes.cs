namespace SqlAnalyzerLib.SqlStaticAnalysis.Constants;

/// <summary>
/// Коды статических правил анализа SQL.
/// </summary>
public enum StaticRuleCodes
{
    // Базовые (твои)
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
    OrderByWithoutLimit,

    // Новые
    ImplicitCrossJoin,
    UnnecessaryDistinct,
    GroupByWithoutHavingOrAggregate,
    RedundantOrderByInSubquery,
    NonIndexedJoin,
    UnnecessaryCast,
    UnionInsteadOfUnionAll,
    ExistsVsIn,
    GroupByWithoutHaving,
    OrderByRandom,
    DistinctWithGroupBy,
    LimitWithoutOrderBy,
    LeftJoinFollowedByWhere,
    CountStarWithJoin,
    MultipleOrConditions,
    SubqueryInSelect,
    FunctionOnIndexColumn,
    HavingWithoutGroupBy,
    BetweenWithNulls,
    InefficientLikePattern,
    CrossJoinWithoutCondition,
    InefficientDistinct,
    WhereTrueOr1Equals1,
    NullEqualsComparison,
    RedundantJoin,
    NestedSelectStar,
    JoinOnInequality,
    FunctionInJoinCondition,
    OrInWhereWithoutIndex,
    OverlyComplexCte,
    ImplicitCastInJoinOrWhere,
    CaseInWhere,
    AggregateOnUnindexed,
    SelectWithoutFrom
}
