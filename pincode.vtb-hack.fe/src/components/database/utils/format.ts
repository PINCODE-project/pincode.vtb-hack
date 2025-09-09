/**
 * Утилиты для форматирования данных в компонентах анализа базы данных
 */

/**
 * Функция для форматирования чисел
 */
export const formatNumber = (value: number | undefined): string => {
	if (value === undefined) return "N/A";
	if (value > 1000000) return `${(value / 1000000).toFixed(1)}M`;
	if (value > 1000) return `${(value / 1000).toFixed(1)}K`;
	return value.toFixed(0);
};

/**
 * Функция для форматирования байтов
 */
export const formatBytes = (bytes: number | undefined): string => {
	if (bytes === undefined) return "N/A";
	if (bytes === 0) return "0 Bytes";
	const k = 1024;
	const sizes = ["Bytes", "KB", "MB", "GB", "TB"];
	const i = Math.floor(Math.log(bytes) / Math.log(k));
	return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
};

/**
 * Функция для сортировки рекомендаций по критичности
 */
export const sortRecommendationsBySeverity = <T extends { severity?: string | null }>(
	recommendations: T[] | null | undefined,
): T[] => {
	if (!recommendations) return [];
	const severityOrder = { critical: 0, high: 1, warning: 2, medium: 2, info: 3, low: 3 };
	return [...recommendations].sort((a, b) => {
		const aSeverity = a.severity?.toLowerCase() || "low";
		const bSeverity = b.severity?.toLowerCase() || "low";
		const aOrder = severityOrder[aSeverity as keyof typeof severityOrder] ?? 4;
		const bOrder = severityOrder[bSeverity as keyof typeof severityOrder] ?? 4;
		return aOrder - bOrder;
	});
};
