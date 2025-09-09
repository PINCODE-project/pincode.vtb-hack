"use client";
import React, { useState, useCallback } from "react";
import { useParams, useRouter } from "next/navigation";
import { Card, CardContent, CardHeader, CardTitle, Button, Textarea, Alert, AlertDescription } from "@pin-code/ui-kit";
import { format } from "sql-formatter";
import { useGetApiQueriesFind, usePostApiQueriesCreate } from "@/generated/hooks/QueryAnalysis";
import { Loader2, Play, FileText, Clock, ArrowLeft } from "lucide-react";
import { QueriesHistory } from "@/components/queries";

export default function DatabaseQueriesPage() {
	const params = useParams();
	const router = useRouter();
	const databaseId = params.databaseId as string;

	const [sqlQuery, setSqlQuery] = useState("");
	const [isFormatting, setIsFormatting] = useState(false);

	// Получаем список запросов
	const { data: allQueries, isLoading: isLoadingQueries, error: queriesError } = useGetApiQueriesFind();

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

			{/* История запросов */}
			<Card>
				<CardHeader>
					<CardTitle className="flex items-center gap-2">
						<Clock className="h-5 w-5" />
						История запросов
					</CardTitle>
				</CardHeader>
				<CardContent>
					<QueriesHistory
						queries={allQueries}
						databases={[]}
						isLoading={isLoadingQueries}
						error={queriesError}
						showDatabaseNames={false}
						databaseFilter={databaseId}
						emptyStateMessage="Нет сохраненных запросов"
						gridCols="grid-cols-1 md:grid-cols-2 xl:grid-cols-3"
					/>
				</CardContent>
			</Card>
		</div>
	);
}
