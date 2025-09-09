import { AccordionContent, AccordionItem, AccordionTrigger } from "@pin-code/ui-kit";
import { Alert, AlertDescription, AlertTitle } from "@pin-code/ui-kit";
import { Card, CardContent } from "@pin-code/ui-kit";
import { AlertCircle, CheckCircle, Clock } from "lucide-react";
import { UseQueryResult } from "@tanstack/react-query";
import type { CacheAnalysisResponse } from "@/generated/models/CacheAnalysisResponse";
import { getStatusIcon } from "../utils/ui-helpers";
import { MetricsTooltip } from "../MetricsTooltip";
import { CacheRecommendations } from "../Recommendations";

interface CacheAnalysisSectionProps {
	query: UseQueryResult<CacheAnalysisResponse>;
}

/**
 * Секция анализа кэша
 */
export function CacheAnalysisSection({ query }: CacheAnalysisSectionProps) {
	return (
		<AccordionItem value="cache" className="border rounded-lg">
			<AccordionTrigger className="px-6 hover:no-underline">
				<div className="flex items-center gap-3">
					{getStatusIcon(query.data?.overallStatus)}
					<div className="text-left">
						<h3 className="text-lg font-semibold">Анализ кэша</h3>
						<p className="text-sm text-muted-foreground">Эффективность работы кэша PostgreSQL</p>
					</div>
					{query.data?.metricsSummary && (
						<MetricsTooltip data={query.data.metricsSummary} title="Метрики кэша" type="cache" />
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
						<AlertDescription>Не удалось загрузить данные анализа кэша</AlertDescription>
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
							<CacheRecommendations recommendations={query.data.recommendations} />
						) : (
							<Card>
								<CardContent>
									<div className="flex items-center gap-2 text-green-600">
										<CheckCircle className="h-5 w-5" />
										<span>Кэш работает эффективно</span>
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
