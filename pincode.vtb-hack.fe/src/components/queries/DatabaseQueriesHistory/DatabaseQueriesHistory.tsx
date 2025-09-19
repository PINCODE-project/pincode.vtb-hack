"use client";

import React, { useCallback, useMemo } from "react";
import { Alert, AlertDescription, Badge, Skeleton, toast } from "@pin-code/ui-kit";
import { format } from "sql-formatter";
import { Activity, AlertTriangle, Clock, Copy, Database, FileText, Play } from "lucide-react";
import { useGetApiPgStateAnalysisTop } from "@/generated/hooks/PgStateAnalysis";
import type { QueryStatAdvanced } from "@/generated/models/QueryStatAdvanced";

interface DatabaseQueriesHistoryProps {
	databaseId: string;
	onQuerySelect: (query: string) => void;
	className?: string;
}

// Функция для определения цвета severity
const getSeverityColor = (severity?: number) => {
	switch (severity) {
		case 0:
			return "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300";
		case 1:
			return "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300";
		case 2:
			return "bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300";
		case 3:
			return "bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-300";
		case 4:
			return "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300";
		default:
			return "bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-300";
	}
};

// Функция для получения текста severity
const getSeverityText = (severity?: number) => {
	switch (severity) {
		case 0:
			return "Отлично";
		case 1:
			return "Хорошо";
		case 2:
			return "Нормально";
		case 3:
			return "Внимание";
		case 4:
			return "Критично";
		default:
			return "Неизвестно";
	}
};

// Функция форматирования времени
const formatTime = (timeMs?: number) => {
	if (!timeMs) return "0ms";
	if (timeMs < 1000) return `${Math.round(timeMs)}ms`;
	return `${(timeMs / 1000).toFixed(2)}s`;
};

// Функция форматирования чисел
const formatNumber = (num?: number) => {
	if (!num) return "0";
	if (num >= 1000000) return `${(num / 1000000).toFixed(1)}M`;
	if (num >= 1000) return `${(num / 1000).toFixed(1)}K`;
	return num.toString();
};

