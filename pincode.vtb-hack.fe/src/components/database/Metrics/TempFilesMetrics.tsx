import type { TempFilesMetricsSummary } from "@/generated/models/TempFilesMetricsSummary";
import { formatNumber, formatBytes } from "../utils/format";
import { Card, CardContent } from "@pin-code/ui-kit";
import { HardDrive, Files, Clock, Activity } from "lucide-react";

interface TempFilesMetricsProps {
	metrics: TempFilesMetricsSummary;
}

/**
 * Компонент для отображения метрик временных файлов
 */
export function TempFilesMetrics({ metrics }: TempFilesMetricsProps) {
	return (
		<div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
			{/* Всего временных файлов */}
			<Card className="border-l-4 border-l-purple-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<Files className="h-4 w-4 text-purple-600 dark:text-purple-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Всего файлов
						</p>
					</div>
					<p className="text-2xl font-bold">{formatNumber(metrics.totalTempFiles)}</p>
					<p className="text-xs text-muted-foreground">временных файлов</p>
				</CardContent>
			</Card>

			{/* Общий объем */}
			<Card className="border-l-4 border-l-indigo-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<HardDrive className="h-4 w-4 text-indigo-600 dark:text-indigo-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">Общий объем</p>
					</div>
					<p className="text-2xl font-bold">{formatBytes(metrics.totalTempBytes)}</p>
					<p className="text-xs text-muted-foreground">записано на диск</p>
				</CardContent>
			</Card>

			{/* Файлов в минуту */}
			<Card className="border-l-4 border-l-cyan-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<Activity className="h-4 w-4 text-cyan-600 dark:text-cyan-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">Файлов/мин</p>
					</div>
					<p className="text-2xl font-bold">{metrics.tempFilesPerMinute?.toFixed(1)}</p>
					<p className="text-xs text-muted-foreground">интенсивность создания</p>
				</CardContent>
			</Card>

			{/* Байт в минуту */}
			<Card className="border-l-4 border-l-orange-500 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<Clock className="h-4 w-4 text-orange-600 dark:text-orange-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">Объем/мин</p>
					</div>
					<p className="text-2xl font-bold">{formatBytes(metrics.tempBytesPerMinute)}</p>
					<p className="text-xs text-muted-foreground">записи на диск</p>
				</CardContent>
			</Card>

			{/* Байт в секунду - критический показатель */}
			<Card className="border-l-4 border-l-red-500 md:col-span-2 lg:col-span-1 py-3">
				<CardContent className="py-1">
					<div className="flex items-center space-x-2 mb-2">
						<HardDrive className="h-4 w-4 text-red-600 dark:text-red-400" />
						<p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
							Нагрузка I/O
						</p>
					</div>
					<p className="text-2xl font-bold status-warning">{formatBytes(metrics.tempBytesPerSecond)}</p>
					<p className="text-xs text-muted-foreground">байт/сек на диск</p>
				</CardContent>
			</Card>
		</div>
	);
}
