"use client";
import React, { useState, useCallback } from "react";
import { useParams, useRouter } from "next/navigation";
import {
	Card,
	CardContent,
	CardHeader,
	CardTitle,
	Button,
	Textarea,
	Alert,
	AlertDescription,
	Accordion,
	AccordionContent,
	AccordionItem,
	AccordionTrigger,
} from "@pin-code/ui-kit";
import { format } from "sql-formatter";
import { useGetApiQueriesFind, usePostApiQueriesCreate } from "@/generated/hooks/QueryAnalysis";
import { Loader2, Play, FileText, Clock, ArrowLeft, Database, Sparkles } from "lucide-react";
import { QueriesHistory, DatabaseQueriesHistory } from "@/components/queries";
import { useGetApiDbConnectionsFind } from "@generated";
import { useSubstituteValues } from "@/components/queries/hooks";

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
	const handlePaste = useCallback(async (e: React.ClipboardEvent<HTMLTextAreaElement>) => {
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

			<Card>
				<CardHeader>
					<CardTitle className="flex items-center gap-2">
						<FileText className="h-5 w-5" />
						SQL Редактор
					</CardTitle>
				</CardHeader>
				<CardContent className="space-y-4">
					<div className="relative">
						<Textarea
							placeholder="Введите ваш SQL запрос здесь..."
							value={sqlQuery}
							onChange={(e) => setSqlQuery(e.target.value)}
							onPaste={handlePaste}
							className="min-h-[300px] font-mono text-sm"
						/>
					</div>

					<div className="flex gap-2">
						<Button variant="outline" onClick={handleFormatSql} disabled={!sqlQuery.trim() || isFormatting}>
							<Sparkles />
							{isFormatting ? <Loader2 className="h-4 w-4 animate-spin mr-2" /> : null}
							Форматировать
						</Button>

						<Button
							onClick={handleAnalyzeQuery}
							disabled={!sqlQuery.trim() || createQueryMutation.isPending}
							className="ml-auto"
						>
							{createQueryMutation.isPending ? (
								<Loader2 className="h-4 w-4 animate-spin mr-2" />
							) : (
								<Play className="h-4 w-4 mr-2" />
							)}
							Проанализировать
						</Button>
					</div>

					{createQueryMutation.error && (
						<Alert variant="destructive">
							<AlertDescription>
								Ошибка создания запроса: {createQueryMutation.error.message}
							</AlertDescription>
						</Alert>
					)}
				</CardContent>
			</Card>

			{/* Аккордеоны с историями */}
			<Accordion type="multiple" className="space-y-4">
				{/* История запросов из БД */}
				<AccordionItem value="database-history" className="border rounded-lg">
					<AccordionTrigger className="hover:no-underline px-6 py-4">
						<div className="flex items-center gap-2">
							<Database className="h-5 w-5" />
							<div className="text-left">
								<div className="text-lg font-semibold">История запросов из БД</div>
								<div className="text-sm text-muted-foreground font-normal">
									Запросы из pg_stat_statements с метриками и рекомендациями. Нажмите на запрос, чтобы
									вставить его в редактор.
								</div>
							</div>
						</div>
					</AccordionTrigger>
					<AccordionContent className="px-6 pb-6">
						<DatabaseQueriesHistory databaseId={databaseId} onQuerySelect={handleDatabaseQuerySelect} />
					</AccordionContent>
				</AccordionItem>

				{/* История проанализированных запросов */}
				<AccordionItem value="analyzed-history" className="border rounded-lg">
					<AccordionTrigger className="hover:no-underline px-6 py-4">
						<div className="flex items-center gap-2">
							<Clock className="h-5 w-5" />
							<div className="text-left">
								<div className="text-lg font-semibold">История проанализированных запросов</div>
								<div className="text-sm text-muted-foreground font-normal">
									Ранее созданные и проанализированные SQL запросы
								</div>
							</div>
						</div>
					</AccordionTrigger>
					<AccordionContent className="px-6 pb-6">
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
					</AccordionContent>
				</AccordionItem>
			</Accordion>
		</div>
	);
}