export function DatabaseQueriesHistory({ databaseId, onQuerySelect, className = "" }: DatabaseQueriesHistoryProps) {
	const {
		data: analysisData,
		isLoading,
		error,
	} = useGetApiPgStateAnalysisTop({
		dbConnectionId: databaseId,
	});

	// Форматирование SQL для отображения
	const formatSqlForDisplay = useCallback((sql?: string | null) => {
		if (!sql) return "-- Пустой запрос";

		try {
			return format(sql, {
				language: "postgresql",
				tabWidth: 2,
				useTabs: false,
				keywordCase: "upper",
				identifierCase: "lower",
				functionCase: "upper",
			});
		} catch {
			return sql; // Если форматирование не удалось, возвращаем как есть
		}
	}, []);

	const handleCopyClick = useCallback((query: QueryStatAdvanced, event: React.MouseEvent) => {
		event.stopPropagation();
		if (query.query) {
			navigator.clipboard.writeText(query.query);
			toast.success("Успешно скопировано!");
		}
	}, []);

	// Обработчик клика на запрос
	const handleQueryClick = useCallback(
		(query: QueryStatAdvanced) => {
			if (query.query) {
				onQuerySelect(query.query);
			}
		},
		[onQuerySelect],
	);

	// Обработанные запросы
	const processedQueries = useMemo(() => {
		return analysisData?.results || [];
	}, [analysisData]);

	if (isLoading) {
		return (
			<div className={`space-y-4 ${className}`}>
				{Array.from({ length: 6 }).map((_, i) => (
					<Skeleton key={i} className="h-32 w-full" />
				))}
			</div>
		);
	}

	if (error) {
		return (
			<Alert variant="destructive" className={className}>
				<AlertTriangle className="h-4 w-4" />
				<AlertDescription>Ошибка загрузки истории запросов из БД: {error.message}</AlertDescription>
			</Alert>
		);
	}

	if (processedQueries.length === 0) {
		return (
			<div className={`text-center py-12 text-muted-foreground ${className}`}>
				<Database className="h-16 w-16 mx-auto mb-4 opacity-50" />
				<p className="text-lg">Нет истории запросов в БД</p>
				<p className="text-sm">Выполните несколько запросов, чтобы они появились в статистике</p>
			</div>
		);
	}

	return (
		<div className={`space-y-4 ${className}`}>
			{/* Информация о генерации отчета */}
			{analysisData?.generatedAtUtc && (
				<div className="text-sm text-muted-foreground mb-4">
					<Clock className="h-4 w-4 inline mr-2" />
					Данные обновлены: {new Date(analysisData.generatedAtUtc).toLocaleString("ru-RU")}
				</div>
			)}

			{/* Список запросов */}
			{processedQueries.map((query, index) => (
				<div
					key={index}
					className="cursor-pointer p-4 rounded-lg border transition-colors"
					onClick={() => handleQueryClick(query)}
				>
					<div className="space-y-3">
						{/* Заголовок с метриками */}
						<div className="flex items-center justify-between">
							<div className="flex items-center gap-2">
								<Badge className={getSeverityColor(query.severity)}>
									{getSeverityText(query.severity)}
								</Badge>
								{query.score && (
									<Badge variant="outline" className="font-mono">
										Score: {query.score.toFixed(2)}
									</Badge>
								)}
							</div>

							<div className="flex items-center gap-4 text-sm text-muted-foreground">
								<div className="flex items-center gap-1">
									<Activity className="h-4 w-4" />
									{formatNumber(query.calls)} вызовов
								</div>
								<div className="flex items-center gap-1">
									<Clock className="h-4 w-4" />
									{formatTime(query.meanTimeMs)} среднее время
								</div>

								<div className="flex items-center gap-1">
									<button
										type="button"
										onClick={(e) => handleCopyClick(query, e)}
										className="p-2 hover:bg-accent rounded-md transition-colors group"
										title="Скопировать SQL код"
									>
										<Copy className="h-4 w-4 text-muted-foreground group-hover:text-foreground" />
									</button>
									<button
										type="button"
										onClick={() => handleQueryClick(query)}
										className="p-2 hover:bg-accent rounded-md transition-colors group"
										title="Вставить запрос в редактор"
									>
										<Play className="h-4 w-4 text-muted-foreground group-hover:text-foreground stroke-green-600" />
									</button>
								</div>
							</div>
						</div>

						{/* SQL код */}
						<div className="bg-muted rounded-md p-3">
							<pre className="text-sm overflow-x-auto whitespace-pre-wrap font-mono">
								{formatSqlForDisplay(query.query)}
							</pre>
						</div>

						{/* Детальные метрики */}
						<div className="grid grid-cols-2 md:grid-cols-4 gap-3 text-sm">
							<div>
								<span className="text-muted-foreground">Общее время:</span>
								<div className="font-mono">{formatTime(query.totalTimeMs)}</div>
							</div>
							<div>
								<span className="text-muted-foreground">Строки:</span>
								<div className="font-mono">{formatNumber(query.rows)}</div>
							</div>
							<div>
								<span className="text-muted-foreground">Блоки чтения:</span>
								<div className="font-mono">{formatNumber(query.sharedBlksRead)}</div>
							</div>
							<div>
								<span className="text-muted-foreground">Temp блоки:</span>
								<div className="font-mono">{formatNumber(query.tempBlksWritten)}</div>
							</div>
						</div>

						{/* Рекомендации */}
						{query.suggestions && query.suggestions.length > 0 && (
							<div className="space-y-2">
								<div className="text-sm font-medium text-muted-foreground">Рекомендации:</div>
								<div className="space-y-1">
									{query.suggestions.map((suggestion, suggestionIndex) => (
										<div
											key={suggestionIndex}
											className="text-sm bg-blue-50 dark:bg-blue-950/30 p-2 rounded border-l-4 border-blue-200 dark:border-blue-800"
										>
											{suggestion.title && (
												<div className="font-medium text-blue-900 dark:text-blue-100 mb-1">
													{suggestion.title}
												</div>
											)}
											{suggestion.description && (
												<div className="text-blue-700 dark:text-blue-200">
													{suggestion.description}
												</div>
											)}
											{suggestion.exampleSql && (
												<div className="mt-2 p-2 bg-blue-100 dark:bg-blue-900/50 rounded text-xs font-mono">
													{suggestion.exampleSql}
												</div>
											)}
										</div>
									))}
								</div>
							</div>
						)}
					</div>
				</div>
			))}

			{/* Примечание */}
			{analysisData?.note && (
				<Alert>
					<FileText className="h-4 w-4" />
					<AlertDescription>{analysisData.note}</AlertDescription>
				</Alert>
			)}
		</div>
	);
}
