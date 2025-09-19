import { CompletionContext, CompletionResult } from "@codemirror/autocomplete";

// SQL ключевые слова
export const SQL_KEYWORDS = [
	// Базовые команды
	"SELECT",
	"INSERT",
	"UPDATE",
	"DELETE",
	"CREATE",
	"DROP",
	"ALTER",
	"TRUNCATE",

	// Структура запросов
	"FROM",
	"WHERE",
	"GROUP BY",
	"HAVING",
	"ORDER BY",
	"LIMIT",
	"OFFSET",
	"JOIN",
	"INNER JOIN",
	"LEFT JOIN",
	"RIGHT JOIN",
	"FULL JOIN",
	"CROSS JOIN",
	"ON",
	"USING",
	"AS",
	"DISTINCT",
	"ALL",
	"UNION",
	"INTERSECT",
	"EXCEPT",

	// Типы данных
	"INTEGER",
	"INT",
	"BIGINT",
	"SMALLINT",
	"DECIMAL",
	"NUMERIC",
	"REAL",
	"DOUBLE PRECISION",
	"SERIAL",
	"BIGSERIAL",
	"MONEY",
	"CHARACTER VARYING",
	"VARCHAR",
	"CHARACTER",
	"CHAR",
	"TEXT",
	"BYTEA",
	"TIMESTAMP",
	"DATE",
	"TIME",
	"INTERVAL",
	"BOOLEAN",
	"POINT",
	"LINE",
	"LSEG",
	"BOX",
	"PATH",
	"POLYGON",
	"CIRCLE",
	"CIDR",
	"INET",
	"MACADDR",
	"UUID",
	"XML",
	"JSON",
	"JSONB",
	"ARRAY",

	// Ограничения
	"PRIMARY KEY",
	"FOREIGN KEY",
	"REFERENCES",
	"UNIQUE",
	"NOT NULL",
	"CHECK",
	"DEFAULT",
	"CONSTRAINT",
	"INDEX",
	"SEQUENCE",

	// Условия
	"AND",
	"OR",
	"NOT",
	"IN",
	"EXISTS",
	"BETWEEN",
	"LIKE",
	"ILIKE",
	"IS NULL",
	"IS NOT NULL",
	"CASE",
	"WHEN",
	"THEN",
	"ELSE",
	"END",

	// Агрегатные функции
	"COUNT",
	"SUM",
	"AVG",
	"MIN",
	"MAX",
	"ARRAY_AGG",
	"STRING_AGG",

	// Функции работы с датами
	"NOW",
	"CURRENT_DATE",
	"CURRENT_TIME",
	"CURRENT_TIMESTAMP",
	"AGE",
	"EXTRACT",
	"DATE_PART",
	"DATE_TRUNC",
	"TO_CHAR",
	"TO_DATE",
	"TO_TIMESTAMP",

	// Строковые функции
	"CONCAT",
	"LENGTH",
	"CHAR_LENGTH",
	"LOWER",
	"UPPER",
	"TRIM",
	"LTRIM",
	"RTRIM",
	"SUBSTRING",
	"POSITION",
	"REPLACE",
	"SPLIT_PART",
	"REGEXP_REPLACE",
	"REGEXP_SPLIT_TO_TABLE",

	// Математические функции
	"ABS",
	"CEIL",
	"FLOOR",
	"ROUND",
	"TRUNC",
	"MOD",
	"POWER",
	"SQRT",
	"EXP",
	"LN",
	"LOG",
	"SIN",
	"COS",
	"TAN",
	"ASIN",
	"ACOS",
	"ATAN",
	"RANDOM",

	// Функции преобразования типов
	"CAST",
	"CONVERT",
	"COALESCE",
	"NULLIF",

	// Системные функции
	"VERSION",
	"CURRENT_USER",
	"SESSION_USER",
	"CURRENT_DATABASE",
	"CURRENT_SCHEMA",

	// Окенные функции
	"ROW_NUMBER",
	"RANK",
	"DENSE_RANK",
	"NTILE",
	"LAG",
	"LEAD",
	"FIRST_VALUE",
	"LAST_VALUE",
	"OVER",
	"PARTITION BY",
	"ROWS",
	"RANGE",
	"UNBOUNDED PRECEDING",
	"UNBOUNDED FOLLOWING",
	"CURRENT ROW",

	// Управление транзакциями
	"BEGIN",
	"COMMIT",
	"ROLLBACK",
	"SAVEPOINT",
	"START TRANSACTION",
	"END",

	// Права доступа
	"GRANT",
	"REVOKE",
	"ROLE",
	"USER",
	"PRIVILEGE",

	// Прочие
	"WITH",
	"RECURSIVE",
	"LATERAL",
	"RETURNING",
	"EXPLAIN",
	"ANALYZE",
	"VERBOSE",
	"COMMENT",
	"COPY",
	"VACUUM",
	"REINDEX",
	"CLUSTER",
];

