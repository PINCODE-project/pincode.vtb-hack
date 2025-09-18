import type { LockAnalysisResult } from "@/generated/models/LockAnalysisResult";
import { formatNumber } from "../utils/format";
import { Card, CardContent } from "@pin-code/ui-kit";
import { Lock, AlertTriangle, Users, Database, TrendingUp, Clock, Activity } from "lucide-react";

interface LockMetricsProps {
	metrics: LockAnalysisResult;
}

/**
 * Компонент для отображения метрик блокировок
 */
export function LockMetrics({ metrics }: LockMetricsProps) {
	const getSeverityColor = (value: number, critical: number, warning: number) => {
		if (value >= critical) return "border-l-red-500 text-red-600";
		if (value >= warning) return "border-l-yellow-500 text-yellow-600";
		return "border-l-green-500 text-green-600";
	};

	return (
		<div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
			{/* Общее количество заблокированных lock'ов */}
			<Card className={`border-l-4 py-3 ${getSeverityColor(metrics.totalBlockedLocks || 0, 100, 20)}`}>
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<Lock className="h-4 w-4" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Заблокировано
						</p>
					</div>
					<p className="text-2xl font-bold">{formatNumber(metrics.totalBlockedLocks)}</p>
					<p className="text-xs text-muted-foreground">блокировок</p>
				</CardContent>
			</Card>

			{/* Количество заблокированных процессов */}
			<Card className={`border-l-4 py-3 ${getSeverityColor(metrics.blockedProcesses || 0, 50, 10)}`}>
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<Users className="h-4 w-4" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Процессов заблокировано
						</p>
					</div>
					<p className="text-2xl font-bold">{formatNumber(metrics.blockedProcesses)}</p>
					<p className="text-xs text-muted-foreground">уникальных процессов</p>
				</CardContent>
			</Card>

			{/* Критические проблемы */}
			<Card className="border-l-4 border-l-red-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<AlertTriangle className="h-4 w-4 text-red-600" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">Критические</p>
					</div>
					<p className="text-2xl font-bold text-red-600">
						{formatNumber(metrics.criticalIssues?.length || 0)}
					</p>
					<p className="text-xs text-muted-foreground">требуют вмешательства</p>
				</CardContent>
			</Card>

			{/* Предупреждения */}
			<Card className="border-l-4 border-l-yellow-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<AlertTriangle className="h-4 w-4 text-yellow-600" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Предупреждения
						</p>
					</div>
					<p className="text-2xl font-bold text-yellow-600">
						{formatNumber(metrics.warningIssues?.length || 0)}
					</p>
					<p className="text-xs text-muted-foreground">требуют внимания</p>
				</CardContent>
			</Card>

			{/* Проблемные PID процессы */}
			<Card className="border-l-4 border-l-purple-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<Activity className="h-4 w-4 text-purple-600" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Проблемные PID
						</p>
					</div>
					<p className="text-2xl font-bold text-purple-600">
						{formatNumber(metrics.topBlockingPids?.length || 0)}
					</p>
					<p className="text-xs text-muted-foreground">блокирующих процессов</p>
				</CardContent>
			</Card>

			{/* Заблокированные таблицы */}
			<Card className="border-l-4 border-l-blue-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<Database className="h-4 w-4 text-blue-600" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Заблокированных таблиц
						</p>
					</div>
					<p className="text-2xl font-bold text-blue-600">
						{formatNumber(metrics.topBlockedTables?.length || 0)}
					</p>
					<p className="text-xs text-muted-foreground">с активными блокировками</p>
				</CardContent>
			</Card>

			{/* Время анализа */}
			{metrics.analysisTime && (
				<Card className="border-l-4 border-l-gray-500 py-3 md:col-span-2 lg:col-span-3">
					<CardContent className="py-1">
						<div className="flex items-center space-x-2 mb-2">
							<Clock className="h-4 w-4 text-gray-600" />
							<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
								Время анализа
							</p>
						</div>
						<p className="text-lg font-semibold text-gray-700">
							{new Date(metrics.analysisTime).toLocaleString("ru-RU", {
								day: "2-digit",
								month: "2-digit",
								year: "numeric",
								hour: "2-digit",
								minute: "2-digit",
								second: "2-digit",
							})}
						</p>
					</CardContent>
				</Card>
			)}
		</div>
	);
}
