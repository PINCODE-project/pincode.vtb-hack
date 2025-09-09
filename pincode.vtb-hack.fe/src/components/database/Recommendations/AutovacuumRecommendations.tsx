import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@pin-code/ui-kit";
import { Database } from "lucide-react";
import type { AutovacuumRecommendation } from "@/generated/models/AutovacuumRecommendation";
import { sortRecommendationsBySeverity } from "../utils/format";
import { getSeverityBadge } from "../utils/ui-helpers";

interface AutovacuumRecommendationsProps {
	recommendations: AutovacuumRecommendation[];
}

/**
 * Компонент для отображения рекомендаций по настройке Autovacuum
 */
export function AutovacuumRecommendations({ recommendations }: AutovacuumRecommendationsProps) {
	const sortedRecommendations = sortRecommendationsBySeverity(recommendations);

	return (
		<div className="space-y-4">
			{sortedRecommendations.map((rec, index) => (
				<Card key={index} className="recommendation-card border-l-blue-500 database-analysis-card">
					<CardHeader className="pb-3">
						<div className="flex items-center justify-between">
							<CardTitle className="text-lg flex items-center gap-2">
								<Database className="h-5 w-5" />
								{rec.tableName || "Общая рекомендация"}
							</CardTitle>
							{getSeverityBadge(rec.severity)}
						</div>
						<CardDescription className="text-sm">Тип: {rec.type || "N/A"}</CardDescription>
					</CardHeader>
					<CardContent>
						<p className="text-sm text-muted-foreground mb-3 whitespace-pre-wrap">{rec.message}</p>

						{rec.parameter && (
							<div className="grid grid-cols-1 md:grid-cols-3 gap-3 mb-3">
								<div>
									<span className="metric-label">Параметр:</span>
									<p className="text-sm font-mono">{rec.parameter}</p>
								</div>
								<div>
									<span className="metric-label">Текущее значение:</span>
									<p className="metric-value text-sm">{rec.currentValue}</p>
								</div>
								<div>
									<span className="metric-label">Рекомендуемое:</span>
									<p className="metric-value text-sm text-green-600">{rec.recommendedValue}</p>
								</div>
							</div>
						)}

						{rec.sqlCommand && (
							<div className="mt-3">
								<span className="metric-label">SQL команда:</span>
								<pre className="sql-code mt-1 p-2 bg-muted rounded text-xs overflow-x-auto">
									{rec.sqlCommand}
								</pre>
							</div>
						)}
					</CardContent>
				</Card>
			))}
		</div>
	);
}
