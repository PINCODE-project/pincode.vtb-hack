import type { MetricSection } from "./types";

/**
 * Данные метрик Cache для анализа настроек БД
 */
export const cacheMetrics: MetricSection = {
	title: "Метрики Cache",
	items: [
		{
			title: "Критическая нехватка памяти",
			metric: "Комбинация AvgCacheHitRatio и BlksReadPerMinute",
			threshold: "Cache Hit Ratio < 85% И Disk Reads > 2000 чтений/мин",
			level: "critical",
			description: "Критическая ситуация с крайне низким процентом попаданий в кэш и высокой дисковой нагрузкой.",
			recommendations: ["Немедленно увеличить размер shared_buffers и провести анализ рабочих нагрузок."],
		},
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
			title: "Отличная производительность кэша",
			metric: "Комбинация AvgCacheHitRatio и BlksReadPerMinute",
			threshold: "Cache Hit Ratio ≥ 90% И Disk Reads < 100 чтений/мин",
			level: "low",
			description: "Кэш работает оптимально с высоким процентом попаданий и минимальной дисковой активностью.",
			recommendations: ["Текущая конфигурация эффективна, поддерживать текущие настройки."],
		},
	],
};
