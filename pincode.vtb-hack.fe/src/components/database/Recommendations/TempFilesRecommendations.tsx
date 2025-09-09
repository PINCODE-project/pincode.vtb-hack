import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@pin-code/ui-kit";
import { HardDrive } from "lucide-react";
import type { TempFilesRecommendation } from "@/generated/models/TempFilesRecommendation";
import { sortRecommendationsBySeverity } from "../utils/format";
import { getSeverityBadge } from "../utils/ui-helpers";

interface TempFilesRecommendationsProps {
	recommendations: TempFilesRecommendation[];
}

/**
 * Компонент для отображения рекомендаций по оптимизации временных файлов
 */
export function TempFilesRecommendations({ recommendations }: TempFilesRecommendationsProps) {
	const sortedRecommendations = sortRecommendationsBySeverity(recommendations);

	return (
		<div className="space-y-4">
			{sortedRecommendations.map((rec, index) => (
				<Card key={index} className="recommendation-card border-l-red-500 database-analysis-card">
					<CardHeader className="pb-3">
						<div className="flex items-center justify-between">
							<CardTitle className="text-lg flex items-center gap-2">
								<HardDrive className="h-5 w-5" />
								Временные файлы
							</CardTitle>
							{getSeverityBadge(rec.severity)}
						</div>
						<CardDescription className="text-sm">Тип: {rec.type || "N/A"}</CardDescription>
					</CardHeader>
					<CardContent>
						<p className="text-sm text-muted-foreground mb-3 whitespace-pre-wrap">{rec.message}</p>

						<div className="grid grid-cols-1 md:grid-cols-2 gap-3">
							{rec.currentValue !== undefined && (
								<div>
									<span className="metric-label">Текущее значение:</span>
									<p className="metric-value text-sm">{rec.currentValue.toFixed(2)}</p>
								</div>
							)}
							{rec.threshold !== undefined && (
								<div>
									<span className="metric-label">Порог:</span>
									<p className="metric-value text-sm">{rec.threshold.toFixed(2)}</p>
								</div>
							)}
						</div>
					</CardContent>
				</Card>
			))}
		</div>
	);
}
