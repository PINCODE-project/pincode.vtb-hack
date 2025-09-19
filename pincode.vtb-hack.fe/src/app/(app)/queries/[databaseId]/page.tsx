"use client";
import React, { useCallback, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { Alert, AlertDescription, Button } from "@pin-code/ui-kit";
import { SqlEditor } from "@/components/ui/sql-editor";
import { format } from "sql-formatter";
import { useGetApiQueriesFind, usePostApiQueriesCreate } from "@/generated/hooks/QueryAnalysis";
import { ArrowLeft, Clock, Copy, Database, Loader2, Play, Sparkles } from "lucide-react";
import { DatabaseQueriesHistory, QueriesHistory } from "@/components/queries";
import { useGetApiDbConnectionsFind } from "@generated";
import { useSubstituteValues } from "@/components/queries/hooks";
import { CollapsibleList, type CollapsibleListItemType } from "@/components/ui/collapsible-list";

export default function DatabaseQueriesPage() {
	const params = useParams();
	const router = useRouter();
	const databaseId = params.databaseId as string;

	const [sqlQuery, setSqlQuery] = useState("");
	const [isFormatting, setIsFormatting] = useState(false);

	// Получаем список запросов
	const { data: allQueries, isLoading: isLoadingQueries, error: queriesError } = useGetApiQueriesFind();
	const { data: databases } = useGetApiDbConnectionsFind();

	// Мутация для создания запроса
	const createQueryMutation = usePostApiQueriesCreate({
		mutation: {
			onSuccess: (data) => {
				if (data.data && databaseId) {
					router.push(`/queries/${databaseId}/${data.data}`);
				}
			},
		},
	});

	// Мутация для подстановки значений в SQL
	const substituteMutation = useSubstituteValues({
		mutation: {
			onSuccess: (data) => {
				console.log("Подстановка значений успешна:", data);
				if (data) {
					const formatted = format(data, {
						language: "postgresql",
						tabWidth: 2,
						useTabs: false,
						keywordCase: "upper",
						identifierCase: "lower",
						functionCase: "upper",
					});
					setSqlQuery(formatted);
				}
			},
			onError: (error) => {
				console.error("Ошибка подстановки значений:", error);
				// В случае ошибки просто вставляем оригинальный запрос
			},
		},
	});

	// Обработка вставки с автоформатированием
	const handlePaste = useCallback(async (e: React.ClipboardEvent<HTMLDivElement>) => {
		e.preventDefault();
		const pastedText = e.clipboardData.getData("text");

		if (pastedText.trim()) {
			try {
				const formatted = format(pastedText, {
					language: "postgresql",
					tabWidth: 2,
					useTabs: false,
					keywordCase: "upper",
					identifierCase: "lower",
					functionCase: "upper",
				});
				setSqlQuery(formatted);
			} catch {
				// Если форматирование не удалось, вставляем как есть
				setSqlQuery(pastedText);
			}
		}
	}, []);

	// Форматирование SQL
	const handleFormatSql = useCallback(async () => {
		if (!sqlQuery.trim()) return;

		setIsFormatting(true);
		try {
			const formatted = format(sqlQuery, {
				language: "postgresql",
				tabWidth: 2,
				useTabs: false,
				keywordCase: "upper",
				identifierCase: "lower",
				functionCase: "upper",
			});
			setSqlQuery(formatted);
		} catch (error) {
			console.error("Ошибка форматирования SQL:", error);
		} finally {
			setIsFormatting(false);
		}
	}, [sqlQuery]);

	// Анализ запроса
	const handleAnalyzeQuery = useCallback(() => {
		if (!sqlQuery.trim() || !databaseId) return;

		createQueryMutation.mutate({
			data: {
				dbConnectionId: databaseId,
				sql: sqlQuery,
			},
		});
	}, [sqlQuery, databaseId, createQueryMutation]);

	// Копирование SQL в буфер обмена
	const handleCopyQuery = useCallback(async () => {
		if (!sqlQuery.trim()) return;

		try {
			await navigator.clipboard.writeText(sqlQuery);
			// Можно добавить toast уведомление о успешном копировании
		} catch (error) {
			console.error("Ошибка копирования в буфер обмена:", error);
		}
	}, [sqlQuery]);

	// Обработчик выбора запроса из истории БД
	const handleDatabaseQuerySelect = useCallback(
		(query: string) => {
			if (!databaseId || !query.trim()) return;

			// Пытаемся подставить значения через API
			substituteMutation.mutate(
				{
					data: {
						dbConnectionId: databaseId,
						sql: query,
					},
				},
				{
					onSuccess: () => {
						// Скроллим вверх к редактору после успешной подстановки
						window.scrollTo({ top: 0, behavior: "smooth" });
					},
					onError: () => {
						// В случае ошибки API, вставляем оригинальный запрос
						console.log("Fallback: используем оригинальный запрос");
						setSqlQuery(query);
						// Скроллим вверх даже при ошибке
						window.scrollTo({ top: 0, behavior: "smooth" });
					},
				},
			);
		},
		[databaseId, substituteMutation, setSqlQuery],
	);

	// Элементы для CollapsibleList
	const historyItems: CollapsibleListItemType[] = [
		{
			id: "database-history",
			title: "История запросов из БД",
			description: "Запросы из pg_stat_statements с метриками и рекомендациями.",
			icon: Database,
			isExpanded: false,
			content: (
				<div className="mr-1">
					<DatabaseQueriesHistory databaseId={databaseId} onQuerySelect={handleDatabaseQuerySelect} />
				</div>
			),
		},
		{
			id: "analyzed-history",
			title: "История проанализированных запросов",
			description: "Ранее созданные и проанализированные SQL запросы",
			icon: Clock,
			isExpanded: false,
			content: (
				<div className="mr-1">
					<QueriesHistory
						queries={allQueries}
						databases={databases}
						isLoading={isLoadingQueries}
						error={queriesError}
						showDatabaseNames={false}
						databaseFilter={databaseId}
						emptyStateMessage="Нет сохраненных запросов"
						gridCols="grid-cols-1 md:grid-cols-2 xl:grid-cols-3"
					/>
				</div>
			),
		},
	];

	return (
		<div className="p-6 space-y-6">
			<div className="mb-6">
				<h1 className="text-3xl font-bold">SQL Запросы</h1>
				<p className="text-muted-foreground mt-2">
					Создавайте и анализируйте SQL запросы для выбранной базы данных
				</p>

				<div className="flex items-center gap-4 mt-4">
					<Button variant="ghost" size="sm" onClick={() => router.push("/queries")}>
						<ArrowLeft className="h-4 w-4 mr-2" />
						Все запросы
					</Button>
				</div>
			</div>

			<div className="space-y-4">
				<SqlEditor
					placeholder="Введите ваш SQL запрос здесь..."
					value={sqlQuery}
					onChange={setSqlQuery}
					onPaste={handlePaste}
					minHeight="300px"
					actions={
						<>
							<Button
								variant="outline"
								size="sm"
								onClick={handleCopyQuery}
								disabled={!sqlQuery.trim()}
								title="Копировать SQL"
							>
								<Copy className="h-4 w-4" />
							</Button>

							<Button
								variant="outline"
								size="sm"
								onClick={handleFormatSql}
								disabled={!sqlQuery.trim() || isFormatting}
								title="Форматировать SQL"
							>
								<Sparkles className="h-4 w-4 stroke-yellow-400" />
								{isFormatting ? <Loader2 className="h-3 w-3 animate-spin ml-1" /> : null}
							</Button>

							<Button
								variant="outline"
								size="sm"
								onClick={handleAnalyzeQuery}
								disabled={!sqlQuery.trim() || createQueryMutation.isPending}
								title="Проанализировать запрос"
							>
								{createQueryMutation.isPending ? (
									<Loader2 className="h-4 w-4 animate-spin" />
								) : (
									<Play className="h-4 w-4 stroke-green-600" />
								)}
								Анализ
							</Button>
						</>
					}
				/>

				{createQueryMutation.error && (
					<Alert variant="destructive">
						<AlertDescription>
							Ошибка создания запроса: {createQueryMutation.error.message}
						</AlertDescription>
					</Alert>
				)}
			</div>

			{/* Списки с историями */}
			<CollapsibleList items={historyItems} />
		</div>
	);
}
