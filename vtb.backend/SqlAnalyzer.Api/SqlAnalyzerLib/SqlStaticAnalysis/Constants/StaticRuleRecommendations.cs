namespace SqlAnalyzerLib.SqlStaticAnalysis.Constants;

/// <summary>
/// Рекомендации к правилам
/// </summary>
public static class StaticRuleRecommendations
{
    public const string FunctionOnColumnRecommendation = "Перепишите условие так, чтобы функция применялась к константе или используйте функциональный индекс.";
    
    public const string TypeMismatchUuidComparisonRecommendation = "Явно приводите строку к UUID через ::uuid.";
    public const string TypeMismatchDateComparisonRecommendation = "Используйте литералы даты/времени или явное приведение типов.";
    public const string TypeMismatchNumericComparisonRecommendation = "Сравнивайте с числовым литералом, а не строкой.";
    
    public const string LeadingWildcardLikeRecommendation = "Для поиска по подстроке используйте trigram-индекс (pg_trgm) или полнотекстовый поиск.";
    public const string SelectStarRecommendation = "Указывайте конкретные колонки в SELECT.";
    public const string NotInNullsRecommendation = "Замените NOT IN на NOT EXISTS или используйте фильтр с IS NULL.";
    public const string OffsetPaginationRecommendation = "Используйте keyset-пагинацию (WHERE col > last_value).";
    public const string CartesianJoinRecommendation = "Задайте явное условие JOIN или перепишите запрос.";
    public const string MissingWhereDeleteRecommendation = "Добавьте WHERE или ограничьте DELETE/UPDATE подзапросом.";
    public const string NonSargableExpressionRecommendation = "Перепишите выражение так, чтобы операция выполнялась над константой, а не колонкой.";
    public const string SubqueryInInsteadOfJoinRecommendation = "Рассмотрите замену IN (SELECT) на JOIN/LEFT JOIN.";
    public const string SubqueryExistsInsteadOfJoinRecommendation = "Замените EXISTS на JOIN, если подзапрос не коррелирован.";
    public const string OrderByWithoutLimitRecommendation = "Добавьте LIMIT или уберите ORDER BY, если он не нужен.";
    public const string ImplicitCrossJoinRecommendation = "Используйте явный CROSS JOIN или INNER JOIN с условием.";
    public const string UnnecessaryDistinctRecommendation = "Удалите DISTINCT, если нет JOIN или агрегатов.";
    public const string GroupByWithoutHavingOrAggregateRecommendation = "Проверьте целесообразность GROUP BY без агрегатов.";
    public const string RedundantOrderByInSubqueryRecommendation = "Удалите ORDER BY из подзапроса без LIMIT.";
    public const string NonIndexedJoinRecommendation = "Убедитесь, что JOIN выполняется по индексируемым колонкам.";
    public const string UnnecessaryCastRecommendation = "Удалите избыточные приведения типов.";
    public const string UnionInsteadOfUnionAllRecommendation = "Используйте UNION ALL, если удаление дублей не требуется.";
    public const string ExistsVsInRecommendation = "Выберите EXISTS для коррелированных подзапросов, IN — для малых списков.";
    public const string OrderByRandomRecommendation = "Замените RANDOM() на выборку по упорядоченному ключу с LIMIT.";
    public const string DistinctWithGroupByRecommendation = "Уберите DISTINCT — GROUP BY уже убирает дубли.";
    public const string LimitWithoutOrderByRecommendation = "Добавьте ORDER BY для предсказуемого результата.";
    public const string LeftJoinFollowedByWhereRecommendation = "Перенесите условие в ON или используйте INNER JOIN.";
    public const string CountStarWithJoinRecommendation = "Используйте COUNT(DISTINCT ...) или перепишите JOIN.";
    public const string MultipleOrConditionsRecommendation = "Используйте IN, UNION или индекс для оптимизации OR.";
    public const string SubqueryInSelectRecommendation = "Вынесите подзапрос в JOIN или CTE.";
    public const string FunctionOnIndexColumnRecommendation = "Создайте функциональный индекс или перепишите условие.";
    public const string HavingWithoutGroupByRecommendation = "Перенесите условие в WHERE или добавьте GROUP BY.";
    public const string BetweenWithNullsRecommendation = "Обработайте NULL явно или исключите их до BETWEEN.";
    public const string InefficientLikePatternRecommendation = "Используйте trigram-индекс или полнотекстовый поиск.";
    public const string CrossJoinWithoutConditionRecommendation = "Добавьте условие или используйте JOIN вместо CROSS JOIN.";
    public const string InefficientDistinctRecommendation = "Удалите DISTINCT, если он не влияет на результат.";
    public const string WhereTrueOr1Equals1Recommendation = "Удалите лишнее условие.";
    public const string NullEqualsComparisonRecommendation = "Используйте IS NULL / IS NOT NULL вместо =/!= NULL.";
    public const string RedundantJoinRecommendation = "Удалите JOIN на неиспользуемую таблицу.";
    public const string NestedSelectStarRecommendation = "Укажите конкретные колонки в подзапросе.";
    public const string JoinOnInequalityRecommendation = "Избегайте JOIN по неравенству — пересмотрите логику.";
    public const string FunctionInJoinConditionRecommendation = "Вынесите функцию из условия JOIN или создайте функциональный индекс.";
    public const string OrInWhereWithoutIndexRecommendation = "Создайте составной индекс или перепишите OR на UNION.";
    public const string OverlyComplexCteRecommendation = "Упростите CTE или разбейте его на несколько шагов.";
    public const string ImplicitCastInJoinOrWhereRecommendation = "Приводите типы явно, чтобы индекс применялся.";
    public const string CaseInWhereRecommendation = "Вынесите CASE из WHERE или перепишите условие.";
    public const string AggregateOnUnindexedRecommendation = "Создайте индекс для ускорения MIN/MAX/COUNT.";
    public const string SelectWithoutFromRecommendation = "Удалите SELECT без FROM, если это отладочный код.";
}
