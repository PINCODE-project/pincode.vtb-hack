/**
 * Утилита для обнаружения SQL команд в тексте
 */

// Регулярные выражения для обнаружения SQL команд
const SQL_PATTERNS = [
	// CREATE INDEX - расширенный паттерн до точки с запятой или конца строки
	/CREATE\s+(?:UNIQUE\s+)?INDEX\s+[^;]*(?:;|$)/gi,
	// ALTER TABLE - расширенный паттерн
	/ALTER\s+TABLE\s+[^;]*(?:;|$)/gi,
	// CREATE statements - расширенный паттерн
	/CREATE\s+(?:TABLE|VIEW|FUNCTION|PROCEDURE|TRIGGER)\s+[^;]*(?:;|$)/gi,
	// VACUUM, ANALYZE - расширенный паттерн
	/(?:VACUUM|ANALYZE)\s+[^;]*(?:;|$)/gi,
	// SET statements для конфигурации - расширенный паттерн
	/SET\s+[^;]*(?:;|$)/gi,
	// DROP statements - расширенный паттерн
	/DROP\s+(?:INDEX|TABLE|VIEW|FUNCTION|PROCEDURE|TRIGGER)\s+[^;]*(?:;|$)/gi,
	// UPDATE statistics - расширенный паттерн
	/UPDATE\s+[^;]*(?:;|$)/gi,
	// EXPLAIN statements
	/EXPLAIN\s+(?:\([^)]+\)\s+)?(?:SELECT|INSERT|UPDATE|DELETE)[\s\S]*?(?=\.|$)/gi,
	// Простые SQL команды в обратных кавычках
	/`([^`]*(?:SELECT|INSERT|UPDATE|DELETE|CREATE|ALTER|DROP|VACUUM|ANALYZE)[^`]*)`/gi,
	// SQL в одинарных кавычках
	/'([^']*(?:SELECT|INSERT|UPDATE|DELETE|CREATE|ALTER|DROP|VACUUM|ANALYZE)[^']*)'/gi,
];

export interface SqlSnippet {
	sql: string;
	startIndex: number;
	endIndex: number;
	type: "command" | "query";
}

/**
 * Извлекает SQL команды из текста
 */
export function extractSqlSnippets(text: string): SqlSnippet[] {
	const snippets: SqlSnippet[] = [];
	const processedRanges: Array<{ start: number; end: number }> = [];

	SQL_PATTERNS.forEach((pattern) => {
		let match;
		while ((match = pattern.exec(text)) !== null) {
			const startIndex = match.index;
			const endIndex = startIndex + match[0].length;

			// Проверяем, что этот диапазон еще не обработан
			const isOverlapping = processedRanges.some(
				(range) =>
					(startIndex >= range.start && startIndex <= range.end) ||
					(endIndex >= range.start && endIndex <= range.end) ||
					(startIndex <= range.start && endIndex >= range.end),
			);

			if (!isOverlapping) {
				let sql = match[1] || match[0]; // Используем группу захвата если есть, иначе весь матч
				sql = sql.trim();

				// Очищаем SQL от лишних символов
				sql = cleanSqlSnippet(sql);

				if (sql && isValidSql(sql)) {
					snippets.push({
						sql,
						startIndex,
						endIndex,
						type: getSqlType(sql),
					});

					processedRanges.push({ start: startIndex, end: endIndex });
				}
			}
		}
		// Сбрасываем lastIndex для повторного использования regex
		pattern.lastIndex = 0;
	});

	// Сортируем по позиции в тексте
	return snippets.sort((a, b) => a.startIndex - b.startIndex);
}

/**
 * Очищает SQL сниппет от лишних символов
 */
function cleanSqlSnippet(sql: string): string {
	return sql
		.replace(/^[`'"]+|[`'"]+$/g, "") // Убираем кавычки в начале и конце
		.replace(/;\s*$/, "") // Убираем точку с запятой в конце
		.trim();
}

/**
 * Проверяет, является ли строка валидной SQL командой
 */
function isValidSql(sql: string): boolean {
	// Минимальная длина
	if (sql.length < 3) return false;

	// Должна содержать хотя бы одно SQL ключевое слово
	const sqlKeywords = /\b(SELECT|INSERT|UPDATE|DELETE|CREATE|ALTER|DROP|VACUUM|ANALYZE|SET|EXPLAIN)\b/i;
	if (!sqlKeywords.test(sql)) return false;

	// Не должна быть просто текстом
	const wordCount = sql.split(/\s+/).length;
	if (wordCount < 2) return false;

	return true;
}

/**
 * Определяет тип SQL команды
 */
function getSqlType(sql: string): "command" | "query" {
	const commandPatterns = /^(CREATE|ALTER|DROP|VACUUM|ANALYZE|SET|INSERT|UPDATE|DELETE)/i;
	return commandPatterns.test(sql.trim()) ? "command" : "query";
}

/**
 * Заменяет SQL сниппеты в тексте на плейсхолдеры
 */
export function replaceSqlSnippetsWithPlaceholders(text: string, snippets: SqlSnippet[]): string {
	let result = text;
	let offset = 0;

	snippets.forEach((snippet, index) => {
		const placeholder = `__SQL_SNIPPET_${index}__`;
		const beforeSnippet = result.substring(0, snippet.startIndex + offset);
		const afterSnippet = result.substring(snippet.endIndex + offset);

		result = beforeSnippet + placeholder + afterSnippet;
		offset += placeholder.length - (snippet.endIndex - snippet.startIndex);
	});

	return result;
}
