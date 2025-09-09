"use client";
import React, { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import {
	Accordion,
	AccordionContent,
	AccordionItem,
	AccordionTrigger,
	Alert,
	AlertDescription,
	Badge,
	Button,
	Skeleton,
} from "@pin-code/ui-kit";
import { format } from "sql-formatter";
import { useGetApiQueriesQueryid, usePostApiQueriesQueryidAnalyze } from "@/generated/hooks/QueryAnalysis";
import { AlertTriangle, ArrowLeft, Brain, CheckCircle, Code2, FileText, Lightbulb, Zap } from "lucide-react";
import { Prism as SyntaxHighlighter } from "react-syntax-highlighter";
import { oneDark } from "react-syntax-highlighter/dist/esm/styles/prism";
import { CodeCopyButton } from "@/components/ui/code-copy-button";

export default function QueryDetailPage() {
	const params = useParams();
	const router = useRouter();
	const databaseId = params.databaseId as string;
	const queryId = params.queryId as string;

	const [openAccordions, setOpenAccordions] = useState<string[]>([
		"sql-query",
		"algorithm-recommendations",
		"ai-recommendations",
		"optimized-query",
	]);

	// Получаем данные запроса
	const { data: queryData, isLoading: isLoadingQuery, error: queryError } = useGetApiQueriesQueryid(queryId);

	// Анализ запроса
	const analyzeQueryMutation = usePostApiQueriesQueryidAnalyze();

	useEffect(() => {
		analyzeQueryMutation.mutate({
			queryId,
			params: { useLlm: true },
		});
		// eslint-disable-next-line
	}, [queryId]);

	// Форматирование SQL
	const formatSql = (sql: string | null) => {
		if (!sql) return "";
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
			return sql;
		}
	};

	// Получение иконки для уровня серьезности
	const getSeverityIcon = (severity?: number) => {
		switch (severity) {
			case 0:
				return <CheckCircle className="h-4 w-4 text-green-500" />;
			case 1:
				return <AlertTriangle className="h-4 w-4 text-yellow-500" />;
			case 2:
				return <AlertTriangle className="h-4 w-4 text-red-500" />;
			default:
				return <Lightbulb className="h-4 w-4 text-blue-500" />;
		}
	};

	// Получение цвета бейджа для серьезности
	const getSeverityVariant = (severity?: number) => {
		switch (severity) {
			case 0:
				return "default";
			case 1:
				return "secondary";
			case 2:
				return "destructive";
			default:
				return "outline";
		}
	};

	// Получение текста для серьезности
	const getSeverityText = (severity?: number) => {
		switch (severity) {
			case 0:
				return "Низкая";
			case 1:
				return "Средняя";
			case 2:
				return "Высокая";
			default:
				return "Неизвестно";
		}
	};

	if (isLoadingQuery) {
		return (
			<div className="p-6 space-y-6">
				<div className="flex items-center gap-4">
					<Skeleton className="h-10 w-10" />
					<Skeleton className="h-8 w-64" />
				</div>
				<Skeleton className="h-96 w-full" />
				<Skeleton className="h-48 w-full" />
			</div>
		);
	}

	if (queryError || !queryData) {
		return (
			<div className="p-6">
				<Alert variant="destructive">
					<AlertDescription>
						Ошибка загрузки запроса: {queryError?.message || "Запрос не найден"}
					</AlertDescription>
				</Alert>
			</div>
		);
	}

	const analysisData = analyzeQueryMutation.data;

	return (
		<div className="p-6 space-y-6">
			{/* Заголовок */}
			<div className="flex items-center justify-between">
				<div className="flex items-center gap-4">
					<Button variant="ghost" size="sm" onClick={() => router.push(`/queries?databaseId=${databaseId}`)}>
						<ArrowLeft className="h-4 w-4 mr-2" />
						Назад к запросам
					</Button>
					<div>
						<h1 className="text-2xl font-bold">Анализ SQL запроса</h1>
						<p className="text-muted-foreground">
							Создан: {new Date(queryData.createdAt).toLocaleString()}
						</p>
					</div>
				</div>
			</div>

			<Accordion
				type="multiple"
				value={openAccordions}
				onValueChange={setOpenAccordions}
				className="flex flex-col gap-4"
			>
				{/* SQL Запрос */}
				<AccordionItem value="sql-query" className="border rounded-lg">
					<AccordionTrigger className="px-6 hover:no-underline">
						<div className="flex items-center gap-3">
							<Code2 className="h-5 w-5" />
							<div className="text-left">
								<h3 className="text-lg font-semibold">SQL Запрос</h3>
								<p className="text-sm text-muted-foreground">Исходный запрос для анализа</p>
							</div>
						</div>
					</AccordionTrigger>
					<AccordionContent className="px-6 pb-6">
						<div className="bg-gray-900 rounded-lg overflow-hidden relative group">
							<CodeCopyButton code={queryData.sql ?? ""} copyId={`sql-${queryId}`} language="sql" />
							<SyntaxHighlighter
								language="sql"
								style={oneDark}
								customStyle={{
									margin: 0,
									fontSize: "14px",
								}}
								showLineNumbers={true}
							>
								{formatSql(queryData.sql)}
							</SyntaxHighlighter>
						</div>
					</AccordionContent>
				</AccordionItem>

				{/* EXPLAIN результат */}

				<AccordionItem value="explain-result" className="border rounded-lg" disabled={!queryData.explainResult}>
					<AccordionTrigger className="px-6 hover:no-underline">
						<div className="flex items-center gap-3">
							<FileText className="h-5 w-5" />
							<div className="text-left">
								<h3 className="text-lg font-semibold">EXPLAIN результат</h3>
								<p className="text-sm text-muted-foreground">План выполнения запроса</p>
							</div>
						</div>
					</AccordionTrigger>
					<AccordionContent className="px-6 pb-6">
						<div className="bg-gray-900 rounded-lg overflow-hidden relative group">
							<CodeCopyButton
								code={queryData.explainResult ?? ""}
								copyId={`explain-${queryId}`}
								language="json"
							/>
							<SyntaxHighlighter
								language="json"
								style={oneDark}
								customStyle={{
									margin: 0,
									fontSize: "12px",
								}}
								showLineNumbers={true}
							>
								{queryData.explainResult!}
							</SyntaxHighlighter>
						</div>
					</AccordionContent>
				</AccordionItem>

				{/* Алгоритмические рекомендации */}
				<AccordionItem
					value="algorithm-recommendations"
					className="border rounded-lg"
					disabled={!analysisData?.algorithmRecommendation?.length}
				>
					<AccordionTrigger className="px-6 hover:no-underline">
						<div className="flex items-center gap-3">
							<Zap className="h-5 w-5" />
							<div className="text-left">
								<h3 className="text-lg font-semibold">Алгоритмические рекомендации</h3>
								<p className="text-sm text-muted-foreground">
									Рекомендации на основе статического анализа
								</p>
							</div>
						</div>
					</AccordionTrigger>
					<AccordionContent className="px-6 pb-6">
						<div className="space-y-4">
							{analysisData?.algorithmRecommendation?.map((rec, idx) => (
								<div key={idx} className="border rounded-lg p-4">
									<div className="flex items-center gap-2 mb-2">
										{getSeverityIcon(rec.severity)}
										<Badge variant={getSeverityVariant(rec.severity)}>
											{getSeverityText(rec.severity)}
										</Badge>
									</div>
									{rec.message && (
										<div className="mb-2">
											<h4 className="font-medium">Проблема:</h4>
											<p className="text-sm text-muted-foreground">{rec.message}</p>
										</div>
									)}
									{rec.suggestion && (
										<div>
											<h4 className="font-medium">Рекомендация:</h4>
											<p className="text-sm text-muted-foreground">{rec.suggestion}</p>
										</div>
									)}
								</div>
							))}
						</div>
					</AccordionContent>
				</AccordionItem>

				{/* ИИ рекомендации */}
				<AccordionItem
					value="ai-recommendations"
					className="border rounded-lg"
					disabled={!analysisData?.llmRecommendations}
				>
					<AccordionTrigger className="px-6 hover:no-underline">
						<div className="flex items-center gap-3">
							<Brain className="h-5 w-5" />
							<div className="text-left">
								<h3 className="text-lg font-semibold">ИИ рекомендации</h3>
								<p className="text-sm text-muted-foreground">Анализ и рекомендации на основе ИИ</p>
							</div>
						</div>
					</AccordionTrigger>
					{analysisData?.llmRecommendations && (
						<AccordionContent className="px-6 pb-6">
							<div className="space-y-6">
								{analysisData?.llmRecommendations?.problems?.length ? (
									<div>
										<h4 className="font-medium mb-3 flex items-center gap-2">
											<AlertTriangle className="h-4 w-4 text-red-500" />
											Обнаруженные проблемы
										</h4>
										<div className="space-y-3">
											{analysisData?.llmRecommendations?.problems.map((problem, idx) => (
												<div key={idx} className="border-l-4 border-red-500 pl-4">
													<p className="text-sm text-muted-foreground">{problem.message}</p>
												</div>
											))}
										</div>
									</div>
								) : null}

								{analysisData?.llmRecommendations?.recommendations?.length ? (
									<div>
										<h4 className="font-medium mb-3 flex items-center gap-2">
											<CheckCircle className="h-4 w-4 text-green-500" />
											Рекомендации по улучшению
										</h4>
										<div className="space-y-3">
											{analysisData?.llmRecommendations?.recommendations.map((rec, idx) => (
												<div key={idx} className="border-l-4 border-green-500 pl-4">
													<p className="text-sm text-muted-foreground">{rec.message}</p>
												</div>
											))}
										</div>
									</div>
								) : null}
							</div>
						</AccordionContent>
					)}
				</AccordionItem>

				{/* Оптимизированный запрос */}
				<AccordionItem
					value="optimized-query"
					className="border rounded-lg"
					disabled={!analysisData?.llmRecommendations?.newQuery}
				>
					<AccordionTrigger className="px-6 hover:no-underline">
						<div className="flex items-center gap-3">
							<Zap className="h-5 w-5" />
							<div className="text-left">
								<h3 className="text-lg font-semibold">Оптимизированный запрос</h3>
								<p className="text-sm text-muted-foreground">Улучшенная версия запроса</p>
							</div>
						</div>
					</AccordionTrigger>
					{analysisData?.llmRecommendations?.newQuery && (
						<AccordionContent className="px-6 pb-6">
							<div className="space-y-4">
								{analysisData?.llmRecommendations?.newQueryAbout && (
									<Alert>
										<Lightbulb className="h-4 w-4" />
										<AlertDescription>
											{analysisData.llmRecommendations.newQueryAbout}
										</AlertDescription>
									</Alert>
								)}
								<div className="bg-gray-900 rounded-lg overflow-hidden relative group">
									<CodeCopyButton
										code={analysisData.llmRecommendations.newQuery ?? ""}
										copyId={`optimized-${queryId}`}
										language="sql"
									/>
									<SyntaxHighlighter
										language="sql"
										style={oneDark}
										customStyle={{
											margin: 0,
											fontSize: "14px",
										}}
										showLineNumbers={true}
									>
										{formatSql(analysisData.llmRecommendations.newQuery)}
									</SyntaxHighlighter>
								</div>
							</div>
						</AccordionContent>
					)}
				</AccordionItem>
			</Accordion>

			{analyzeQueryMutation.error && (
				<Alert variant="destructive">
					<AlertDescription>Ошибка анализа: {analyzeQueryMutation.error.message}</AlertDescription>
				</Alert>
			)}
		</div>
	);
}