// PostgreSQL специфичные функции и операторы
export const POSTGRESQL_FUNCTIONS = [
	// JSON функции
	"JSON_BUILD_ARRAY",
	"JSON_BUILD_OBJECT",
	"JSON_OBJECT",
	"JSON_ARRAY",
	"JSON_EXTRACT_PATH",
	"JSON_EXTRACT_PATH_TEXT",
	"JSONB_BUILD_ARRAY",
	"JSONB_BUILD_OBJECT",
	"JSONB_EXTRACT_PATH",
	"JSONB_EXTRACT_PATH_TEXT",
	"JSONB_PRETTY",
	"JSONB_SET",
	"JSONB_INSERT",
	"JSONB_AGG",
	"JSON_AGG",
	"TO_JSON",
	"TO_JSONB",

	// Массивы
	"ARRAY_LENGTH",
	"ARRAY_POSITION",
	"ARRAY_POSITIONS",
	"ARRAY_APPEND",
	"ARRAY_PREPEND",
	"ARRAY_CAT",
	"ARRAY_REMOVE",
	"ARRAY_REPLACE",
	"UNNEST",
	"ARRAY_TO_STRING",
	"STRING_TO_ARRAY",
	"ARRAY_DIMS",
	"CARDINALITY",

	// Регулярные выражения
	"REGEXP_MATCHES",
	"REGEXP_SPLIT_TO_ARRAY",
	"REGEXP_REPLACE",

	// Полнотекстовый поиск
	"TO_TSVECTOR",
	"TO_TSQUERY",
	"PLAINTO_TSQUERY",
	"PHRASETO_TSQUERY",
	"WEBSEARCH_TO_TSQUERY",
	"TS_RANK",
	"TS_RANK_CD",
	"TS_HEADLINE",

	// Геометрия и география
	"ST_DISTANCE",
	"ST_WITHIN",
	"ST_CONTAINS",
	"ST_INTERSECTS",
	"ST_OVERLAPS",
	"ST_TOUCHES",
	"ST_CROSSES",
	"ST_DISJOINT",
	"ST_AREA",
	"ST_LENGTH",
	"ST_PERIMETER",
	"ST_CENTROID",
	"ST_BUFFER",
	"ST_UNION",
	"ST_INTERSECTION",

	// Криптография
	"MD5",
	"SHA1",
	"SHA224",
	"SHA256",
	"SHA384",
	"SHA512",
	"CRYPT",
	"GEN_SALT",
	"ENCODE",
	"DECODE",

	// Системная информация
	"PG_SIZE_PRETTY",
	"PG_DATABASE_SIZE",
	"PG_TABLE_SIZE",
	"PG_INDEXES_SIZE",
	"PG_TOTAL_RELATION_SIZE",
	"PG_COLUMN_SIZE",
	"PG_BACKEND_PID",
	"PG_POSTMASTER_START_TIME",
];

// Операторы
export const SQL_OPERATORS = [
	"=",
	"!=",
	"<>",
	"<",
	">",
	"<=",
	">=",
	"||",
	"&&",
	">>",
	"<<",
	"->",
	"->>",
	"#>",
	"#>>",
	"?",
	"?&",
	"?|",
	"@>",
	"<@",
	"@@",
	"~",
	"~*",
	"!~",
	"!~*",
];

// Создаем список всех автодополнений
const ALL_COMPLETIONS = [
	...SQL_KEYWORDS.map((keyword) => ({
		label: keyword,
		type: "keyword",
		info: `SQL ключевое слово: ${keyword}`,
	})),
	...POSTGRESQL_FUNCTIONS.map((func) => ({
		label: func,
		type: "function",
		info: `PostgreSQL функция: ${func}`,
	})),
	...SQL_OPERATORS.map((op) => ({
		label: op,
		type: "operator",
		info: `SQL оператор: ${op}`,
	})),
];

// Функция автокомплита
export function sqlCompletions(context: CompletionContext): CompletionResult | null {
	const word = context.matchBefore(/\w*/);
	if (!word) return null;

	const from = word.from;
	const to = word.to;
	const text = word.text.toUpperCase();

	// Если текст слишком короткий, не показываем автокомплит
	if (text.length < 1) return null;

	// Фильтруем совпадения с умной сортировкой
	const options = ALL_COMPLETIONS.filter((completion) => completion.label.includes(text)).sort((a, b) => {
		// Сначала точные совпадения с начала
		const aStartsWith = a.label.startsWith(text);
		const bStartsWith = b.label.startsWith(text);

		if (aStartsWith && !bStartsWith) return -1;
		if (!aStartsWith && bStartsWith) return 1;

		// Потом по типу: keywords -> functions -> operators
		const typeOrder = { keyword: 0, function: 1, operator: 2 };
		const aTypeOrder = typeOrder[a.type as keyof typeof typeOrder] ?? 3;
		const bTypeOrder = typeOrder[b.type as keyof typeof typeOrder] ?? 3;

		if (aTypeOrder !== bTypeOrder) return aTypeOrder - bTypeOrder;

		// И наконец по алфавиту
		return a.label.localeCompare(b.label);
	});

	if (options.length === 0) return null;

	return {
		from,
		to,
		options: options.slice(0, 20).map((completion) => ({
			label: completion.label,
			type: completion.type,
			info: completion.info,
			apply: completion.label,
			boost: completion.label.startsWith(text) ? 1 : 0,
		})),
	};
}
