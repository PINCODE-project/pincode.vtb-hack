import type { TempFilesMetricsSummary } from "@/generated/models/TempFilesMetricsSummary";
import { formatNumber, formatBytes } from "../utils/format";

interface TempFilesMetricsProps {
	metrics: TempFilesMetricsSummary;
}

/**
 * Компонент для отображения метрик временных файлов
 */
export function TempFilesMetrics({ metrics }: TempFilesMetricsProps) {
	return (
		<div className="grid grid-cols-1 md:grid-cols-2 gap-4 p-4 bg-muted/30 rounded-lg">
			<div className="space-y-2">
				<span className="metric-label">Общая статистика</span>
				<div className="space-y-1">
					<div className="flex justify-between">
						<span className="text-sm">Всего файлов:</span>
						<span className="metric-value text-sm">{formatNumber(metrics.totalTempFiles)}</span>
					</div>
					<div className="flex justify-between">
						<span className="text-sm">Общий объем:</span>
						<span className="metric-value text-sm">{formatBytes(metrics.totalTempBytes)}</span>
					</div>
				</div>
			</div>

			<div className="space-y-2">
				<span className="metric-label">Интенсивность</span>
				<div className="space-y-1">
					<div className="flex justify-between">
						<span className="text-sm">Файлов/мин:</span>
						<span className="metric-value text-sm">{metrics.tempFilesPerMinute?.toFixed(2)}</span>
					</div>
					<div className="flex justify-between">
						<span className="text-sm">Байт/мин:</span>
						<span className="metric-value text-sm">{formatBytes(metrics.tempBytesPerMinute)}</span>
					</div>
					<div className="flex justify-between">
						<span className="text-sm">Байт/сек:</span>
						<span className="metric-value text-sm">{formatBytes(metrics.tempBytesPerSecond)}</span>
					</div>
				</div>
			</div>
		</div>
	);
}
