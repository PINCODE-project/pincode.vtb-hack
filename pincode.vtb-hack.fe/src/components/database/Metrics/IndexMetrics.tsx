import type { IndexUsageStatistics } from "@/generated/models/IndexUsageStatistics";
import { formatNumber } from "../utils/format";

interface IndexMetricsProps {
	metrics: IndexUsageStatistics;
}

/**
 * Компонент для отображения статистики индексов
 */
export function IndexMetrics({ metrics }: IndexMetricsProps) {
	return (
		<div className="grid grid-cols-1 md:grid-cols-2 gap-4 p-4 bg-muted/30 rounded-lg">
			<div className="space-y-2">
				<span className="metric-label">Общая статистика</span>
				<div className="space-y-1">
					<div className="flex justify-between">
						<span className="text-sm">Всего индексов:</span>
						<span className="metric-value text-sm">{formatNumber(metrics.totalIndexes)}</span>
					</div>
					<div className="flex justify-between">
						<span className="text-sm">Всего сканирований:</span>
						<span className="metric-value text-sm">{formatNumber(metrics.totalScans)}</span>
					</div>
					<div className="flex justify-between">
						<span className="text-sm">Средняя эффективность:</span>
						<span className="metric-value text-sm text-green-600">
							{metrics.averageEfficiency?.toFixed(2)}%
						</span>
					</div>
				</div>
			</div>

			{metrics.indexesByEfficiency && (
				<div className="space-y-2">
					<span className="metric-label">Распределение по эффективности</span>
					<div className="space-y-1">
						{Object.entries(metrics.indexesByEfficiency).map(([level, count]) => (
							<div key={level} className="flex justify-between">
								<span className="text-sm">{level}:</span>
								<span className="metric-value text-sm">{count}</span>
							</div>
						))}
					</div>
				</div>
			)}
		</div>
	);
}
