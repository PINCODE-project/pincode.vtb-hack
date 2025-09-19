import type { MetricSection } from "./types";

/**
 * Данные метрик Autovacuum для анализа настроек БД
 */
export const autovacuumMetrics: MetricSection = {
	title: "Метрики Autovacuum",
	items: [
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
			recommendations: ["Увеличить количество рабочих процессов autovacuum для параллельной обработки таблиц."],
		},
	],
};
