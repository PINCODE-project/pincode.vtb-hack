"use client";
import React, { useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import { Alert, AlertDescription, Badge, Button, Skeleton } from "@pin-code/ui-kit";
import { format } from "sql-formatter";
import { useGetApiQueriesQueryid, usePostApiQueriesQueryidAnalyze } from "@/generated/hooks/QueryAnalysis";
import { AlertTriangle, ArrowLeft, Brain, CheckCircle, Code2, FileText, Lightbulb, Zap } from "lucide-react";
import { Prism as SyntaxHighlighter } from "react-syntax-highlighter";
import { oneDark } from "react-syntax-highlighter/dist/esm/styles/prism";
import { CodeCopyButton } from "@/components/ui/code-copy-button";
import { ExplainPlanVisualizer } from "@/components/query/ExplainPlanVisualizer";
import { QueryPerformanceComparison } from "@/components/query/QueryPerformanceComparison";
import { TextWithSqlSnippets } from "@/components/ui/text-with-sql-snippets";
import { CollapsibleList, type CollapsibleListItemType } from "@/components/ui/collapsible-list";
import type { PlanNode } from "@/generated/models/PlanNode";

// Универсальный тип для всех рекомендаций
type UnifiedRecommendation = {
	severity?: number;
	problem?: string | null;
	recommendation?: string | null;
	source: "static" | "explain";
};

// Функция для конвертации PlanNode в формат, ожидаемый ExplainPlanVisualizer
const convertPlanNodeToExplainNode = (node: PlanNode): any => {
	const result: any = {
		"Node Type": node.nodeType || "Unknown",
	};

	// Добавляем основные метрики если они есть
	if (node.startupCost !== null && node.startupCost !== undefined) {
		result["Startup Cost"] = node.startupCost;
	}
	if (node.totalCost !== null && node.totalCost !== undefined) {
		result["Total Cost"] = node.totalCost;
	}
	if (node.planRows !== null && node.planRows !== undefined) {
		result["Plan Rows"] = node.planRows;
	}
	if (node.planWidth !== null && node.planWidth !== undefined) {
		result["Plan Width"] = node.planWidth;
	}
	if (node.actualStartupTimeMs !== null && node.actualStartupTimeMs !== undefined) {
		result["Actual Startup Time"] = node.actualStartupTimeMs;
	}
	if (node.actualTotalTimeMs !== null && node.actualTotalTimeMs !== undefined) {
		result["Actual Total Time"] = node.actualTotalTimeMs;
	}
	if (node.actualRows !== null && node.actualRows !== undefined) {
		result["Actual Rows"] = node.actualRows;
	}
	if (node.actualLoops !== null && node.actualLoops !== undefined) {
		result["Actual Loops"] = node.actualLoops;
	}

	// Добавляем специфичные для узла данные
	if (node.nodeSpecific) {
		Object.keys(node.nodeSpecific).forEach((key) => {
			const value = node.nodeSpecific![key];
			if (value !== null && value !== undefined) {
				result[key] = value;
			}
		});
	}

	// Рекурсивно обрабатываем дочерние узлы
	if (node.children && node.children.length > 0) {
		result.Plans = node.children.map(convertPlanNodeToExplainNode);
	}

	return result;
};

export default function QueryDetailPage() {
	const params = useParams();
	const router = useRouter();
	const databaseId = params.databaseId as string;
	const queryId = params.queryId as string;

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

	// Создаем элементы для CollapsibleList
	const queryAnalysisItems: CollapsibleListItemType[] = [
		// SQL Запрос
		{
			id: "sql-query",
			title: "SQL Запрос",
			description: "Исходный запрос для анализа",
			icon: Code2,
			isExpanded: false,
			content: (
				<div className="bg-gray-900 rounded-lg overflow-hidden relative group mr-1">
					<CodeCopyButton code={queryData?.sql ?? ""} copyId={`sql-${queryId}`} language="sql" />
					<SyntaxHighlighter
						language="sql"
						style={oneDark}
						customStyle={{
							margin: 0,
							fontSize: "12px",
						}}
						showLineNumbers={true}
						wrapLines={true}
					>
						{formatSql(queryData?.sql)}
					</SyntaxHighlighter>
				</div>
			),
		},
		// EXPLAIN результат (только если есть)
		...(queryData?.explainResult?.rootNode
			? [
					{
						id: "explain-result",
						title: "EXPLAIN результат",
						description: "План выполнения запроса",
						icon: FileText,
						isExpanded: false,
						content: (
							<div className="mr-1">
								<ExplainPlanVisualizer
									explainResult={JSON.stringify({
										Plan: convertPlanNodeToExplainNode(queryData.explainResult.rootNode),
										"Planning Time": queryData.explainResult.planningTimeMs,
										"Execution Time": queryData.explainResult.executionTimeMs,
									})}
								/>
							</div>
						),
					},
				]
			: []),
		// Алгоритмические рекомендации (только если есть или загружаются)
		...(analyzeQueryMutation.isPending || combinedRecommendations.length > 0
			? [
					{
						id: "algorithm-recommendations",
						title: "Алгоритмические рекомендации",
						description: "Рекомендации на основе статического анализа и плана выполнения",
						icon: Zap,
						isExpanded: true,
						content: analyzeQueryMutation.isPending ? (
							<div className="text-muted-foreground">Анализируем запрос...</div>
						) : (
							<div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4 mr-1">
								{combinedRecommendations.map((rec, idx: number) => (
									<div key={idx} className="border rounded-lg p-4 flex flex-col">
										<div className="flex items-center gap-2 mb-2 flex-wrap">
											{getSeverityIcon(rec.severity)}
											<Badge variant={getSeverityVariant(rec.severity)}>
												{getSeverityText(rec.severity)}
											</Badge>
											<Badge variant="outline" className="text-xs">
												{rec.source === "static" ? "Статический анализ" : "Анализ плана"}
											</Badge>
										</div>
										{rec.problem && (
											<div className="mb-2 flex-1">
												<h4 className="font-medium mb-1">Проблема:</h4>
												<div className="text-sm text-muted-foreground">
													<TextWithSqlSnippets text={rec.problem} />
												</div>
											</div>
										)}
										{rec.recommendation && (
											<div className="flex-1">
												<h4 className="font-medium mb-1">Рекомендация:</h4>
												<div className="text-sm text-muted-foreground">
													<TextWithSqlSnippets text={rec.recommendation} />
												</div>
											</div>
										)}
									</div>
								))}
							</div>
						),
					},
				]
			: []),
		// ИИ рекомендации (только если есть или загружаются)
		...(analyzeQueryMutation.isPending || analysisData?.llmRecommendations
			? [
					{
						id: "ai-recommendations",
						title: "ИИ рекомендации",
						description: "Рекомендации с использованием ИИ",
						icon: Brain,
						isExpanded: true,
						content: analyzeQueryMutation.isPending ? (
							<div className="text-muted-foreground">Генерируем рекомендации...</div>
						) : (
							<div className="space-y-6">
								{analysisData?.llmRecommendations?.llmAnswer?.problems?.length ? (
									<div>
										<h4 className="font-medium mb-3 flex items-center gap-2">
											<AlertTriangle className="h-4 w-4 text-red-500" />
											Обнаруженные проблемы
										</h4>
										<div className="space-y-3">
											{analysisData?.llmRecommendations?.llmAnswer?.problems.map(
												(problem, idx) => (
													<div key={idx} className="border-l-4 border-red-500 pl-4">
														<div className="text-sm text-muted-foreground">
															<TextWithSqlSnippets text={problem.message || ""} />
														</div>
													</div>
												),
											)}
										</div>
									</div>
								) : null}

								{analysisData?.llmRecommendations?.llmAnswer?.recommendations?.length ? (
									<div>
										<h4 className="font-medium mb-3 flex items-center gap-2">
											<CheckCircle className="h-4 w-4 text-green-500" />
											Рекомендации по улучшению
										</h4>
										<div className="space-y-3">
											{analysisData?.llmRecommendations?.llmAnswer?.recommendations.map(
												(rec, idx) => (
													<div key={idx} className="border-l-4 border-green-500 pl-4">
														<div className="text-sm text-muted-foreground">
															<TextWithSqlSnippets text={rec.message || ""} />
														</div>
													</div>
												),
											)}
										</div>
									</div>
								) : null}
							</div>
						),
					},
				]
			: []),
		// Оптимизированный запрос (только если есть или загружается)
		...(analyzeQueryMutation.isPending || analysisData?.llmRecommendations?.llmAnswer?.newQuery
			? [
					{
						id: "optimized-query",
						title: "Оптимизированный запрос",
						description: "Улучшенная версия запроса ",
						icon: Zap,
						isExpanded: true,
						content: analyzeQueryMutation.isPending ? (
							<div className="text-muted-foreground">Оптимизируем запрос...</div>
						) : (
							<div className="space-y-4">
								{analysisData?.llmRecommendations?.llmAnswer?.newQueryAbout && (
									<Alert>
										<Lightbulb className="h-4 w-4" />
										<AlertDescription className="whitespace-pre-wrap">
											<TextWithSqlSnippets
												text={analysisData.llmRecommendations.llmAnswer.newQueryAbout}
											/>
										</AlertDescription>
									</Alert>
								)}
								<div className="bg-gray-900 rounded-lg overflow-hidden relative group">
									<CodeCopyButton
										code={analysisData?.llmRecommendations?.llmAnswer?.newQuery ?? ""}
										copyId={`optimized-${queryId}`}
										language="sql"
									/>
									<SyntaxHighlighter
										language="sql"
										style={oneDark}
										customStyle={{
											margin: 0,
											fontSize: "12px",
										}}
										showLineNumbers={true}
									>
										{formatSql(analysisData?.llmRecommendations?.llmAnswer?.newQuery || "")}
									</SyntaxHighlighter>
								</div>
							</div>
						),
					},
				]
			: []),
		// Сравнение производительности (только если есть данные для сравнения)
		...(analysisData?.explainComparisonDto
			? [
					{
						id: "performance-comparison",
						title: "Сравнение производительности",
						description: "Сравнение метрик оригинального и оптимизированного запросов",
						icon: CheckCircle,
						isExpanded: true,
						content: (
							<div className="mr-1">
								<QueryPerformanceComparison comparison={analysisData.explainComparisonDto} />
							</div>
						),
					},
				]
			: []),
	];

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

			<CollapsibleList items={queryAnalysisItems} className="space-y-4 px-2" />

			{analyzeQueryMutation.error && (
				<Alert variant="destructive">
					<AlertDescription>Ошибка анализа: {analyzeQueryMutation.error.message}</AlertDescription>
				</Alert>
			)}
		</div>
	);
}
