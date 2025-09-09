import { AccordionContent, AccordionItem, AccordionTrigger } from "@pin-code/ui-kit";
import { Alert, AlertDescription, AlertTitle } from "@pin-code/ui-kit";
import { Card, CardContent } from "@pin-code/ui-kit";
import { Activity, AlertCircle, CheckCircle, Clock } from "lucide-react";
import { UseQueryResult } from "@tanstack/react-query";
import type { IndexAnalysisResult } from "@/generated/models/IndexAnalysisResult";
import { MetricsDetails } from "../MetricsDetails";
import { IndexRecommendations } from "../Recommendations";
import { sortRecommendationsBySeverity } from "../utils/format";

interface IndexAnalysisSectionProps {
	query: UseQueryResult<IndexAnalysisResult>;
}

/**
 * Секция анализа индексов
 */
export function IndexAnalysisSection({ query }: IndexAnalysisSectionProps) {
	return (
		<AccordionItem value="indexes" className="border rounded-lg">
			<AccordionTrigger className="px-6 hover:no-underline">
				<div className="flex items-center gap-3">
					<Activity className="h-5 w-5 text-blue-500" />
					<div className="text-left">
						<h3 className="text-lg font-semibold">Анализ индексов</h3>
						<p className="text-sm text-muted-foreground">Рекомендации по оптимизации индексов</p>
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
						<AlertDescription>Не удалось загрузить данные анализа индексов</AlertDescription>
					</Alert>
				)}

				{query.data && (
					<div className="space-y-6">
						{query.data.periodStart && query.data.periodEnd && (
							<div className="text-sm text-muted-foreground">
								Период анализа: {new Date(query.data.periodStart).toLocaleString()} -{" "}
								{new Date(query.data.periodEnd).toLocaleString()}
							</div>
						)}

						{/* Неиспользуемые индексы */}
						{query.data.unusedIndexes && query.data.unusedIndexes.length > 0 && (
							<div>
								<h4 className="text-md font-semibold mb-3 text-red-600">Неиспользуемые индексы</h4>
								<IndexRecommendations
									recommendations={sortRecommendationsBySeverity(query.data.unusedIndexes)}
								/>
							</div>
						)}

						{/* Неэффективные индексы */}
						{query.data.inefficientIndexes && query.data.inefficientIndexes.length > 0 && (
							<div>
								<h4 className="text-md font-semibold mb-3 text-yellow-600">Неэффективные индексы</h4>
								<IndexRecommendations
									recommendations={sortRecommendationsBySeverity(query.data.inefficientIndexes)}
								/>
							</div>
						)}

						{/* Растущие индексы */}
						{query.data.growingIndexes && query.data.growingIndexes.length > 0 && (
							<div>
								<h4 className="text-md font-semibold mb-3 text-blue-600">Быстро растущие индексы</h4>
								<IndexRecommendations
									recommendations={sortRecommendationsBySeverity(query.data.growingIndexes)}
								/>
							</div>
						)}

						{!query.data.unusedIndexes?.length &&
							!query.data.inefficientIndexes?.length &&
							!query.data.growingIndexes?.length && (
								<Card>
									<CardContent>
										<div className="flex items-center gap-2 text-green-600">
											<CheckCircle className="h-5 w-5" />
											<span>Индексы работают эффективно</span>
										</div>
									</CardContent>
								</Card>
							)}

						{query.data.indexUsageStatistics && (
							<MetricsDetails
								data={query.data.indexUsageStatistics}
								title="Детальная статистика использования индексов"
								type="index"
							/>
						)}
					</div>
				)}
			</AccordionContent>
		</AccordionItem>
	);
}
