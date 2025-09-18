import {
	AccordionContent,
	AccordionItem,
	AccordionTrigger,
	Alert,
	AlertDescription,
	AlertTitle,
} from "@pin-code/ui-kit";
import { AlertCircle, CheckCircle, Clock, Lock } from "lucide-react";
import { UseQueryResult } from "@tanstack/react-query";
import type { LockAnalysisResult } from "@/generated/models/LockAnalysisResult";
import { MetricsDetails } from "../MetricsDetails";
import { LockRecommendations } from "../Recommendations";

interface LockAnalysisSectionProps {
	query: UseQueryResult<LockAnalysisResult>;
}

/**
 * Получает иконку статуса на основе данных анализа блокировок
 */
function getLockStatusIcon(data?: LockAnalysisResult) {
	if (!data) return <Lock className="h-5 w-5 text-gray-500" />;

	const hasCritical = data.criticalIssues && data.criticalIssues.length > 0;
	const hasWarnings = data.warningIssues && data.warningIssues.length > 0;
	const hasHighBlockCount = (data.totalBlockedLocks || 0) > 50;

	if (hasCritical || hasHighBlockCount) {
		return <AlertCircle className="h-5 w-5 text-red-500" />;
	}

	if (hasWarnings || (data.totalBlockedLocks || 0) > 10) {
		return <AlertCircle className="h-5 w-5 text-yellow-500" />;
	}

	return <CheckCircle className="h-5 w-5 text-green-500" />;
}

/**
 * Секция анализа блокировок
 */
export function LockAnalysisSection({ query }: LockAnalysisSectionProps) {
	return (
		<AccordionItem value="locks" className="border rounded-lg">
			<AccordionTrigger className="px-6 hover:no-underline">
				<div className="flex items-center gap-3">
					{getLockStatusIcon(query.data)}
					<div className="text-left">
						<h3 className="text-lg font-semibold">Анализ блокировок</h3>
						<p className="text-sm text-muted-foreground">Мониторинг и диагностика блокировок PostgreSQL</p>
					</div>
				</div>
			</AccordionTrigger>
			<AccordionContent className="px-6 pb-6">
				{query.isLoading && (
					<div className="flex items-center gap-2 text-muted-foreground">
						<Clock className="h-4 w-4 loading-spinner" />
						Загрузка данных...
					</div>
				)}

				{query.error && (
					<Alert variant="destructive">
						<AlertCircle className="h-4 w-4" />
						<AlertTitle>Ошибка загрузки</AlertTitle>
						<AlertDescription>Не удалось загрузить данные анализа блокировок</AlertDescription>
					</Alert>
				)}

				{query.data && (
					<div className="space-y-4">
						{query.data.analysisTime && (
							<div className="text-sm text-muted-foreground">
								Время анализа:{" "}
								{new Date(query.data.analysisTime).toLocaleString("ru-RU", {
									day: "2-digit",
									month: "2-digit",
									year: "numeric",
									hour: "2-digit",
									minute: "2-digit",
									second: "2-digit",
								})}
							</div>
						)}

						{/* Рекомендации и анализ */}
						<LockRecommendations analysis={query.data} />

						{/* Детальные метрики блокировок */}
						<MetricsDetails data={query.data} title="Детальные метрики блокировок" type="locks" />
					</div>
				)}
			</AccordionContent>
		</AccordionItem>
	);
}
