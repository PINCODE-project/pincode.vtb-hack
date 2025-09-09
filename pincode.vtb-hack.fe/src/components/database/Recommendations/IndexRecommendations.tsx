import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@pin-code/ui-kit";
import { Activity } from "lucide-react";
import type { IndexRecommendation } from "@/generated/models/IndexRecommendation";
import { sortRecommendationsBySeverity } from "../utils/format";
import { getSeverityBadge } from "../utils/ui-helpers";

interface IndexRecommendationsProps {
	recommendations: IndexRecommendation[];
}

/**
 * Компонент для отображения рекомендаций по оптимизации индексов
 */
export function IndexRecommendations({ recommendations }: IndexRecommendationsProps) {
	const sortedRecommendations = sortRecommendationsBySeverity(recommendations);

	return (
		<div className="space-y-4">
			{sortedRecommendations.map((rec, index) => (
				<Card key={index} className="recommendation-card border-l-orange-500 database-analysis-card">
					<CardHeader className="pb-3">
						<div className="flex items-center justify-between">
							<CardTitle className="text-lg flex items-center gap-2">
								<Activity className="h-5 w-5" />
								{rec.indexName || "Индекс"}
							</CardTitle>
							{getSeverityBadge(rec.severity)}
						</div>
						<CardDescription className="text-sm">
							{rec.schemaName}.{rec.tableName}
						</CardDescription>
					</CardHeader>
					<CardContent>
						<p className="text-sm text-muted-foreground mb-3 whitespace-pre-wrap">{rec.recommendation}</p>

						<div className="grid grid-cols-1 md:grid-cols-3 gap-3">
							<div>
								<span className="metric-label">Тип метрики:</span>
								<p className="text-sm">{rec.metricType}</p>
							</div>
							{rec.averageValue !== undefined && (
								<div>
									<span className="metric-label">Среднее значение:</span>
									<p className="metric-value text-sm">{rec.averageValue.toFixed(2)}</p>
								</div>
							)}
							{rec.formattedSize && (
								<div>
									<span className="metric-label">Размер:</span>
									<p className="metric-value text-sm">{rec.formattedSize}</p>
								</div>
							)}
						</div>
					</CardContent>
				</Card>
			))}
		</div>
	);
}
