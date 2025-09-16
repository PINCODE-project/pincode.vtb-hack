import type { AutovacuumMetricsSummary } from "@/generated/models/AutovacuumMetricsSummary";
import { formatNumber } from "../utils/format";
import { Card, CardContent } from "@pin-code/ui-kit";
import { Database, AlertTriangle, AlertCircle, TrendingUp, Activity, BarChart3, Gauge } from "lucide-react";

interface AutovacuumMetricsProps {
	metrics: AutovacuumMetricsSummary;
}

/**
 * Компонент для отображения метрик Autovacuum
 */
export function AutovacuumMetrics({ metrics }: AutovacuumMetricsProps) {
	return (
		<div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
			{/* Общее количество таблиц */}
			<Card className="border-l-4 border-l-blue-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<Database className="h-4 w-4 text-blue-600 dark:text-blue-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Всего таблиц
						</p>
					</div>
					<p className="text-2xl font-bold">{formatNumber(metrics.totalTables)}</p>
				</CardContent>
			</Card>

			{/* Проблемные таблицы */}
			<Card className="border-l-4 border-l-yellow-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<AlertTriangle className="h-4 w-4 text-yellow-600 dark:text-yellow-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Требуют внимания
						</p>
					</div>
					<p className="text-2xl font-bold status-warning">{formatNumber(metrics.problematicTables)}</p>
					<p className="text-xs text-muted-foreground">мертвых строк &gt; 10%</p>
				</CardContent>
			</Card>

			{/* Критические таблицы */}
			<Card className="border-l-4 border-l-red-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<AlertCircle className="h-4 w-4 text-red-600 dark:text-red-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Критическое состояние
						</p>
					</div>
					<p className="text-2xl font-bold status-critical">{formatNumber(metrics.criticalTables)}</p>
					<p className="text-xs text-muted-foreground">мертвых строк &gt; 20%</p>
				</CardContent>
			</Card>

			{/* Средний процент мертвых строк */}
			<Card className="border-l-4 border-l-purple-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<TrendingUp className="h-4 w-4 text-purple-600 dark:text-purple-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Средний процент
						</p>
					</div>
					<p className="text-2xl font-bold">{metrics.avgDeadTupleRatio?.toFixed(1)}%</p>
					<p className="text-xs text-muted-foreground">мертвых строк</p>
				</CardContent>
			</Card>

			{/* Максимальный процент */}
			<Card className="border-l-4 border-l-orange-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<TrendingUp className="h-4 w-4 text-orange-600 dark:text-orange-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Максимальный процент
						</p>
					</div>
					<p className="text-2xl font-bold status-critical">{metrics.maxDeadTupleRatio?.toFixed(1)}%</p>
					<p className="text-xs text-muted-foreground">мертвых строк</p>
				</CardContent>
			</Card>

			{/* Худшая таблица - растянута на 2 колонки */}
			<Card className="border-l-4 border-l-red-500 md:col-span-2 lg:col-span-1 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<Database className="h-4 w-4 text-red-600 dark:text-red-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Худшая таблица
						</p>
					</div>
					<p className="text-lg font-bold status-critical break-all" title={metrics.worstTable || ""}>
						{metrics.worstTable || "N/A"}
					</p>
					<p className="text-xs text-muted-foreground">
						{metrics.worstTableRatio?.toFixed(1)}% мертвых строк
					</p>
				</CardContent>
			</Card>

			{/* Общий системный процент мертвых строк */}
			<Card className="border-l-4 border-l-indigo-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<Gauge className="h-4 w-4 text-indigo-600 dark:text-indigo-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Системный процент
						</p>
					</div>
					<p className="text-2xl font-bold">{metrics.systemWideDeadTupleRatio?.toFixed(1)}%</p>
					<p className="text-xs text-muted-foreground">мертвых строк в кластере</p>
				</CardContent>
			</Card>

			{/* Общие живые строки */}
			<Card className="border-l-4 border-l-green-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<Activity className="h-4 w-4 text-green-600 dark:text-green-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Живые строки
						</p>
					</div>
					<p className="text-2xl font-bold">{formatNumber(metrics.totalLiveTuples)}</p>
					<p className="text-xs text-muted-foreground">всего в базе</p>
				</CardContent>
			</Card>

			{/* Общие мертвые строки */}
			<Card className="border-l-4 border-l-gray-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<Activity className="h-4 w-4 text-gray-600 dark:text-gray-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Мертвые строки
						</p>
					</div>
					<p className="text-2xl font-bold status-warning">{formatNumber(metrics.totalDeadTuples)}</p>
					<p className="text-xs text-muted-foreground">всего в базе</p>
				</CardContent>
			</Card>

			{/* Таблицы > 10% */}
			<Card className="border-l-4 border-l-yellow-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<BarChart3 className="h-4 w-4 text-yellow-600 dark:text-yellow-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Таблицы &gt; 10%
						</p>
					</div>
					<p className="text-2xl font-bold status-warning">{formatNumber(metrics.tablesAbove10Percent)}</p>
					<p className="text-xs text-muted-foreground">мертвых строк</p>
				</CardContent>
			</Card>

			{/* Таблицы > 20% */}
			<Card className="border-l-4 border-l-orange-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<BarChart3 className="h-4 w-4 text-orange-600 dark:text-orange-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Таблицы &gt; 20%
						</p>
					</div>
					<p className="text-2xl font-bold status-critical">{formatNumber(metrics.tablesAbove20Percent)}</p>
					<p className="text-xs text-muted-foreground">мертвых строк</p>
				</CardContent>
			</Card>

			{/* Таблицы > 30% */}
			<Card className="border-l-4 border-l-red-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<BarChart3 className="h-4 w-4 text-red-600 dark:text-red-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Таблицы &gt; 30%
						</p>
					</div>
					<p className="text-2xl font-bold status-critical">{formatNumber(metrics.tablesAbove30Percent)}</p>
					<p className="text-xs text-muted-foreground">мертвых строк</p>
				</CardContent>
			</Card>

			{/* Средняя скорость изменений */}
			<Card className="border-l-4 border-l-cyan-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<TrendingUp className="h-4 w-4 text-cyan-600 dark:text-cyan-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Скорость изменений
						</p>
					</div>
					<p className="text-2xl font-bold">{metrics.avgChangeRatePercent?.toFixed(1)}%</p>
					<p className="text-xs text-muted-foreground">средняя по таблицам</p>
				</CardContent>
			</Card>
		</div>
	);
}
