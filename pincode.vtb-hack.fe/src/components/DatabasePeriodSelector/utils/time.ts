/**
 * Утилиты для работы с временными метками
 */

export interface GroupedTimeMetric {
	/** Представительное время группы (обычно самое раннее) */
	time: Date;
	/** Все временные метки в группе */
	originalTimes: string[];
	/** Тип метрики */
	metricType: MetricType;
}

export type MetricType = "autovacuum" | "cache" | "index" | "tempfiles" | "locks";

export interface MetricConfig {
	type: MetricType;
	color: string;
	label: string;
}

export const METRIC_CONFIGS: Record<MetricType, MetricConfig> = {
	autovacuum: {
		type: "autovacuum",
		color: "#3b82f6", // blue
		label: "Автовакуум",
	},
	cache: {
		type: "cache",
		color: "#10b981", // emerald
		label: "Кэш",
	},
	index: {
		type: "index",
		color: "#f59e0b", // amber
		label: "Индексы",
	},
	tempfiles: {
		type: "tempfiles",
		color: "#ef4444", // red
		label: "Временные файлы",
	},
	locks: {
		type: "locks",
		color: "#8b5cf6", // violet
		label: "Блокировки",
	},
};

/**
 * Группирует временные метки, которые отличаются менее чем на заданный интервал
 * @param timestamps - массив ISO строк времени
 * @param metricType - тип метрики
 * @param thresholdMs - порог в миллисекундах для группировки (по умолчанию 5 секунд)
 */
export function groupNearbyTimestamps(
	timestamps: string[],
	metricType: MetricType,
	thresholdMs: number = 5000,
): GroupedTimeMetric[] {
	if (timestamps.length === 0) return [];

	// Преобразуем в Date и сортируем
	const sortedDates = timestamps
		.map((t) => ({
			original: t,
			date: new Date(t),
		}))
		.sort((a, b) => a.date.getTime() - b.date.getTime());

	const groups: GroupedTimeMetric[] = [];
	let currentGroup: { original: string; date: Date }[] = [sortedDates[0]];

	for (let i = 1; i < sortedDates.length; i++) {
		const current = sortedDates[i];
		const lastInGroup = currentGroup[currentGroup.length - 1];

		// Если текущая метка отличается от последней в группе более чем на пороговое значение
		if (current.date.getTime() - lastInGroup.date.getTime() > thresholdMs) {
			// Завершаем текущую группу
			groups.push({
				time: currentGroup[0].date, // Используем самое раннее время как представительное
				originalTimes: currentGroup.map((item) => item.original),
				metricType,
			});

			// Начинаем новую группу
			currentGroup = [current];
		} else {
			// Добавляем в текущую группу
			currentGroup.push(current);
		}
	}

	// Не забываем последнюю группу
	if (currentGroup.length > 0) {
		groups.push({
			time: currentGroup[0].date,
			originalTimes: currentGroup.map((item) => item.original),
			metricType,
		});
	}

	return groups;
}

/**
 * Объединяет метки от разных источников в один упорядоченный массив
 */
export function combineMetricTimestamps(metrics: {
	autovacuum?: string[];
	cache?: string[];
	index?: string[];
	tempfiles?: string[];
	locks?: string[];
}): GroupedTimeMetric[] {
	const allGrouped: GroupedTimeMetric[] = [];

	// Группируем каждый тип метрик отдельно
	Object.entries(metrics).forEach(([key, timestamps]) => {
		if (timestamps && timestamps.length > 0) {
			const grouped = groupNearbyTimestamps(timestamps, key as MetricType);
			allGrouped.push(...grouped);
		}
	});

	// Сортируем по времени
	return allGrouped.sort((a, b) => a.time.getTime() - b.time.getTime());
}

/**
 * Определяет диапазон времени для отображения на таймлайне
 */
export function calculateTimelineRange(
	groupedMetrics: GroupedTimeMetric[],
	customRange?: { start: Date; end: Date },
): { start: Date; end: Date } {
	if (groupedMetrics.length === 0) {
		const now = new Date();
		return {
			start: new Date(now.getTime() - 24 * 60 * 60 * 1000), // 24 часа назад
			end: now,
		};
	}

	const times = groupedMetrics.map((m) => m.time);
	const earliest = new Date(Math.min(...times.map((t) => t.getTime())));
	const latest = new Date(Math.max(...times.map((t) => t.getTime())));

	// Если есть кастомный диапазон, расширяем его, чтобы включить все метрики
	if (customRange) {
		const expandedStart = new Date(Math.min(earliest.getTime(), customRange.start.getTime()));
		const expandedEnd = new Date(Math.max(latest.getTime(), customRange.end.getTime()));

		// Добавляем небольшой отступ
		const totalDuration = expandedEnd.getTime() - expandedStart.getTime();
		const padding = Math.max(totalDuration * 0.1, 60 * 60 * 1000); // Минимум 1 час отступа

		return {
			start: new Date(expandedStart.getTime() - padding),
			end: new Date(expandedEnd.getTime() + padding),
		};
	}

	// Добавляем отступ по краям для лучшего отображения
	const duration = latest.getTime() - earliest.getTime();
	const padding = Math.max(duration * 0.2, 30 * 60 * 1000); // Минимум 30 минут отступа

	return {
		start: new Date(earliest.getTime() - padding),
		end: new Date(latest.getTime() + padding),
	};
}

/**
 * Пресеты для быстрого выбора периода
 */
export interface TimePreset {
	label: string;
	getValue: () => { start: Date; end: Date };
}

export const TIME_PRESETS: TimePreset[] = [
	{
		label: "1h",
		getValue: () => {
			const now = new Date();
			return {
				start: new Date(now.getTime() - 60 * 60 * 1000), // 1 час назад
				end: now,
			};
		},
	},
	{
		label: "6h",
		getValue: () => {
			const now = new Date();
			return {
				start: new Date(now.getTime() - 60 * 60 * 1000 * 6), // 6 час назад
				end: now,
			};
		},
	},
	{
		label: "1d",
		getValue: () => {
			const now = new Date();
			return {
				start: new Date(now.getTime() - 24 * 60 * 60 * 1000), // 1 день назад
				end: now,
			};
		},
	},
	{
		label: "1w",
		getValue: () => {
			const now = new Date();
			return {
				start: new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000), // 1 неделя назад
				end: now,
			};
		},
	},
	{
		label: "1m",
		getValue: () => {
			const now = new Date();
			return {
				start: new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000), // 1 месяц назад
				end: now,
			};
		},
	},
];

/**
 * Форматирует дату для отображения
 */
export function formatDateRange(start: Date, end: Date): string {
	const options: Intl.DateTimeFormatOptions = {
		year: "numeric",
		month: "short",
		day: "numeric",
		hour: "2-digit",
		minute: "2-digit",
	};

	const startStr = start.toLocaleDateString("ru-RU", options);
	const endStr = end.toLocaleDateString("ru-RU", options);

	return `${startStr} — ${endStr}`;
}

/**
 * Форматирует время для меток на таймлайне в формате "01.01.01, 12:33"
 */
export function formatTimelineLabel(date: Date): string {
	const day = String(date.getDate()).padStart(2, "0");
	const month = String(date.getMonth() + 1).padStart(2, "0");
	const year = String(date.getFullYear()).slice(-2); // Последние 2 цифры года
	const hours = String(date.getHours()).padStart(2, "0");
	const minutes = String(date.getMinutes()).padStart(2, "0");

	return `${day}.${month}.${year}, ${hours}:${minutes}`;
}
