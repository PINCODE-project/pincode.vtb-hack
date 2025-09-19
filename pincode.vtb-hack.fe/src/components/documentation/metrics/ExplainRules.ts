import type { SQLRule } from "./SQLRules";

/**
 * Данные Explain правил для алгоритмического анализа планов выполнения запросов
 */
export const explainRules: SQLRule[] = [
	{
		rule: "SeqScanOnLargeTable",
		description:
			"Последовательное сканирование большой таблицы (PlanRows={1}) может приводить к высоким затратам ввода-вывода и замедлению выполнения.",
		recommendation: "Создайте индекс по фильтруемым колонкам или рассмотрите партиционирование таблицы.",
	},
	{
		rule: "NestedLoopOnLargeTables",
		description:
			"Nested Loop Join между большими таблицами (PlanRows={0}) крайне неэффективен и может вызвать экспоненциальный рост времени выполнения.",
		recommendation: "Замените Nested Loop на Hash Join или Merge Join при работе с большими таблицами.",
	},
	{
		rule: "MisestimatedRows",
		description:
			"Сильное расхождение между фактическим количеством строк ({0}) и оценкой планировщика ({1}) в таблице '{2}' указывает на неточные статистики.",
		recommendation:
			"Запустите ANALYZE для актуализации статистик; при необходимости используйте CREATE STATISTICS для зависимых колонок.",
	},
	{
		rule: "HashAggOnLargeTable",
		description:
			"Hash Aggregate на большой таблице (PlanRows={0}) может вызвать значительное использование памяти или запись во временные файлы.",
		recommendation: "Увеличьте work_mem или предварительно сократите набор данных (фильтрация/CTE).",
	},
	{
		rule: "FunctionScan",
		description:
			"Вызов Function Scan по '{0}' может быть медленным из-за отсутствия индексов и оптимизации функций.",
		recommendation:
			"Перепишите функцию так, чтобы она была IMMUTABLE/STABLE, либо материализуйте данные в таблице.",
	},
	{
		rule: "MaterializeNode",
		description:
			"Узел Materialize создаёт временные копии данных; если они не нужны повторно, это приводит к избыточным затратам памяти.",
		recommendation: "Удалите избыточный Materialize или замените его на временные таблицы/CTE.",
	},
	{
		rule: "UnexpectedParallelism",
		description:
			"Узел '{0}' исполняется параллельно (loops={1}), хотя не предназначен для этого. Возможны накладные расходы без выигрыша в скорости.",
		recommendation:
			"Проверьте parallel_setup_cost/parallel_tuple_cost и убедитесь, что узлы действительно parallel-safe.",
	},
	{
		rule: "CardinalityMismatch",
		description: "Кардинальное несоответствие: оценка планировщика сильно отличается от фактического числа строк.",
		recommendation:
			"Обновите статистику (ANALYZE), настройте default_statistics_target или создайте многоколонные статистики.",
	},
	{
		rule: "BitmapHeapOverfetch",
		description:
			"Bitmap Heap Scan по таблице '{0}' возвращает гораздо больше строк, чем предполагалось Bitmap Index Scan — возможна низкая селективность индекса.",
		recommendation:
			"Создайте более селективный индекс или добавьте INCLUDE колонки, чтобы сократить лишние чтения.",
	},
	{
		rule: "HashSpillBatches",
		description:
			"Hash оператор использует несколько батчей (Batches > 1), что указывает на нехватку памяти и запись промежуточных данных на диск.",
		recommendation: "Увеличьте work_mem или уменьшите объём данных во входном потоке.",
	},
	{
		rule: "HashSpillDisk",
		description: "Hash оператор выполняет запись данных на диск из-за переполнения памяти.",
		recommendation: "Поднимите work_mem или используйте Merge Join вместо Hash Join.",
	},
	{
		rule: "HashSpillTempFiles",
		description: "Hash оператор/Hash Join пишет временные файлы на диск (TempWritten) — явный индикатор спиллов.",
		recommendation: "Повысьте work_mem, уменьшите количество обрабатываемых строк, либо используйте предагрегацию.",
	},
	{
		rule: "IndexFilterMismatch",
		description:
			"Index Scan/Bitmap Heap Scan использует фильтр, который не покрывается Index Cond — часть работы выполняется без индекса.",
		recommendation: "Создайте индекс с выражением или добавьте недостающие колонки в INCLUDE.",
	},
	{
		rule: "IndexOnlyHeapFetch",
		description: "Index Only Scan требует heap fetch — видимость строк в visibility map неполная.",
		recommendation:
			"Выполните VACUUM ANALYZE; рассмотрите CLUSTER или BRIN-индексы для улучшения покрытия visibility map.",
	},
	{
		rule: "NestedLoopHeavyInner",
		description:
			"Nested Loop выполняет дорогостоящую внутреннюю операцию многократно, что сильно замедляет запрос.",
		recommendation: "Создайте индекс на колонках join-ключа или замените Nested Loop на Hash/Merge Join.",
	},
	{
		rule: "Parallelism",
		description:
			"Запланированы параллельные воркеры, но они не запущены. Возможные причины: функции не parallel-safe, настройки сервера или параметры запроса.",
		recommendation:
			"Проверьте настройки max_parallel_workers, max_parallel_workers_per_gather и parallel_setup_cost.",
	},
	{
		rule: "SeqScanFractionRemoved",
		description:
			"Seq Scan по таблице '{0}' отбрасывает большую часть строк ({1}), что указывает на неэффективное чтение лишних данных.",
		recommendation: "Создайте частичный индекс по условию WHERE или рассмотрите денормализацию/предагрегацию.",
	},
	{
		rule: "SeqScanIOHeave",
		description: "Seq Scan по таблице '{0}' выполняет большое количество операций ввода-вывода (IO).",
		recommendation: "Создайте индексы по фильтрам и пересмотрите JOIN/WHERE.",
	},
	{
		rule: "SortExternal",
		description: "Сортировка выполняется с использованием внешней памяти — вероятен spill на диск.",
		recommendation: "Увеличьте work_mem или создайте индекс по колонкам ORDER BY.",
	},
	{
		rule: "SortExternalTempFile",
		description: "Сортировка использует диск (Sort Space Type=Disk) — данные не помещаются в память.",
		recommendation: "Увеличьте work_mem; оптимизируйте сортируемый набор (LIMIT, предфильтрация).",
	},
	{
		rule: "SortExternalTempWritten",
		description: "Сортировка записывает временные блоки на диск (TempWritten > 0).",
		recommendation: "Оптимизируйте ORDER BY, создайте индекс или повысьте work_mem.",
	},
	{
		rule: "TempFileSortSpill",
		description: "Узел создаёт временные файлы для сортировок/хешей — это спиллы на диск.",
		recommendation: "Поднимите work_mem и уменьшите размер сортировок/хешей.",
	},
	{
		rule: "HighBufferReads",
		description: "Узел '{0}' по таблице '{1}' выполняет чрезмерное количество чтений буферов ({2} блоков).",
		recommendation: "Создайте индексы и фильтры для уменьшения числа читаемых блоков.",
	},
	{
		rule: "LargeNumberOfLoops",
		description:
			"Узел '{0}' по таблице '{1}' выполняется {2} раз (loops) — возможна неэффективная стратегия соединений.",
		recommendation: "Избегайте многократных Nested Loop — используйте Hash/Merge Join или материализацию.",
	},
	{
		rule: "RepeatedSeqScan",
		description:
			"Несколько Seq Scan повторяются для таблиц: {0}. Это может быть результатом неудачной декомпозиции запроса.",
		recommendation: "Оптимизируйте запрос с помощью CTE/подзапросов или кэширования промежуточных результатов.",
	},
	{
		rule: "IndexOnlyScanButBitmap",
		description:
			"Index Only Scan по таблице '{0}' вынужденно использует Bitmap Heap Scan, что говорит о нехватке покрывающего индекса.",
		recommendation: "Добавьте недостающие колонки в индекс, чтобы сделать его полностью покрывающим.",
	},
	{
		rule: "HashJoinWithSkew",
		description: "Hash Join испытывает дисбаланс данных (ratio {0}x), что снижает эффективность распараллеливания.",
		recommendation:
			"Используйте parallel hash join, перераспределение данных или настройку random_page_cost для снижения дисбаланса.",
	},
	{
		rule: "ParallelSeqScanIneffective",
		description: "Parallel Seq Scan по таблице '{0}' оказался неэффективным (ActualLoops={1}).",
		recommendation: "Снизьте parallel_setup_cost или настройте parallel_workers для адекватного распараллеливания.",
	},
	{
		rule: "SortSpillToDisk",
		description: "Sort узел пишет данные во временные файлы (spill) — TempWritten={0}.",
		recommendation: "Создайте индекс под ORDER BY или увеличьте work_mem.",
	},
	{
		rule: "ExcessiveTempFiles",
		description: "Узел '{0}' создает чрезмерное количество временных файлов (TempRead+TempWritten={1}).",
		recommendation: "Оптимизируйте запрос или увеличьте work_mem, чтобы сократить создание временных файлов.",
	},
	{
		rule: "FunctionInWherePerformance",
		description: "Функция в WHERE для таблицы '{0}' делает условие неиндексируемым.",
		recommendation: "Перепишите условие так, чтобы использовать индекс (например, вычисляемое поле).",
	},
	{
		rule: "LeadingWildcardLike",
		description: "Условие LIKE с ведущим '%' по таблице '{0}' исключает использование индексов.",
		recommendation: "Используйте trigram-индексы (pg_trgm) или полнотекстовый поиск вместо LIKE с ведущим '%'.",
	},
	{
		rule: "MissingStatistics",
		description: "Таблица '{0}' имеет неточные статистики: планировщик неверно оценивает число строк.",
		recommendation: "Выполните ANALYZE и настройте default_statistics_target для улучшения оценок.",
	},
	{
		rule: "CorrelatedSubqueryExec",
		description: "Коррелированный подзапрос выполняется {0} раз, что приводит к многократным обращениям к данным.",
		recommendation: "Перепишите подзапрос на JOIN или CTE для устранения множественных вызовов.",
	},
	{
		rule: "SlowStartupTime",
		description: "Узел '{0}' имеет длительное стартовое время ({1} мс), что замедляет выдачу первых результатов.",
		recommendation:
			"Оптимизируйте план — используйте индексы или материализованные представления для ускорения старта.",
	},
	{
		rule: "ActualVsEstimatedLargeDiff",
		description: "Большое расхождение между фактическим и оценочным числом строк (Actual/Plan={0}).",
		recommendation: "Обновите статистику и проверьте селективность предикатов.",
	},
	{
		rule: "FilterAfterAggregate",
		description: "Фильтрация выполняется после агрегирования на узле '{0}', что увеличивает нагрузку.",
		recommendation: "Перенесите фильтр в WHERE/HAVING до агрегирования.",
	},
	{
		rule: "WorkMemExceededEstimate",
		description: "Узел '{0}' пишет временные файлы ({1}), что говорит о превышении work_mem.",
		recommendation: "Увеличьте work_mem или перепишите запрос для снижения объема промежуточных данных.",
	},
	{
		rule: "LargeAggregateMemory",
		description: "Агрегатный узел '{0}' потребляет много памяти ({1} MB).",
		recommendation: "Сократите набор входных строк перед агрегацией или увеличьте work_mem.",
	},
	{
		rule: "SortMethodExternal",
		description: "Sort узел '{0}' использует внешний метод сортировки (Sort Method={1}), что указывает на spill.",
		recommendation: "Увеличьте work_mem или уменьшите сортируемый набор (LIMIT, предагрегация).",
	},
	{
		rule: "BitmapIndexScanOnSmallTable",
		description: "Bitmap Index Scan по маленькой таблице '{0}' (PlanRows={1}) может быть избыточным.",
		recommendation: "Используйте Seq Scan вместо Bitmap Index Scan — он будет быстрее на малых таблицах.",
	},
	{
		rule: "IndexScanWithFilterOnNonIndexedCol",
		description: "Index Scan использует фильтр '{0}', не покрытый индексом.",
		recommendation: "Добавьте индекс по фильтруемой колонке.",
	},
	{
		rule: "SeqScanOnRecentlyUpdatedTable",
		description:
			"Seq Scan по недавно обновлённой таблице '{0}' (updates={1}) может игнорировать индексы из-за неактуальных статистик.",
		recommendation: "Выполните VACUUM ANALYZE или создайте индекс по актуальным фильтрам.",
	},
	{
		rule: "SeqScanWithHighTempWrites",
		description: "Seq Scan по таблице '{0}' создаёт большое количество временных файлов ({1}).",
		recommendation: "Увеличьте work_mem и оптимизируйте фильтры для снижения записи во временные файлы.",
	},
	{
		rule: "IndexScanButBitmapRecheck",
		description:
			"Index Scan по таблице '{0}' требует Bitmap Heap Recheck — возможно, индекс не полностью оптимален.",
		recommendation: "Создайте более точный индекс, чтобы исключить recheck.",
	},
	{
		rule: "ParallelWorkersTooMany",
		description:
			"Узел '{0}' запускает слишком много воркеров (Launched={1}, Planned={2}), что приводит к накладным расходам.",
		recommendation: "Снизьте количество параллельных воркеров — настройте max_parallel_workers_per_gather.",
	},
	{
		rule: "HashAggWithoutHashableKey",
		description: "Hash Aggregate узел '{0}' использует ключ '{1}', неподходящий для хэширования.",
		recommendation: "Используйте Sort Aggregate или преобразуйте ключ в хэшируемый тип.",
	},
	{
		rule: "CrossProductDetected",
		description: "Nested Loop без условий соединения на узле '{0}' — это кросс-произведение, крайне неэффективное.",
		recommendation: "Добавьте условия соединения (ON/USING), чтобы избежать кросс-произведения.",
	},
];
