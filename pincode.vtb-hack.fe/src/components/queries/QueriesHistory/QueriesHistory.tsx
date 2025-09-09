"use client";

import React, { useCallback, useMemo } from "react";
import { useRouter } from "next/navigation";
import { Alert, AlertDescription, Badge, Card, CardContent, Skeleton } from "@pin-code/ui-kit";
import { format } from "sql-formatter";
import { FileText } from "lucide-react";
import { Prism as SyntaxHighlighter } from "react-syntax-highlighter";
import { materialDark } from "react-syntax-highlighter/dist/esm/styles/prism";
import { CodeCopyButton } from "@/components/ui/code-copy-button";

interface QueryData {
	id: string;
	sql?: string | null;
	dbConnectionId: string;
	createdAt: string;
}

interface DatabaseData {
	id: string;
	name: string | null;
}

interface QueriesHistoryProps {
	queries?: QueryData[];
	databases?: DatabaseData[];
	isLoading?: boolean;
	error?: Error | null;
	showDatabaseNames?: boolean;
	databaseFilter?: string; // Если передан, фильтрует запросы по конкретной БД
	emptyStateMessage?: string;
	className?: string;
	gridCols?: string; // CSS классы для grid-cols
}

export function QueriesHistory({
	queries = [],
	databases = [],
	isLoading = false,
	error = null,
	showDatabaseNames = true,
	databaseFilter,
	emptyStateMessage = "Нет сохраненных запросов",
	className = "",
	gridCols = "grid-cols-1 md:grid-cols-2",
}: QueriesHistoryProps) {
	const router = useRouter();

	// Создаем карту БД для быстрого доступа
	const databasesMap = useMemo(
		() => Object.fromEntries(databases?.map((item) => [item.id, item]) ?? []),
		[databases],
	);

	// Фильтрация и дедупликация запросов
	const processedQueries = useMemo(() => {
		let filteredQueries = queries;

		// Фильтруем по БД если указан фильтр
		if (databaseFilter) {
			filteredQueries = queries.filter((query) => query.dbConnectionId === databaseFilter);
		}

		if (filteredQueries.length === 0) return [];

		// Дедупликация запросов - оставляем только самый новый запрос для каждой комбинации SQL + БД
		const queryMap = new Map<string, QueryData>();

		filteredQueries.forEach((query) => {
			// Создаем ключ из нормализованного SQL и ID БД
			const normalizedSql = query.sql?.trim().toLowerCase() || "";
			const key = `${query.dbConnectionId}_${normalizedSql}`;

			const existingQuery = queryMap.get(key);
			if (!existingQuery || new Date(query.createdAt) > new Date(existingQuery.createdAt)) {
				queryMap.set(key, query);
			}
		});

		// Возвращаем массив отсортированный по дате создания (новые сверху)
		return Array.from(queryMap.values()).sort(
			(a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
		);
	}, [queries, databaseFilter]);

	// Обработчик выбора запроса
	const handleSelectQuery = useCallback(
		(queryId: string, dbConnectionId: string) => {
			router.push(`/queries/${dbConnectionId}/${queryId}`);
		},
		[router],
	);

	// Форматирование SQL для отображения
	const formatSqlForDisplay = useCallback((sql?: string) => {
		if (!sql) return "-- Пустой запрос";

		try {
			return format(sql, {
				language: "postgresql",
				tabWidth: 2,
				useTabs: false,
				keywordCase: "upper",
				identifierCase: "lower",
				indentStyle: "tabularLeft",
				functionCase: "upper",
			});
		} catch {
			return sql; // Если форматирование не удалось, возвращаем как есть
		}
	}, []);

	// Расчет динамической высоты для SQL блока
	const calculateSqlHeight = useCallback(
		(sql?: string) => {
			const formattedSql = formatSqlForDisplay(sql);
			const sqlLines = formattedSql.split("\n").length;
			return Math.min(Math.max(sqlLines * 20 + 100, 180), 400);
		},
		[formatSqlForDisplay],
	);

	if (isLoading) {
		return (
			<div className={`grid ${gridCols} gap-4 ${className}`}>
				{Array.from({ length: 8 }).map((_, i) => (
					<Skeleton key={i} className="h-48 w-full" />
				))}
			</div>
		);
	}

	if (error) {
		return (
			<Alert variant="destructive" className={className}>
				<AlertDescription>Ошибка загрузки запросов: {error.message}</AlertDescription>
			</Alert>
		);
	}

	if (processedQueries.length === 0) {
		return (
			<div className={`text-center py-12 text-muted-foreground ${className}`}>
				<FileText className="h-16 w-16 mx-auto mb-4 opacity-50" />
				<p className="text-lg">{emptyStateMessage}</p>
				<p className="text-sm">
					{databaseFilter ? "Создайте первый запрос выше" : "Создайте первый запрос выбрав БД слева"}
				</p>
			</div>
		);
	}

	return (
		<div className={`grid ${gridCols} gap-4 ${className}`}>
			{processedQueries.map((query) => {
				const estimatedHeight = calculateSqlHeight(query.sql ?? undefined);
				const formattedSql = formatSqlForDisplay(query.sql ?? undefined);

				return (
					<Card
						key={query.id}
						className="cursor-pointer card-hover flex flex-col h-fit grow"
						onClick={() => handleSelectQuery(query.id, query.dbConnectionId)}
					>
						<CardContent className="flex flex-col h-full">
							<div className="space-y-3 flex-1">
								<div className="flex items-center justify-between gap-2">
									<Badge variant="outline" className="text-xs truncate flex-shrink-0">
										{databasesMap[query.dbConnectionId]?.name ?? "Unknown DB"}
									</Badge>

									<Badge
										variant="secondary"
										className={`text-xs flex-shrink-0 ${!showDatabaseNames ? "mr-auto" : ""}`}
									>
										{new Date(query.createdAt).toLocaleDateString()}
									</Badge>
								</div>
								<div className="bg-gray-900 rounded-lg overflow-hidden flex-1 relative group">
									<CodeCopyButton code={query.sql ?? ""} copyId={query.id} language="sql" />
									<SyntaxHighlighter
										language="sql"
										style={materialDark}
										customStyle={{
											margin: 0,
											fontSize: "11px",
											minHeight: "120px",
											maxHeight: `${estimatedHeight}px`,
											overflow: "auto",
											padding: "12px",
										}}
										showLineNumbers={false}
										wrapLines={true}
										wrapLongLines={true}
									>
										{formattedSql}
									</SyntaxHighlighter>
								</div>
							</div>
						</CardContent>
					</Card>
				);
			})}
		</div>
	);
}
