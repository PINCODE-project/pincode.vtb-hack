import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@pin-code/ui-kit";
import { Lock, AlertTriangle, Info, CheckCircle } from "lucide-react";
import type { LockAnalysisResult } from "@/generated/models/LockAnalysisResult";

interface LockRecommendationsProps {
	analysis: LockAnalysisResult;
}

/**
 * Компонент для отображения рекомендаций по анализу блокировок
 */
export function LockRecommendations({ analysis }: LockRecommendationsProps) {
	const hasCriticalIssues = analysis.criticalIssues && analysis.criticalIssues.length > 0;
	const hasWarningIssues = analysis.warningIssues && analysis.warningIssues.length > 0;
	const hasRecommendations = analysis.recommendations && analysis.recommendations.length > 0;

	// Если нет проблем, показываем положительное сообщение
	if (!hasCriticalIssues && !hasWarningIssues && !hasRecommendations) {
		return (
			<Card>
				<CardContent>
					<div className="flex items-center gap-2 text-green-600 success-state">
						<CheckCircle className="h-5 w-5" />
						<span>Критические блокировки не обнаружены</span>
					</div>
				</CardContent>
			</Card>
		);
	}

	return (
		<div className="space-y-4">
			{/* Критические проблемы */}
			{hasCriticalIssues && (
				<Card className="border-l-red-500 bg-red-50/50">
					<CardHeader className="pb-3">
						<CardTitle className="text-lg flex items-center gap-2 text-red-700">
							<AlertTriangle className="h-5 w-5" />
							Критические блокировки
						</CardTitle>
						<CardDescription>Требуют немедленного вмешательства</CardDescription>
					</CardHeader>
					<CardContent>
						<ul className="space-y-2">
							{analysis.criticalIssues!.map((issue, index) => (
								<li key={index} className="text-sm text-red-700 flex items-start gap-2">
									<AlertTriangle className="h-4 w-4 mt-0.5 flex-shrink-0" />
									<span>{issue}</span>
								</li>
							))}
						</ul>
					</CardContent>
				</Card>
			)}

			{/* Предупреждения */}
			{hasWarningIssues && (
				<Card className="border-l-yellow-500 bg-yellow-50/50">
					<CardHeader className="pb-3">
						<CardTitle className="text-lg flex items-center gap-2 text-yellow-700">
							<Info className="h-5 w-5" />
							Потенциальные проблемы
						</CardTitle>
						<CardDescription>Требуют внимания</CardDescription>
					</CardHeader>
					<CardContent>
						<ul className="space-y-2">
							{analysis.warningIssues!.map((issue, index) => (
								<li key={index} className="text-sm text-yellow-700 flex items-start gap-2">
									<Info className="h-4 w-4 mt-0.5 flex-shrink-0" />
									<span>{issue}</span>
								</li>
							))}
						</ul>
					</CardContent>
				</Card>
			)}

			{/* Рекомендации */}
			{hasRecommendations && (
				<Card className="border-l-blue-500">
					<CardHeader className="pb-3">
						<CardTitle className="text-lg flex items-center gap-2">
							<Lock className="h-5 w-5" />
							Рекомендации по устранению
						</CardTitle>
						<CardDescription>Советы по оптимизации блокировок</CardDescription>
					</CardHeader>
					<CardContent>
						<ul className="space-y-2">
							{analysis.recommendations!.map((recommendation, index) => (
								<li key={index} className="text-sm text-muted-foreground flex items-start gap-2">
									<CheckCircle className="h-4 w-4 mt-0.5 flex-shrink-0 text-green-600" />
									<span>{recommendation}</span>
								</li>
							))}
						</ul>
					</CardContent>
				</Card>
			)}

			{/* Дополнительная информация */}
			{(analysis.topBlockingPids || analysis.topBlockedTables) && (
				<Card>
					<CardHeader className="pb-3">
						<CardTitle className="text-lg flex items-center gap-2">
							<Lock className="h-5 w-5" />
							Дополнительная информация
						</CardTitle>
					</CardHeader>
					<CardContent className="space-y-4">
						{analysis.topBlockingPids && analysis.topBlockingPids.length > 0 && (
							<div>
								<span className="text-sm font-medium text-muted-foreground">
									Проблемные процессы (PID):
								</span>
								<div className="flex flex-wrap gap-2 mt-1">
									{analysis.topBlockingPids.map((pid, index) => (
										<span
											key={index}
											className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-red-100 text-red-800"
										>
											{pid}
										</span>
									))}
								</div>
							</div>
						)}

						{analysis.topBlockedTables && analysis.topBlockedTables.length > 0 && (
							<div>
								<span className="text-sm font-medium text-muted-foreground">Блокируемые таблицы:</span>
								<div className="flex flex-wrap gap-2 mt-1">
									{analysis.topBlockedTables.map((table, index) => (
										<span
											key={index}
											className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800"
										>
											{table}
										</span>
									))}
								</div>
							</div>
						)}
					</CardContent>
				</Card>
			)}
		</div>
	);
}
