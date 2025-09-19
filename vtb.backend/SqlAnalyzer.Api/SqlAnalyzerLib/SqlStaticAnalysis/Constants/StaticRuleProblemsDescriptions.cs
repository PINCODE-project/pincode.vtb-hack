namespace SqlAnalyzerLib.SqlStaticAnalysis.Constants;

/// <summary>
/// Описание всех проблем (человечества) SQL-запросов
/// </summary>
public static class StaticRuleProblemsDescriptions
{
    public const string FunctionOnColumnProblemDescription = "Функция применяется к колонке в выражении (WHERE/ON/Join), что делает индекс менее эффективным.";
    
    public const string TypeMismatchUuidComparisonProblemDescription = "Найдено сравнение с UUID-форматной строкой без явного ::uuid.";
    public const string TypeMismatchDateComparisonProblemDescription = "Найдено сравнение с датой/временем в виде строки без явного типа.";
    public const string TypeMismatchNumericComparisonProblemDescription = "Найдено сравнение колонки с численной строкой.";
    
    public const string LeadingWildcardLikeProblemDescription = "LIKE/ILIKE начинается с '%', обычный btree-индекс не поможет.";
    public const string SelectStarProblemDescription = "Использование SELECT * приводит к передаче лишних данных и неопределённости.";
    public const string NotInNullsProblemDescription = "Используется NOT IN, который некорректно работает при наличии NULL в подзапросе.";
    public const string OffsetPaginationProblemDescription = "OFFSET в пагинации приводит к пропуску строк и росту затрат при больших смещениях.";
    public const string CartesianJoinProblemDescription = "В FROM используется перечисление таблиц через запятую без JOIN/ON — возможно Cartesian product.";
    public const string MissingWhereDeleteProblemDescription = "DELETE или UPDATE без WHERE затронет все строки таблицы.";
    public const string NonSargableExpressionProblemDescription = "В выражении используются арифметические операции над колонками (например, col + 1), что делает предикат non-sargable.";
    public const string SubqueryInInsteadOfJoinProblemDescription = "Найдено IN (SELECT ...). В некоторых случаях использование JOIN/LEFT JOIN экономичнее по производительности и понятнее по плану выполнения.";
    public const string SubqueryExistsInsteadOfJoinProblemDescription = "Найдено EXISTS (SELECT ...). Это может быть оправдано, но если подзапрос не коррелирован, JOIN может быть эффективнее.";
    public const string OrderByWithoutLimitProblemDescription = "Используется ORDER BY без LIMIT — сортировка может выполняться по всей таблице.";
    public const string ImplicitCrossJoinProblemDescription = "Используется запятая в FROM — это неявный CROSS JOIN, лучше использовать явный JOIN.";
    public const string UnnecessaryDistinctProblemDescription = "DISTINCT может быть избыточным — в запросе нет JOIN или GROUP BY.";
    public const string GroupByWithoutHavingOrAggregateProblemDescription = "GROUP BY используется без агрегатных функций — вероятно, ошибка или лишнее.";
    public const string RedundantOrderByInSubqueryProblemDescription = "ORDER BY внутри подзапроса без LIMIT не имеет смысла и может замедлять выполнение.";
    public const string NonIndexedJoinProblemDescription = "JOIN выполняется не по ID — убедитесь, что в условии есть индексируемый столбец.";
    public const string UnnecessaryCastProblemDescription = "Найдены избыточные приведения типов (::text::text).";
    public const string UnionInsteadOfUnionAllProblemDescription = "Используется UNION без ALL — лишнее удаление дублей может замедлять запрос.";
    public const string ExistsVsInProblemDescription = "Используется IN с подзапросом.";
    public const string OrderByRandomProblemDescription = "Используется ORDER BY RANDOM() — крайне неэффективная операция.";
    public const string DistinctWithGroupByProblemDescription = "Используется DISTINCT вместе с GROUP BY.";
    public const string LimitWithoutOrderByProblemDescription = "Используется LIMIT без ORDER BY — результат выборки может быть непредсказуемым.";
    public const string LeftJoinFollowedByWhereProblemDescription = "LEFT JOIN используется вместе с условием в WHERE, что эквивалентно INNER JOIN.";
    public const string CountStarWithJoinProblemDescription = "COUNT(*) с JOIN может возвращать завышенное количество строк из-за дубликатов.";
    public const string MultipleOrConditionsProblemDescription = "Обнаружено более трёх условий OR — это может замедлить выполнение.";
    public const string SubqueryInSelectProblemDescription = "Найден подзапрос внутри SELECT — это может привести к N+1 выполнению.";
    public const string FunctionOnIndexColumnProblemDescription = "Используются функции на индексируемых колонках — индекс не будет применён.";
    public const string HavingWithoutGroupByProblemDescription = "Используется HAVING без GROUP BY — условие избыточно и может быть вынесено в WHERE.";
    public const string BetweenWithNullsProblemDescription = "Используется BETWEEN c возможными NULL.";
    public const string InefficientLikePatternProblemDescription = "LIKE с ведущим и замыкающим % не использует индекс.";
    public const string CrossJoinWithoutConditionProblemDescription = "CROSS JOIN без условия приводит к декартовому произведению.";
    public const string InefficientDistinctProblemDescription = "DISTINCT используется без JOIN или GROUP BY — возможно, он лишний.";
    public const string WhereTrueOr1Equals1ProblemDescription = "Условие WHERE TRUE или WHERE 1=1 не имеет смысла и может быть ошибкой.";
    public const string NullEqualsComparisonProblemDescription = "Сравнение с NULL через = или != некорректно.";
    public const string RedundantJoinProblemDescription = "JOIN на таблицу, не используемую в SELECT или WHERE, является избыточным.";
    public const string NestedSelectStarProblemDescription = "Используется SELECT * внутри подзапроса — укажите только необходимые колонки.";
    public const string JoinOnInequalityProblemDescription = "JOIN выполняется по условию неравенства, что может привести к полному сканированию.";
    public const string FunctionInJoinConditionProblemDescription = "В условии JOIN используются функции — индекс не будет применён.";
    public const string OrInWhereWithoutIndexProblemDescription = "Обнаружено использование OR в WHERE. При отсутствии составных индексов это приведёт к Seq Scan.";
    public const string OverlyComplexCteProblemDescription = "CTE содержит вложенные SELECT — возможно, он слишком сложен и требует упрощения.";
    public const string ImplicitCastInJoinOrWhereProblemDescription = "Обнаружено неявное приведение типов в JOIN или WHERE — индекс может не использоваться.";
    public const string CaseInWhereProblemDescription = "Используется CASE внутри WHERE — условие несаргируемое, индекс не применяется.";
    public const string AggregateOnUnindexedProblemDescription = "MIN/MAX/COUNT могут работать быстрее с индексом, но индекс не найден.";
    public const string SelectWithoutFromProblemDescription = "Обнаружен SELECT без FROM — вероятно, это артефакт отладки.";
}