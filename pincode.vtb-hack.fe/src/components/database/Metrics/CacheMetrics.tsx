import type { CacheMetricsSummary } from "@/generated/models/CacheMetricsSummary";
import { formatNumber } from "../utils/format";

interface CacheMetricsProps {
	metrics: CacheMetricsSummary;
}

/**
 * Компонент для отображения метрик кэша
 */
export function CacheMetrics({ metrics }: CacheMetricsProps) {
	return (
		<div className="grid grid-cols-1 md:grid-cols-3 gap-4 p-4 bg-muted/30 rounded-lg">
			<div className="space-y-2">
				<span className="metric-label">Статистика попаданий</span>
				<div className="space-y-1">
					<div className="flex justify-between">
						<span className="text-sm">Средний hit ratio:</span>
						<span className="metric-value text-sm text-green-600">
							{metrics.avgCacheHitRatio?.toFixed(2)}%
						</span>
					</div>
					<div className="flex justify-between">
						<span className="text-sm">Минимальный:</span>
						<span className="metric-value text-sm">{metrics.minCacheHitRatio?.toFixed(2)}%</span>
					</div>
					<div className="flex justify-between">
						<span className="text-sm">Максимальный:</span>
						<span className="metric-value text-sm">{metrics.maxCacheHitRatio?.toFixed(2)}%</span>
					</div>
				</div>
			</div>

			<div className="space-y-2">
				<span className="metric-label">Активность в минуту</span>
				<div className="space-y-1">
					<div className="flex justify-between">
						<span className="text-sm">Hits:</span>
						<span className="metric-value text-sm">{formatNumber(metrics.blksHitPerMinute)}</span>
					</div>
					<div className="flex justify-between">
						<span className="text-sm">Reads:</span>
						<span className="metric-value text-sm">{formatNumber(metrics.blksReadPerMinute)}</span>
					</div>
					<div className="flex justify-between">
						<span className="text-sm">Total:</span>
						<span className="metric-value text-sm">{formatNumber(metrics.blksAccessedPerMinute)}</span>
					</div>
				</div>
			</div>

			<div className="space-y-2">
				<span className="metric-label">Общая статистика</span>
				<div className="space-y-1">
					<div className="flex justify-between">
						<span className="text-sm">Всего hits:</span>
						<span className="metric-value text-sm">{formatNumber(metrics.totalBlksHit)}</span>
					</div>
					<div className="flex justify-between">
						<span className="text-sm">Всего reads:</span>
						<span className="metric-value text-sm">{formatNumber(metrics.totalBlksRead)}</span>
					</div>
					<div className="flex justify-between">
						<span className="text-sm">Точек данных:</span>
						<span className="metric-value text-sm">{formatNumber(metrics.dataPointsCount)}</span>
					</div>
				</div>
			</div>
		</div>
	);
}
