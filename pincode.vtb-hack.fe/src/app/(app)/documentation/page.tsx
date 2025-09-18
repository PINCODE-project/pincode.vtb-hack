"use client";

import React from "react";
import { Badge, ScrollArea, Separator } from "@pin-code/ui-kit";
import { Database, Code, Archive, Zap, Lock, HardDrive } from "lucide-react";
import { TableOfContents } from "@/components/documentation/TableOfContents";

interface SQLRule {
	rule: string;
	description: string;
	recommendation: string;
}

interface MetricSection {
	title: string;
	items: MetricItem[];
}

interface MetricItem {
	title: string;
	metric?: string;
	threshold?: string;
	level?: "critical" | "high" | "medium" | "low";
	description: string;
	recommendations: string[];
}

/**
 * Страница документации с правилами и рекомендациями
 */
export default function DocumentationPage() {
	// Структура многоуровневого оглавления
	const tableOfContentsItems = [
		// Анализ настроек БД
		{
			title: "Анализ настроек БД",
			url: "#database-settings-analysis",
			depth: 1,
		},
		{
			title: "Метрики Autovacuum",
			url: "#autovacuum-metrics",
			depth: 2,
		},
		{
			title: "Метрики Cache",
			url: "#cache-metrics",
			depth: 2,
		},
		{
			title: "Метрики Блокировок",
			url: "#lock-metrics",
			depth: 2,
		},
		{
			title: "Метрики Индексов",
			url: "#index-metrics",
			depth: 2,
		},
		{
			title: "Метрики Временных файлов",
			url: "#temp-files-metrics",
			depth: 2,
		},
		// Алгоритмический анализ SQL запросов
		{
			title: "Алгоритмический анализ SQL запросов",
			url: "#sql-analysis-section",
			depth: 1,
		},
		{
			title: "Правила анализа SQL-запроса",
			url: "#sql-rules-analysis",
			depth: 2,
		},
		{
			title: "Правила анализа Explain запроса",
			url: "#explain-rules-analysis",
			depth: 2,
		},
	];

	// SQL правила из data.txt
	const sqlRules: SQLRule[] = [
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

	// Метрики БД из data2.txt
	const autovacuumMetrics: MetricSection = {
		title: "Метрики Autovacuum",
		items: [
			{
				title: "Высокий системный уровень мертвых tuples",
				metric: "SystemWideDeadTupleRatio",
				threshold: "> 15%",
				level: "high",
				description:
					"Системный уровень мертвых tuples превышает 15%, что указывает на необходимость более агрессивной очистки.",
				recommendations: [
					"Уменьшить параметр autovacuum_vacuum_scale_factor для более частого запуска autovacuum.",
				],
			},
			{
				title: "Большое количество проблемных таблиц",
				metric: "TablesAbove20Percent vs TotalTables",
				threshold: "> 30% таблиц имеют >20% мертвых tuples",
				level: "medium",
				description:
					"Более 30% таблиц в системе имеют высокий уровень мертвых tuples (>20%), что указывает на нехватку рабочих процессов autovacuum.",
				recommendations: [
					"Увеличить количество рабочих процессов autovacuum для параллельной обработки таблиц.",
				],
			},
			{
				title: "Критические таблицы с высоким уровнем мертвых tuples",
				metric: "DeadTupleRatio и ChangeRatePercent",
				threshold: "Таблицы с пометкой critical",
				level: "critical",
				description:
					"Критические таблицы с экстремально высоким уровнем мертвых tuples и быстрым ростом этого показателя.",
				recommendations: [
					"Индивидуальная настройка параметра autovacuum_vacuum_scale_factor для конкретной таблицы",
					"50% мертвых tuples → scale_factor = 0.02",
					"≤50% мертвых tuples → scale_factor = 0.05",
				],
			},
		],
	};

	const cacheMetrics: MetricSection = {
		title: "Метрики Cache",
		items: [
			{
				title: "Низкий Cache Hit Ratio",
				metric: "AvgCacheHitRatio",
				threshold: "< 90%",
				level: "high",
				description:
					"Процент попаданий в кэш ниже оптимального уровня, что указывает на неэффективное использование памяти.",
				recommendations: ["Увеличить размер shared_buffers или оптимизировать рабочие нагрузки."],
			},
			{
				title: "Высокая дисковая активность",
				metric: "BlksReadPerMinute",
				threshold: "> 1000 чтений/мин",
				level: "high",
				description: "Высокая частота чтений с диска указывает на недостаточность размера кэша.",
				recommendations: [
					"Увеличить размер shared_buffers, оптимизировать запросы или рассмотреть добавление RAM.",
				],
			},
			{
				title: "Критическая нехватка памяти",
				metric: "Комбинация AvgCacheHitRatio и BlksReadPerMinute",
				threshold: "Cache Hit Ratio < 85% И Disk Reads > 2000 чтений/мин",
				level: "critical",
				description:
					"Критическая ситуация с крайне низким процентом попаданий в кэш и высокой дисковой нагрузкой.",
				recommendations: ["Немедленно увеличить размер shared_buffers и провести анализ рабочих нагрузок."],
			},
			{
				title: "Отличная производительность кэша",
				metric: "Комбинация AvgCacheHitRatio и BlksReadPerMinute",
				threshold: "Cache Hit Ratio ≥ 90% И Disk Reads < 100 чтений/мин",
				level: "low",
				description:
					"Кэш работает оптимально с высоким процентом попаданий и минимальной дисковой активностью.",
				recommendations: ["Текущая конфигурация эффективна, поддерживать текущие настройки."],
			},
		],
	};

	const indexMetrics: MetricSection = {
		title: "Метрики Индексов",
		items: [
			{
				title: "Неиспользуемые индексы",
				metric: "averageScans и maxSize",
				threshold: "Различные пороги по размеру",
				level: "high",
				description: "Индексы, которые редко или совсем не используются, но занимают место на диске.",
				recommendations: [
					"Критический уровень (> 100 MB): Удалить индекс с использованием CONCURRENTLY",
					"Средний уровень (10-100 MB): Рассмотреть возможность удаления",
					"Низкий уровень (≤ 10 MB): Можно оставить, если размер не критичен",
				],
			},
			{
				title: "Неэффективные индексы",
				metric: "efficiency в процентах",
				threshold: "< 30% эффективности",
				level: "high",
				description:
					"Индексы с низкой эффективностью, которые читают много данных для возврата малого количества результатов.",
				recommendations: [
					"< 10%: Пересмотреть необходимость существования индекса",
					"10-30%: Добавить более селективные колонки в индекс",
					"> 30%: Оптимизировать порядок колонок в индексе",
				],
			},
			{
				title: "Быстрорастущие индексы",
				metric: "growthPercentage",
				threshold: "> 50% роста",
				level: "critical",
				description: "Индексы, которые быстро увеличиваются в размере, что может указывать на фрагментацию.",
				recommendations: [
					"Выполнить REINDEX INDEX CONCURRENTLY",
					"Оптимизировать fillfactor для таблиц с частыми обновлениями",
					"Рассмотреть partitioning таблицы",
				],
			},
			{
				title: "Интенсивно используемые индексы",
				metric: "averageScans и efficiency",
				threshold: "Высокое использование с низкой эффективностью",
				level: "medium",
				description: "Индексы с высокой нагрузкой, которые нуждаются в оптимизации.",
				recommendations: [
					"Срочно оптимизировать структуру индекса для низкой эффективности",
					"Продолжать мониторить для хорошей эффективности",
					"Обеспечить регулярное обслуживание индекса",
				],
			},
		],
	};

	const lockMetrics: MetricSection = {
		title: "Метрики Блокировок",
		items: [
			{
				title: "Долгие AccessExclusiveLock блокировки",
				metric: "Продолжительность блокировки",
				threshold: "> 10 секунд",
				level: "critical",
				description: "Блокировки AccessExclusiveLock продолжительностью более 10 секунд.",
				recommendations: [
					"Немедленно провести анализ процессов, удерживающих долгие блокировки",
					"Перенести DDL-операции в период минимальной нагрузки",
					"Использовать инструменты мониторинга для отслеживания длительных блокировок",
				],
			},
			{
				title: "Блокировки транзакций с долгим ожиданием",
				metric: "Время ожидания",
				threshold: "> 30 секунд",
				level: "high",
				description: "Блокировки типа transactionid с ожиданием более 30 секунд.",
				recommendations: [
					"Выявить транзакции, вызывающие взаимоблокировки",
					"Оптимизировать порядок выполнения операций в транзакциях",
					"Установить разумные таймауты для операций",
				],
			},
			{
				title: "Блокировки системных таблиц",
				metric: "OID таблицы",
				threshold: "< 16384 (системные таблицы)",
				level: "critical",
				description: "Блокировки на таблицах с OID < 16384 (системные таблицы).",
				recommendations: [
					"Немедленно исследовать процессы, блокирующие системные таблицы",
					"Избегать длительных транзакций, затрагивающих системные каталоги",
					"Рассмотреть возможность экстренного вмешательства",
				],
			},
			{
				title: "Множественные блокировки отношений",
				metric: "Количество блокировок relation",
				threshold: "> 5 одновременно",
				level: "medium",
				description: "Более 5 блокировок типа relation одновременно.",
				recommendations: [
					"Проанализировать шаблоны доступа к часто блокируемым таблицам",
					"Рассмотреть возможность сегментирования горячих таблиц",
					"Оптимизировать бизнес-логику для уменьшения конкуренции",
				],
			},
		],
	};

	const tempFilesMetrics: MetricSection = {
		title: "Метрики Временных файлов",
		items: [
			{
				title: "Критический рост объема временных файлов",
				metric: "Скорость записи во временные файлы",
				threshold: "> 1 MB/сек",
				level: "critical",
				description: "Серьезная нехватка памяти work_mem для операций сортировки, агрегации и хэширования.",
				recommendations: [
					"Немедленно увеличить параметр work_mem для пользовательских сессий",
					"Оптимизировать запросы с большими сортировками и агрегациями",
					"Рассмотреть увеличение общей оперативной памяти сервера",
				],
			},
			{
				title: "Высокий рост количества временных файлов",
				metric: "Количество файлов в минуту",
				threshold: "> 2 файлов/мин",
				level: "high",
				description: "Нехватка памяти work_mem для операций сортировки и агрегации.",
				recommendations: [
					"Увеличить параметр work_mem в конфигурации PostgreSQL",
					"Проанализировать и оптимизировать запросы с ORDER BY, GROUP BY, DISTINCT",
					"Создать индексы для поддержки операций сортировки и агрегации",
				],
			},
			{
				title: "Стабильная работа",
				metric: "Комбинированные показатели",
				threshold: "< 2 файлов/мин И < 1 MB/сек",
				level: "low",
				description:
					"Система работает стабильно, операции сортировки и агрегации преимущественно выполняются в памяти.",
				recommendations: [
					"Продолжать регулярный мониторинг показателей",
					"Периодически анализировать планы запросов на предмет эффективности",
					"Следить за ростом объема данных и корректировать настройки памяти",
				],
			},
		],
	};

	const getLevelBadge = (level?: "critical" | "high" | "medium" | "low") => {
		const variant =
			level === "critical" || level === "high" ? "destructive" : level === "medium" ? "secondary" : "default";
		return level ? (
			<Badge variant={variant} className="text-xs">
				{level}
			</Badge>
		) : null;
	};

	return (
		<div className="flex h-screen">
			{/* Боковое оглавление */}
			<div className="w-80 border-r bg-muted/10 p-4">
				<ScrollArea className="h-[calc(100vh-2rem)]">
					<TableOfContents
						items={tableOfContentsItems}
						title="Оглавление"
						defaultOpen={true}
						className="mb-4"
					/>
				</ScrollArea>
			</div>

			{/* Основной контент */}
			<div className="flex-1 overflow-auto">
				<div className="max-w-4xl mx-auto p-8">
					<div className="mb-8">
						<h1 className="text-3xl font-bold tracking-tight mb-2">Документация PostgreSQL</h1>
						<p className="text-muted-foreground">
							Правила анализа и рекомендации по оптимизации базы данных PostgreSQL
						</p>
					</div>

					{/* Анализ настроек БД */}
					<section id="database-settings-analysis" className="mb-12">
						<h1 className="text-3xl font-bold mb-6 flex items-center gap-2">
							<Database className="h-8 w-8" />
							Анализ настроек БД
						</h1>
						<p className="text-muted-foreground mb-8">
							Метрики и рекомендации для оптимизации производительности PostgreSQL
						</p>

						{/* Autovacuum метрики */}
						<section id="autovacuum-metrics" className="mb-8">
							<h2 className="text-2xl font-bold mb-4 flex items-center gap-2">
								<Archive className="h-6 w-6" />
								Метрики Autovacuum
							</h2>

							<div className="space-y-6">
								{autovacuumMetrics.items.map((item, index) => (
									<div key={index} className="border-l-4 border-l-blue-500 pl-4">
										<div className="flex items-center gap-2 mb-2">
											<h3 className="text-lg font-semibold">{item.title}</h3>
											{getLevelBadge(item.level)}
										</div>
										<div className="space-y-2 text-sm">
											{item.metric && (
												<p>
													<strong>Метрика:</strong> {item.metric}
												</p>
											)}
											{item.threshold && (
												<p>
													<strong>Пороговое значение:</strong> {item.threshold}
												</p>
											)}
											<p className="text-muted-foreground">{item.description}</p>
											<div>
												<strong>Рекомендации:</strong>
												<ul className="mt-1 ml-4 space-y-1">
													{item.recommendations.map((rec, recIndex) => (
														<li key={recIndex} className="list-disc">
															{rec}
														</li>
													))}
												</ul>
											</div>
										</div>
									</div>
								))}
							</div>
						</section>

						<Separator className="my-6" />

						{/* Cache метрики */}
						<section id="cache-metrics" className="mb-8">
							<h2 className="text-2xl font-bold mb-4 flex items-center gap-2">
								<Zap className="h-6 w-6" />
								Метрики Cache
							</h2>

							<div className="space-y-6">
								{cacheMetrics.items.map((item, index) => (
									<div key={index} className="border-l-4 border-l-green-500 pl-4">
										<div className="flex items-center gap-2 mb-2">
											<h3 className="text-lg font-semibold">{item.title}</h3>
											{getLevelBadge(item.level)}
										</div>
										<div className="space-y-2 text-sm">
											{item.metric && (
												<p>
													<strong>Метрика:</strong> {item.metric}
												</p>
											)}
											{item.threshold && (
												<p>
													<strong>Пороговое значение:</strong> {item.threshold}
												</p>
											)}
											<p className="text-muted-foreground">{item.description}</p>
											<div>
												<strong>Рекомендации:</strong>
												<ul className="mt-1 ml-4 space-y-1">
													{item.recommendations.map((rec, recIndex) => (
														<li key={recIndex} className="list-disc">
															{rec}
														</li>
													))}
												</ul>
											</div>
										</div>
									</div>
								))}
							</div>
						</section>

						<Separator className="my-6" />

						{/* Lock метрики */}
						<section id="lock-metrics" className="mb-8">
							<h2 className="text-2xl font-bold mb-4 flex items-center gap-2">
								<Lock className="h-6 w-6" />
								Метрики Блокировок
							</h2>

							<div className="space-y-6">
								{lockMetrics.items.map((item, index) => (
									<div key={index} className="border-l-4 border-l-red-500 pl-4">
										<div className="flex items-center gap-2 mb-2">
											<h3 className="text-lg font-semibold">{item.title}</h3>
											{getLevelBadge(item.level)}
										</div>
										<div className="space-y-2 text-sm">
											{item.metric && (
												<p>
													<strong>Метрика:</strong> {item.metric}
												</p>
											)}
											{item.threshold && (
												<p>
													<strong>Пороговое значение:</strong> {item.threshold}
												</p>
											)}
											<p className="text-muted-foreground">{item.description}</p>
											<div>
												<strong>Рекомендации:</strong>
												<ul className="mt-1 ml-4 space-y-1">
													{item.recommendations.map((rec, recIndex) => (
														<li key={recIndex} className="list-disc">
															{rec}
														</li>
													))}
												</ul>
											</div>
										</div>
									</div>
								))}
							</div>
						</section>

						<Separator className="my-6" />

						{/* Index метрики */}
						<section id="index-metrics" className="mb-8">
							<h2 className="text-2xl font-bold mb-4 flex items-center gap-2">
								<Database className="h-6 w-6" />
								Метрики Индексов
							</h2>

							<div className="space-y-6">
								{indexMetrics.items.map((item, index) => (
									<div key={index} className="border-l-4 border-l-purple-500 pl-4">
										<div className="flex items-center gap-2 mb-2">
											<h3 className="text-lg font-semibold">{item.title}</h3>
											{getLevelBadge(item.level)}
										</div>
										<div className="space-y-2 text-sm">
											{item.metric && (
												<p>
													<strong>Метрика:</strong> {item.metric}
												</p>
											)}
											{item.threshold && (
												<p>
													<strong>Пороговое значение:</strong> {item.threshold}
												</p>
											)}
											<p className="text-muted-foreground">{item.description}</p>
											<div>
												<strong>Рекомендации:</strong>
												<ul className="mt-1 ml-4 space-y-1">
													{item.recommendations.map((rec, recIndex) => (
														<li key={recIndex} className="list-disc">
															{rec}
														</li>
													))}
												</ul>
											</div>
										</div>
									</div>
								))}
							</div>
						</section>

						<Separator className="my-6" />

						{/* Temp files метрики */}
						<section id="temp-files-metrics" className="mb-8">
							<h2 className="text-2xl font-bold mb-4 flex items-center gap-2">
								<HardDrive className="h-6 w-6" />
								Метрики Временных файлов
							</h2>

							<div className="space-y-6">
								{tempFilesMetrics.items.map((item, index) => (
									<div key={index} className="border-l-4 border-l-orange-500 pl-4">
										<div className="flex items-center gap-2 mb-2">
											<h3 className="text-lg font-semibold">{item.title}</h3>
											{getLevelBadge(item.level)}
										</div>
										<div className="space-y-2 text-sm">
											{item.metric && (
												<p>
													<strong>Метрика:</strong> {item.metric}
												</p>
											)}
											{item.threshold && (
												<p>
													<strong>Пороговое значение:</strong> {item.threshold}
												</p>
											)}
											<p className="text-muted-foreground">{item.description}</p>
											<div>
												<strong>Рекомендации:</strong>
												<ul className="mt-1 ml-4 space-y-1">
													{item.recommendations.map((rec, recIndex) => (
														<li key={recIndex} className="list-disc">
															{rec}
														</li>
													))}
												</ul>
											</div>
										</div>
									</div>
								))}
							</div>
						</section>
					</section>

					<Separator className="my-12" />

					{/* Алгоритмический анализ SQL запросов */}
					<section id="sql-analysis-section" className="mb-12">
						<h1 className="text-3xl font-bold mb-6 flex items-center gap-2">
							<Code className="h-8 w-8" />
							Алгоритмический анализ SQL запросов
						</h1>
						<p className="text-muted-foreground mb-8">
							Правила анализа SQL-запросов и планов выполнения для выявления проблем производительности
						</p>

						{/* SQL правила */}
						<section id="sql-rules-analysis" className="mb-8">
							<h2 className="text-2xl font-bold mb-4">Правила анализа SQL-запроса</h2>
							<p className="text-muted-foreground mb-6">
								Наиболее распространенные проблемы в SQL-запросах и способы их решения
							</p>

							<div className="space-y-4">
								{sqlRules.map((rule, index) => (
									<div key={index} className="border-l-4 border-l-blue-500 pl-4">
										<div className="mb-2">
											<Badge variant="outline" className="text-xs mb-2">
												{rule.rule}
											</Badge>
										</div>
										<div className="space-y-2 text-sm">
											<p>
												<strong>Проблема:</strong> {rule.description}
											</p>
											<p>
												<strong>Рекомендация:</strong> {rule.recommendation}
											</p>
										</div>
									</div>
								))}
							</div>
						</section>

						<Separator className="my-6" />

						{/* Explain правила (пустой раздел с заглушкой) */}
						<section id="explain-rules-analysis" className="mb-8">
							<h2 className="text-2xl font-bold mb-4">Правила анализа Explain запроса</h2>
							<div className="text-sm text-muted-foreground p-6 bg-muted/20 rounded border-l-4 border-l-yellow-500">
								<p>
									<strong>В разработке:</strong> Правила анализа EXPLAIN запросов будут добавлены в
									следующем обновлении документации.
								</p>
							</div>
						</section>
					</section>
				</div>
			</div>
		</div>
	);
}
