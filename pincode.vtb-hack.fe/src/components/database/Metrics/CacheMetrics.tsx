import type { CacheMetricsSummary } from "@/generated/models/CacheMetricsSummary";
import { formatNumber } from "../utils/format";
import { Card, CardContent } from "@pin-code/ui-kit";
import { Target, Activity, Clock, BarChart3, Database, Timer } from "lucide-react";

interface CacheMetricsProps {
	metrics: CacheMetricsSummary;
}

/**
 * Компонент для отображения метрик кэша
 */
export function CacheMetrics({ metrics }: CacheMetricsProps) {
	return (
		<div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
			{/* Средний процент попаданий */}
			<Card className="border-l-4 border-l-green-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<Target className="h-4 w-4 text-green-600 dark:text-green-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Средний Hit Ratio
						</p>
					</div>
					<p className="text-2xl font-bold status-healthy">{metrics.avgCacheHitRatio?.toFixed(1)}%</p>
					<p className="text-xs text-muted-foreground">попаданий в кэш</p>
				</CardContent>
			</Card>

			{/* Минимальный процент */}
			<Card className="border-l-4 border-l-blue-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<BarChart3 className="h-4 w-4 text-blue-600 dark:text-blue-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">Минимальный</p>
					</div>
					<p className="text-2xl font-bold">{metrics.minCacheHitRatio?.toFixed(1)}%</p>
					<p className="text-xs text-muted-foreground">hit ratio</p>
				</CardContent>
			</Card>

			{/* Максимальный процент */}
			<Card className="border-l-4 border-l-cyan-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<BarChart3 className="h-4 w-4 text-cyan-600 dark:text-cyan-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Максимальный
						</p>
					</div>
					<p className="text-2xl font-bold">{metrics.maxCacheHitRatio?.toFixed(1)}%</p>
					<p className="text-xs text-muted-foreground">hit ratio</p>
				</CardContent>
			</Card>

			{/* Попадания в минуту */}
			<Card className="border-l-4 border-l-emerald-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<Activity className="h-4 w-4 text-emerald-600 dark:text-emerald-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Попадания/мин
						</p>
					</div>
					<p className="text-2xl font-bold">{formatNumber(metrics.blksHitPerMinute)}</p>
					<p className="text-xs text-muted-foreground">блоков в кэше</p>
				</CardContent>
			</Card>

			{/* Чтения с диска в минуту */}
			<Card className="border-l-4 border-l-orange-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<Database className="h-4 w-4 text-orange-600 dark:text-orange-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">Чтения/мин</p>
					</div>
					<p className="text-2xl font-bold">{formatNumber(metrics.blksReadPerMinute)}</p>
					<p className="text-xs text-muted-foreground">блоков с диска</p>
				</CardContent>
			</Card>

			{/* Общие обращения в минуту */}
			<Card className="border-l-4 border-l-purple-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<Clock className="h-4 w-4 text-purple-600 dark:text-purple-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Обращений/мин
						</p>
					</div>
					<p className="text-2xl font-bold">{formatNumber(metrics.blksAccessedPerMinute)}</p>
					<p className="text-xs text-muted-foreground">всего к блокам</p>
				</CardContent>
			</Card>

			{/* Общие попадания */}
			<Card className="border-l-4 border-l-teal-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<Target className="h-4 w-4 text-teal-600 dark:text-teal-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Всего попаданий
						</p>
					</div>
					<p className="text-2xl font-bold">{formatNumber(metrics.totalBlksHit)}</p>
					<p className="text-xs text-muted-foreground">блоков в кэше</p>
				</CardContent>
			</Card>

			{/* Общие чтения */}
			<Card className="border-l-4 border-l-amber-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<Database className="h-4 w-4 text-amber-600 dark:text-amber-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Всего чтений
						</p>
					</div>
					<p className="text-2xl font-bold">{formatNumber(metrics.totalBlksRead)}</p>
					<p className="text-xs text-muted-foreground">блоков с диска</p>
				</CardContent>
			</Card>

			{/* Точки данных */}
			<Card className="border-l-4 border-l-indigo-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<Timer className="h-4 w-4 text-indigo-600 dark:text-indigo-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Точки данных
						</p>
					</div>
					<p className="text-2xl font-bold">{formatNumber(metrics.dataPointsCount)}</p>
					<p className="text-xs text-muted-foreground">для анализа</p>
				</CardContent>
			</Card>
		</div>
	);
}
