import { AccordionContent, AccordionItem, AccordionTrigger } from "@pin-code/ui-kit";
import { Alert, AlertDescription, AlertTitle } from "@pin-code/ui-kit";
import { Card, CardContent } from "@pin-code/ui-kit";
import { AlertCircle, CheckCircle, Clock, HardDrive } from "lucide-react";
import { UseQueryResult } from "@tanstack/react-query";
import type { TempFilesRecommendationResponse } from "@/generated/models/TempFilesRecommendationResponse";
import { MetricsTooltip } from "../MetricsTooltip";
import { TempFilesRecommendations } from "../Recommendations";

interface TempFilesAnalysisSectionProps {
	query: UseQueryResult<TempFilesRecommendationResponse>;
}

/**
 * Секция анализа временных файлов
 */
export function TempFilesAnalysisSection({ query }: TempFilesAnalysisSectionProps) {
	return (
		<AccordionItem value="temp-files" className="border rounded-lg">
			<AccordionTrigger className="px-6 hover:no-underline">
				<div className="flex items-center gap-3">
					<HardDrive className="h-5 w-5 text-purple-500" />
					<div className="text-left">
						<h3 className="text-lg font-semibold">Анализ временных файлов</h3>
						<p className="text-sm text-muted-foreground">Оптимизация работы с временными файлами</p>
					</div>
					{query.data?.metricsSummary && (
						<MetricsTooltip
							data={query.data.metricsSummary}
							title="Метрики временных файлов"
							type="tempFiles"
						/>
					)}
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
						<AlertDescription>Не удалось загрузить данные анализа временных файлов</AlertDescription>
					</Alert>
				)}

				{query.data && (
					<div className="space-y-4">
						{query.data.analysisPeriodStart && query.data.analysisPeriodEnd && (
							<div className="text-sm text-muted-foreground">
								Период анализа: {new Date(query.data.analysisPeriodStart).toLocaleString()} -{" "}
								{new Date(query.data.analysisPeriodEnd).toLocaleString()}
							</div>
						)}

						{query.data.recommendations && query.data.recommendations.length > 0 ? (
							<TempFilesRecommendations recommendations={query.data.recommendations} />
						) : (
							<Card>
								<CardContent>
									<div className="flex items-center gap-2 text-green-600">
										<CheckCircle className="h-5 w-5" />
										<span>Временные файлы используются оптимально</span>
									</div>
								</CardContent>
							</Card>
						)}
					</div>
				)}
			</AccordionContent>
		</AccordionItem>
	);
}
