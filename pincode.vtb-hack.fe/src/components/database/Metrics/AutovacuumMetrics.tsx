import type { AutovacuumMetricsSummary } from "@/generated/models/AutovacuumMetricsSummary";
import { formatNumber } from "../utils/format";

interface AutovacuumMetricsProps {
	metrics: AutovacuumMetricsSummary;
}

/**
 * Компонент для отображения метрик Autovacuum
 */
export function AutovacuumMetrics({ metrics }: AutovacuumMetricsProps) {
	return (
		<div className="grid grid-cols-1 md:grid-cols-3 gap-4 p-4 bg-muted/30 rounded-lg">
			<div className="space-y-2">
				<span className="metric-label">Общая статистика</span>
				<div className="space-y-1">
					<div className="flex justify-between">
						<span className="text-sm">Всего таблиц:</span>
						<span className="metric-value text-sm">{formatNumber(metrics.totalTables)}</span>
					</div>
					<div className="flex justify-between">
						<span className="text-sm">Проблемных таблиц:</span>
						<span className="metric-value text-sm text-yellow-600">
							{formatNumber(metrics.problematicTables)}
						</span>
					</div>
					<div className="flex justify-between">
						<span className="text-sm">Критичных таблиц:</span>
						<span className="metric-value text-sm text-red-600">
							{formatNumber(metrics.criticalTables)}
						</span>
					</div>
				</div>
			</div>

			<div className="space-y-2">
				<span className="metric-label">Мертвые строки</span>
				<div className="space-y-1">
					<div className="flex justify-between">
						<span className="text-sm">Средний процент:</span>
						<span className="metric-value text-sm">{metrics.avgDeadTupleRatio?.toFixed(2)}%</span>
					</div>
					<div className="flex justify-between">
						<span className="text-sm">Максимальный процент:</span>
						<span className="metric-value text-sm text-red-600">
							{metrics.maxDeadTupleRatio?.toFixed(2)}%
						</span>
					</div>
					<div className="flex justify-between">
						<span className="text-sm">Худшая таблица:</span>
						<span className="metric-value text-sm text-red-600 truncate" title={metrics.worstTable || ""}>
							{metrics.worstTable || "N/A"}
						</span>
					</div>
				</div>
			</div>

			<div className="space-y-2">
				<span className="metric-label">Распределение по уровням</span>
				<div className="space-y-1">
					<div className="flex justify-between">
						<span className="text-sm">&gt; 10%:</span>
						<span className="metric-value text-sm">{formatNumber(metrics.tablesAbove10Percent)}</span>
					</div>
					<div className="flex justify-between">
						<span className="text-sm">&gt; 20%:</span>
						<span className="metric-value text-sm text-yellow-600">
							{formatNumber(metrics.tablesAbove20Percent)}
						</span>
					</div>
					<div className="flex justify-between">
						<span className="text-sm">&gt; 30%:</span>
						<span className="metric-value text-sm text-red-600">
							{formatNumber(metrics.tablesAbove30Percent)}
						</span>
					</div>
				</div>
			</div>
		</div>
	);
}
