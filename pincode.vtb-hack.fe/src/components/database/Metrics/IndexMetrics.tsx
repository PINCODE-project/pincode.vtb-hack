import type { IndexUsageStatistics } from "@/generated/models/IndexUsageStatistics";
import { formatNumber } from "../utils/format";
import { Card, CardContent } from "@pin-code/ui-kit";
import { BarChart3, Activity, TrendingUp, Target } from "lucide-react";

interface IndexMetricsProps {
	metrics: IndexUsageStatistics;
}

/**
 * Компонент для отображения статистики индексов
 */
export function IndexMetrics({ metrics }: IndexMetricsProps) {
	return (
		<div className="space-y-4">
			{/* Основные метрики */}
			<div className="grid grid-cols-1 md:grid-cols-3 gap-3">
				{/* Всего индексов */}
				<Card className="border-l-4 border-l-blue-500 py-3">
					<CardContent className="py-1">
						<div className="flex items-center space-x-2 mb-2">
							<BarChart3 className="h-4 w-4 text-blue-600 dark:text-blue-400" />
							<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
								Всего индексов
							</p>
						</div>
						<p className="text-2xl font-bold">{formatNumber(metrics.totalIndexes)}</p>
						<p className="text-xs text-muted-foreground">в системе</p>
					</CardContent>
				</Card>

				{/* Всего сканирований */}
				<Card className="border-l-4 border-l-green-500 py-3">
					<CardContent className="py-1">
						<div className="flex items-center space-x-2 mb-2">
							<Activity className="h-4 w-4 text-green-600 dark:text-green-400" />
							<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
								Всего сканирований
							</p>
						</div>
						<p className="text-2xl font-bold">{formatNumber(metrics.totalScans)}</p>
						<p className="text-xs text-muted-foreground">за период</p>
					</CardContent>
				</Card>

				{/* Средняя эффективность */}
				<Card className="border-l-4 border-l-emerald-500 py-3">
					<CardContent className="py-1">
						<div className="flex items-center space-x-2 mb-2">
							<Target className="h-4 w-4 text-emerald-600 dark:text-emerald-400" />
							<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
								Эффективность
							</p>
						</div>
						<p className="text-2xl font-bold status-healthy">{metrics.averageEfficiency?.toFixed(1)}%</p>
						<p className="text-xs text-muted-foreground">средняя</p>
					</CardContent>
				</Card>
			</div>

			{/* Распределение по эффективности */}
			{metrics.indexesByEfficiency && (
				<div>
					<h5 className="metric-label mb-3">Распределение по уровням эффективности</h5>
					<div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-2">
						{Object.entries(metrics.indexesByEfficiency).map(([level, count], index) => (
							<Card
								key={level}
								className={`py-3 border-l-4 ${
									index === 0
										? "border-l-green-500"
										: index === 1
											? "border-l-yellow-500"
											: "border-l-red-500"
								}`}
							>
								<CardContent className="py-1">
									<div className="flex items-center space-x-2 mb-2">
										<div
											className={`p-1.5 rounded-md ${
												index === 0
													? "bg-green-100 dark:bg-green-900/20"
													: index === 1
														? "bg-yellow-100 dark:bg-yellow-900/20"
														: "bg-red-100 dark:bg-red-900/20"
											}`}
										>
											<TrendingUp
												className={`h-4 w-4 ${
													index === 0
														? "text-green-600 dark:text-green-400"
														: index === 1
															? "text-yellow-600 dark:text-yellow-400"
															: "text-red-600 dark:text-red-400"
												}`}
											/>
										</div>
										<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
											{level}
										</p>
									</div>
									<div>
										<p className="text-lg font-bold">{count}</p>
									</div>
								</CardContent>
							</Card>
						))}
					</div>
				</div>
			)}
		</div>
	);
}
