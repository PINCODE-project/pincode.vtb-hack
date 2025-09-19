export interface SQLRule {
	rule: string;
	description: string;
	recommendation: string;
	level?: "critical" | "high" | "medium" | "low";
}

/**
 * Данные SQL правил для алгоритмического анализа запросов
 */
export const sqlRules: SQLRule[] = [
	{
		rule: "IN vs EXISTS",
		description: "Неверный выбор конструкции может замедлить запрос.",
		recommendation: "EXISTS лучше для коррелированных подзапросов, IN — для малых фиксированных списков.",
	},
	{
		rule: "Неявный CROSS JOIN",
		description: "FROM с несколькими таблицами без условия приводит к CROSS JOIN.",
		recommendation: "Явно укажите INNER JOIN или CROSS JOIN.",
	},
	{
		rule: "BETWEEN с NULL",
		description: "BETWEEN игнорирует строки с NULL, что может дать неожиданный результат.",
		recommendation: "Явно исключайте NULL или добавьте IS NOT NULL.",
	},
	{
		rule: "SELECT *",
		description:
			"Запрос SELECT * возвращает все колонки, что создаёт избыточный трафик и ломает планы при изменении схемы.",
		recommendation: "Указывайте только нужные колонки явно.",
	},
	{
		rule: "SELECT * в подзапросе",
		description: "SELECT * в подзапросе возвращает ненужные колонки.",
		recommendation: "Укажите конкретные колонки.",
	},
	{
		rule: "ORDER BY в подзапросе",
		description: "ORDER BY внутри подзапроса без LIMIT не влияет на результат.",
		recommendation: "Удалите ORDER BY внутри подзапроса.",
	},
	{
		rule: "Лишний JOIN",
		description: "Таблица в JOIN не используется в SELECT/WHERE.",
		recommendation: "Удалите JOIN.",
	},
	{
		rule: "Лишние CAST",
		description: "Приведения вида col::text::text бесполезны и замедляют выполнение.",
		recommendation: "Удалите избыточные CAST.",
	},
	{
		rule: "WHERE TRUE/1=1",
		description: "Условие всегда истинно и бесполезно.",
		recommendation: "Удалите его.",
	},
	{
		rule: "SELECT без FROM",
		description: "SELECT без FROM часто остаётся от отладки.",
		recommendation: "Удалите его.",
	},
	{
		rule: "GROUP BY без агрегатов",
		description: "GROUP BY без агрегатных функций обычно не нужен и сигнализирует о логической ошибке.",
		recommendation: "Уберите GROUP BY или добавьте агрегаты.",
	},
	{
		rule: "DISTINCT + GROUP BY",
		description: "DISTINCT после GROUP BY избыточен — GROUP BY сам убирает дубликаты.",
		recommendation: "Уберите DISTINCT.",
	},
	{
		rule: "DISTINCT без смысла",
		description: "DISTINCT без необходимости только замедляет.",
		recommendation: "Уберите DISTINCT.",
	},
	{
		rule: "DISTINCT без JOIN",
		description: "DISTINCT без JOIN/GROUP BY не даёт выгоды и замедляет запрос.",
		recommendation: "Уберите DISTINCT, если дубликатов быть не может.",
	},
	{
		rule: "Функция в JOIN",
		description: "Функция в условии JOIN блокирует индекс.",
		recommendation: "Создайте функциональный индекс или перепишите условие.",
	},
	{
		rule: "JOIN без индекса",
		description: "JOIN по неиндексированным колонкам может привести к Nested Loop с Seq Scan.",
		recommendation: "Создайте индекс по условию JOIN.",
	},
	{
		rule: "OR без индекса",
		description: "OR без составного индекса вызывает Seq Scan.",
		recommendation: "Создайте индекс или замените OR на UNION.",
	},
	{
		rule: "IN (SELECT ...)",
		description: "IN (SELECT ...) может выполняться медленно при больших подзапросах.",
		recommendation: "Рассмотрите замену на JOIN/LEFT JOIN.",
	},
	{
		rule: "Сложный CTE",
		description: "CTE с вложенными SELECT перегружает оптимизатор.",
		recommendation: "Разбейте на несколько шагов или упростите логику.",
	},
	{
		rule: "Неявный CAST",
		description: "Автоприведение типов в WHERE/JOIN ломает индекс.",
		recommendation: "Приводите типы явно.",
	},
	{
		rule: "Сравнение UUID со строкой",
		description: "Сравнение UUID-колонки со строкой без ::uuid ломает оптимизацию, и индекс не используется.",
		recommendation: "Приводите строку явно к типу UUID ('id'::uuid).",
	},
	{
		rule: "Сравнение числа со строкой",
		description: "Колонка числового типа сравнивается с текстом (col = '123'), что делает индекс бесполезным.",
		recommendation: "Пишите числа как числовые литералы (123) вместо строк.",
	},
	{
		rule: "Подзапрос в SELECT",
		description: "Подзапрос в SELECT часто выполняется для каждой строки (N+1).",
		recommendation: "Перепишите через JOIN или вынесите в CTE.",
	},
	{
		rule: "Функция на колонке",
		description:
			"Если в условии WHERE/ON используется функция (например, LOWER(col)), то обычный индекс по col не применяется. Это приводит к Seq Scan.",
		recommendation:
			"Перепишите условие так, чтобы функция применялась к константе (LOWER('abc')) или создайте функциональный индекс по выражению.",
	},
	{
		rule: "CASE в WHERE",
		description: "CASE внутри WHERE делает условие несаргируемым.",
		recommendation: "Перепишите условие на простые OR/AND.",
	},
	{
		rule: "Арифметика на колонке",
		description: "Выражения вида col+1 = 5 приводят к Seq Scan, так как индекс по col не используется.",
		recommendation: "Перепишите условие (col = 4) или используйте вычисляемые поля.",
	},
	{
		rule: "HAVING без GROUP BY",
		description: "HAVING без GROUP BY не имеет смысла и перегружает запрос.",
		recommendation: "Перенесите условие в WHERE.",
	},
	{
		rule: "LEFT JOIN + WHERE",
		description: "Условие в WHERE после LEFT JOIN убирает NULL-строки и превращает его в INNER JOIN.",
		recommendation: "Перенесите условие в ON или замените на INNER JOIN.",
	},
	{
		rule: "OFFSET пагинация",
		description: "OFFSET 100000 заставляет сервер всё равно пропустить 100000 строк, что крайне дорого.",
		recommendation: "Перейдите на keyset-пагинацию (WHERE id > last_id ORDER BY id).",
	},
	{
		rule: "LIKE с %...%",
		description: "LIKE '%abc%' не использует индекс.",
		recommendation: "Используйте trigram-индекс или fulltext.",
	},
	{
		rule: "LIKE с ведущим %",
		description: "LIKE '%abc' или ILIKE '%abc' не используют B-Tree индекс и приводят к Seq Scan.",
		recommendation: "Используйте trigram-индекс (pg_trgm) или полнотекстовый поиск.",
	},
	{
		rule: "ORDER BY RANDOM()",
		description: "Перемешивание строк через RANDOM() заставляет сортировать всю таблицу.",
		recommendation: "Используйте ORDER BY id OFFSET floor(random()*count) или другой подход.",
	},
	{
		rule: "NOT IN с NULL",
		description:
			"NOT IN возвращает пустой результат, если подзапрос содержит хотя бы один NULL. Это часто ведёт к логическим ошибкам.",
		recommendation: "Используйте NOT EXISTS или добавьте фильтр IS NOT NULL в подзапрос.",
	},
	{
		rule: "EXISTS (SELECT ...)",
		description: "EXISTS с некоррелированным подзапросом выполняется дороже, чем JOIN.",
		recommendation: "Используйте JOIN, если связь не коррелирована.",
	},
	{
		rule: "NULL =",
		description: "= NULL и != NULL не работают — всегда NULL.",
		recommendation: "Используйте IS NULL / IS NOT NULL.",
	},
	{
		rule: "CROSS JOIN без условия",
		description: "CROSS JOIN без условия даёт декартово произведение.",
		recommendation: "Используйте INNER JOIN или задайте условие.",
	},
	{
		rule: "Много OR",
		description: "OR с несколькими условиями вызывает Seq Scan.",
		recommendation: "Используйте IN, UNION или составной индекс.",
	},
	{
		rule: "COUNT(*) с JOIN",
		description: "COUNT(*) с JOIN считает дублирующиеся строки.",
		recommendation: "Используйте COUNT(DISTINCT col) или перепишите JOIN.",
	},
	{
		rule: "Функция на индексной колонке",
		description: "Функции на колонках в условиях блокируют использование индекса.",
		recommendation: "Используйте функциональный индекс или перепишите условие.",
	},
	{
		rule: "Сравнение даты со строкой",
		description:
			"Дата/время сравниваются со строковым литералом (col = '2021-01-01'), что вызывает неявное приведение и Seq Scan.",
		recommendation: "Используйте литералы даты/времени (DATE '2021-01-01') или явное приведение.",
	},
	{
		rule: "JOIN по неравенству",
		description: "JOIN по условию <> или < вызывает полные сканы.",
		recommendation: "Избегайте неравенств в JOIN.",
	},
	{
		rule: "UNION без ALL",
		description: "UNION всегда убирает дубли, что требует сортировки/хеша.",
		recommendation: "Если дубликаты не критичны — используйте UNION ALL.",
	},
	{
		rule: "DELETE/UPDATE без WHERE",
		description: "Удаляются или обновляются все строки таблицы. Это может быть катастрофической ошибкой.",
		recommendation: "Добавьте WHERE или используйте ограничение через подзапрос.",
	},
	{
		rule: "LIMIT без ORDER BY",
		description: "LIMIT без сортировки возвращает произвольные строки.",
		recommendation: "Добавьте ORDER BY для предсказуемости.",
	},
	{
		rule: "ORDER BY без LIMIT",
		description: "Сортировка без ограничения вынуждает сервер упорядочить всю таблицу.",
		recommendation: "Добавьте LIMIT или уберите ORDER BY, если он не нужен.",
	},
	{
		rule: "Агрегаты без индекса",
		description: "MIN/MAX/COUNT работают дольше без индекса.",
		recommendation: "Добавьте индекс по колонке.",
	},
	{
		rule: "FROM через запятую",
		description: "FROM t1, t2 без условий = декартово произведение (CROSS JOIN).",
		recommendation: "Всегда используйте явные JOIN с ON или JOIN ... USING.",
	},
];
