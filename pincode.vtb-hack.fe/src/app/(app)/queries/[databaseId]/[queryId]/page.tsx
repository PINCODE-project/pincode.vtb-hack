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
import { ExplainPlanVisualizer } from "@/components/query/ExplainPlanVisualizer";
import { TextWithSqlSnippets } from "@/components/ui/text-with-sql-snippets";

// Универсальный тип для всех рекомендаций
type UnifiedRecommendation = {
	severity?: number;
	problem?: string | null;
	recommendation?: string | null;
	source: "static" | "explain";
};

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

	// Объединяем рекомендации из обеих частей анализа
	const getCombinedRecommendations = (): UnifiedRecommendation[] => {
		const combined: UnifiedRecommendation[] = [];

		// Добавляем статический анализ
		const staticFindings = analysisData?.algorithmRecommendation?.queryAnalysisResult?.findings || [];
		staticFindings.forEach((finding) => {
			combined.push({
				severity: finding.severity,
				problem: finding.problem,
				recommendation: finding.recommendations, // recommendations во множественном числе
				source: "static",
			});
		});

		// Добавляем анализ плана выполнения
		const explainFindings = analysisData?.algorithmRecommendation?.explainAnalysisResult?.findings || [];
		explainFindings.forEach((finding) => {
			combined.push({
				severity: finding.severity,
				problem: finding.problem,
				recommendation: finding.recommendation, // recommendation в единственном числе
				source: "explain",
			});
		});

		return combined;
	};

	const combinedRecommendations = getCombinedRecommendations();

	return (
		<div className="p-6 space-y-6 max-w-[calc(100vw-450px-var(--spacing)*6)]">
			<div className="flex items-center justify-between">
				<div className="flex flex-col gap-4">
					<div>
						<h1 className="text-3xl font-bold">Анализ SQL запроса</h1>
						<p className="text-muted-foreground mt-2">
							Создан: {new Date(queryData.createdAt).toLocaleString()}
						</p>
					</div>

					<Button
						variant="ghost"
						size="sm"
						onClick={() => router.push(`/queries?databaseId=${databaseId}`)}
						className="w-fit"
					>
						<ArrowLeft className="h-4 w-4 mr-2" />
						Назад к запросам
					</Button>
				</div>
			</div>

			<Accordion
				type="multiple"
				value={openAccordions}
				onValueChange={setOpenAccordions}
				className="flex flex-col gap-4"
			>
				<AccordionItem value="sql-query" className="border rounded-lg bg-card">
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
								wrapLines={true}
							>
								{formatSql(queryData.sql)}
							</SyntaxHighlighter>
						</div>
					</AccordionContent>
				</AccordionItem>

				{/* Показываем секцию EXPLAIN только если есть результат */}
				{queryData.explainResult && (
					<AccordionItem value="explain-result" className="border rounded-lg">
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
							<ExplainPlanVisualizer explainResult={queryData.explainResult} />
						</AccordionContent>
					</AccordionItem>
				)}

				{/* Показываем секцию только если есть рекомендации или данные загружаются */}
				{(analyzeQueryMutation.isPending || combinedRecommendations.length > 0) && (
					<AccordionItem
						value="algorithm-recommendations"
						className="border rounded-lg"
						disabled={analyzeQueryMutation.isPending}
					>
						<AccordionTrigger className="px-6 hover:no-underline">
							<div className="flex items-center gap-3">
								<Zap className="h-5 w-5" />
								<div className="text-left">
									<h3 className="text-lg font-semibold">Алгоритмические рекомендации</h3>
									<p className="text-sm text-muted-foreground">
										{analyzeQueryMutation.isPending
											? "Анализируем запрос..."
											: "Рекомендации на основе статического анализа и плана выполнения"}
									</p>
								</div>
							</div>
						</AccordionTrigger>
						{!analyzeQueryMutation.isPending && combinedRecommendations.length > 0 && (
							<AccordionContent className="px-6 pb-6">
								<div className="space-y-4">
									{combinedRecommendations.map((rec, idx: number) => (
										<div key={idx} className="border rounded-lg p-4">
											<div className="flex items-center gap-2 mb-2">
												{getSeverityIcon(rec.severity)}
												<Badge variant={getSeverityVariant(rec.severity)}>
													{getSeverityText(rec.severity)}
												</Badge>
												<Badge variant="outline" className="text-xs">
													{rec.source === "static" ? "Статический анализ" : "Анализ плана"}
												</Badge>
											</div>
											{rec.problem && (
												<div className="mb-2">
													<h4 className="font-medium">Проблема:</h4>
													<div className="text-sm text-muted-foreground">
														<TextWithSqlSnippets text={rec.problem} />
													</div>
												</div>
											)}
											{rec.recommendation && (
												<div>
													<h4 className="font-medium">Рекомендация:</h4>
													<div className="text-sm text-muted-foreground">
														<TextWithSqlSnippets text={rec.recommendation} />
													</div>
												</div>
											)}
										</div>
									))}
								</div>
							</AccordionContent>
						)}
					</AccordionItem>
				)}

				{/* Показываем ИИ секцию только если есть рекомендации или данные загружаются */}
				{(analyzeQueryMutation.isPending || analysisData?.llmRecommendations) && (
					<AccordionItem
						value="ai-recommendations"
						className="border rounded-lg"
						disabled={analyzeQueryMutation.isPending}
					>
						<AccordionTrigger className="px-6 hover:no-underline">
							<div className="flex items-center gap-3">
								<Brain className="h-5 w-5" />
								<div className="text-left">
									<h3 className="text-lg font-semibold">ИИ рекомендации</h3>
									<p className="text-sm text-muted-foreground">
										{analyzeQueryMutation.isPending
											? "Генерируем рекомендации..."
											: "Анализ и рекомендации на основе ИИ"}
									</p>
								</div>
							</div>
						</AccordionTrigger>
						{!analyzeQueryMutation.isPending && analysisData?.llmRecommendations && (
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
														<div className="text-sm text-muted-foreground">
															<TextWithSqlSnippets text={problem.message || ""} />
														</div>
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
														<div className="text-sm text-muted-foreground">
															<TextWithSqlSnippets text={rec.message || ""} />
														</div>
													</div>
												))}
											</div>
										</div>
									) : null}
								</div>
							</AccordionContent>
						)}
					</AccordionItem>
				)}

				{/* Показываем секцию оптимизированного запроса только если есть запрос или данные загружаются */}
				{(analyzeQueryMutation.isPending || analysisData?.llmRecommendations?.newQuery) && (
					<AccordionItem
						value="optimized-query"
						className="border rounded-lg"
						disabled={analyzeQueryMutation.isPending}
					>
						<AccordionTrigger className="px-6 hover:no-underline">
							<div className="flex items-center gap-3">
								<Zap className="h-5 w-5" />
								<div className="text-left">
									<h3 className="text-lg font-semibold">Оптимизированный запрос</h3>
									<p className="text-sm text-muted-foreground">
										{analyzeQueryMutation.isPending
											? "Оптимизируем запрос..."
											: "Улучшенная версия запроса"}
									</p>
								</div>
							</div>
						</AccordionTrigger>
						{!analyzeQueryMutation.isPending && analysisData?.llmRecommendations?.newQuery && (
							<AccordionContent className="px-6 pb-6">
								<div className="space-y-4">
									{analysisData?.llmRecommendations?.newQueryAbout && (
										<Alert>
											<Lightbulb className="h-4 w-4" />
											<AlertDescription className="whitespace-pre-wrap">
												<TextWithSqlSnippets
													text={analysisData.llmRecommendations.newQueryAbout}
												/>
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
				)}
			</Accordion>

			{analyzeQueryMutation.error && (
				<Alert variant="destructive">
					<AlertDescription>Ошибка анализа: {analyzeQueryMutation.error.message}</AlertDescription>
				</Alert>
			)}
		</div>
	);
}
