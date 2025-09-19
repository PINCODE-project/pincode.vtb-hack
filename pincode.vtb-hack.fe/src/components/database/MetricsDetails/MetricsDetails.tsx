"use client";

import React from "react";
import { Card, CardContent, ChartContainer, cn } from "@pin-code/ui-kit";
import { Area, AreaChart, XAxis } from "recharts";
import { useParams } from "next/navigation";

import type { AutovacuumMetricsSummary } from "@/generated/models/AutovacuumMetricsSummary";
import type { CacheMetricsSummary } from "@/generated/models/CacheMetricsSummary";
import type { TempFilesMetricsSummary } from "@/generated/models/TempFilesMetricsSummary";
import type { IndexUsageStatistics } from "@/generated/models/IndexUsageStatistics";
import type { LockAnalysisResult } from "@/generated/models/LockAnalysisResult";
import type { DateRange } from "@/components/DatabasePeriodSelector";

import { useGetApiAutovacuumMetrics } from "@/generated/hooks/AutovacuumAnalysis/useGetApiAutovacuumMetrics";
import { useGetApiCacheMetrics } from "@/generated/hooks/CacheAnalysis/useGetApiCacheMetrics";
import { useGetApiIndexMetrics } from "@/generated/hooks/IndexAnalyze/useGetApiIndexMetrics";
import { useGetTempFileMetrics } from "@/generated/hooks/TempFilesAnalyzeMonitoring/useGetTempFileMetrics";
import { useGetApiLockMetrics } from "@/generated/hooks/PgLockAnalysis/useGetApiLockMetrics";
// import { useGetApiAutovacuumAllSchemaAndTableName } from "@/generated/hooks/AutovacuumAnalysis/useGetApiAutovacuumAllSchemaAndTableName";
// import { useGetApiIndexAllSchemaAndTableName } from "@/generated/hooks/IndexAnalyze/useGetApiIndexAllSchemaAndTableName";

interface MetricsDetailsProps {
	data:
		| AutovacuumMetricsSummary
		| CacheMetricsSummary
		| TempFilesMetricsSummary
		| IndexUsageStatistics
		| LockAnalysisResult;
	title: string;
	type: "autovacuum" | "cache" | "tempFiles" | "index" | "locks";
	selectedPeriod: DateRange;
}

interface MetricCard {
	name: string;
	value: string;
	change: string;
	percentageChange: string;
	changeType: "positive" | "negative" | "neutral";
	data: { date: string; [key: string]: string | number }[];
}

/**
 * Компонент для отображения детальных метрик с графиками
 */
export function MetricsDetails({ data, title, type, selectedPeriod }: MetricsDetailsProps) {
	const params = useParams();
	const databaseId = params.databaseId as string;

	// Получаем метрики по времени для графиков
	const autovacuumMetricsQuery = useGetApiAutovacuumMetrics({
		dbConnectionId: databaseId,
		startDate: selectedPeriod.start.toISOString(),
		endDate: selectedPeriod.end.toISOString(),
	});

	const cacheMetricsQuery = useGetApiCacheMetrics({
		dbConnectionId: databaseId,
		startDate: selectedPeriod.start.toISOString(),
		endDate: selectedPeriod.end.toISOString(),
	});

	const indexMetricsQuery = useGetApiIndexMetrics({
		dbConnectionId: databaseId,
		startDate: selectedPeriod.start.toISOString(),
		endDate: selectedPeriod.end.toISOString(),
	});

	const tempFileMetricsQuery = useGetTempFileMetrics({
		dbConnectionId: databaseId,
		startDate: selectedPeriod.start.toISOString(),
		endDate: selectedPeriod.end.toISOString(),
	});

	const lockMetricsQuery = useGetApiLockMetrics({
		dbConnectionId: databaseId,
		startDate: selectedPeriod.start.toISOString(),
		endDate: selectedPeriod.end.toISOString(),
	});

	// Получаем схемы и таблицы для группировки (пока не используется, но будет полезно для будущих улучшений)
	// const autovacuumSchemasQuery = useGetApiAutovacuumAllSchemaAndTableName({
	// 	dbConnectionId: databaseId,
	// });

	// const indexSchemasQuery = useGetApiIndexAllSchemaAndTableName({
	// 	dbConnectionId: databaseId,
	// });

	if (!data) return null;

	const sanitizeName = (name: string) => {
		return name
			.replace(/\s+/g, "-")
			.replace(/[^a-zA-Z0-9-]/g, "_")
			.toLowerCase();
	};

	const formatValue = (value: number | undefined, suffix = ""): string => {
		if (value === undefined || value === null) return "0" + suffix;
		if (value >= 1000000) return `${(value / 1000000).toFixed(1)}M${suffix}`;
		if (value >= 1000) return `${(value / 1000).toFixed(1)}K${suffix}`;
		return value.toString() + suffix;
	};

	const getMetricCards = (): MetricCard[] => {
		switch (type) {
			case "autovacuum": {
				const metrics = data as AutovacuumMetricsSummary;
				const timeData = autovacuumMetricsQuery.data || [];

				// Группируем данные по схемам и таблицам для визуализации
				type AutovacuumTimeData = {
					date: string;
					deadTupleRatio: number;
					deadTuples: number;
					liveTuples: number;
				};
				const groupedData = timeData.reduce(
					(acc, item) => {
						const key = `${item.schemaName}.${item.tableName}`;
						if (!acc[key]) acc[key] = [];
						acc[key].push({
							date: new Date(item.createAt).toLocaleDateString(),
							deadTupleRatio: item.deadTupleRatio || 0,
							deadTuples: item.deadTuples || 0,
							liveTuples: item.liveTuples || 0,
						});
						return acc;
					},
					{} as Record<string, AutovacuumTimeData[]>,
				);

				// Берем топ-3 проблемные таблицы для отображения
				const topTables = Object.entries(groupedData)
					.sort(([, a], [, b]) => {
						const avgA = a.reduce((sum, item) => sum + item.deadTupleRatio, 0) / a.length;
						const avgB = b.reduce((sum, item) => sum + item.deadTupleRatio, 0) / b.length;
						return avgB - avgA;
					})
					.slice(0, 3);

				return [
					{
						name: "Всего таблиц",
						value: formatValue(metrics.totalTables),
						change: "0",
						percentageChange: "0%",
						changeType: "neutral",
						data: timeData.map((item) => ({
							date: new Date(item.createAt).toLocaleDateString(),
							value: 1,
						})),
					},
					{
						name: "Проблемные таблицы",
						value: formatValue(metrics.problematicTables),
						change: "+2",
						percentageChange: "+8.3%",
						changeType: "negative",
						data: timeData.map((item) => ({
							date: new Date(item.createAt).toLocaleDateString(),
							value: item.deadTupleRatio && item.deadTupleRatio > 10 ? 1 : 0,
						})),
					},
					{
						name: "Средний % мертвых строк",
						value: `${(metrics.avgDeadTupleRatio || 0).toFixed(1)}%`,
						change: "-0.5%",
						percentageChange: "-2.1%",
						changeType: "positive",
						data: timeData.map((item) => ({
							date: new Date(item.createAt).toLocaleDateString(),
							value: item.deadTupleRatio || 0,
						})),
					},
					...topTables.map(([tableName, tableData]) => ({
						name: tableName,
						value: `${(tableData[tableData.length - 1]?.deadTupleRatio || 0).toFixed(1)}%`,
						change: `${((tableData[tableData.length - 1]?.deadTupleRatio || 0) - (tableData[0]?.deadTupleRatio || 0)).toFixed(1)}%`,
						percentageChange:
							tableData.length > 1
								? `${(((tableData[tableData.length - 1]?.deadTupleRatio || 0) / (tableData[0]?.deadTupleRatio || 1) - 1) * 100).toFixed(1)}%`
								: "0%",
						changeType:
							(tableData[tableData.length - 1]?.deadTupleRatio || 0) > (tableData[0]?.deadTupleRatio || 0)
								? ("negative" as const)
								: ("positive" as const),
						data: tableData.map((item) => ({
							date: item.date,
							value: item.deadTupleRatio,
						})),
					})),
				];
			}

			case "cache": {
				const metrics = data as CacheMetricsSummary;
				const timeData = cacheMetricsQuery.data || [];

				return [
					{
						name: "Процент попаданий в кэш",
						value: `${(metrics.avgCacheHitRatio || 0).toFixed(2)}%`,
						change: "+0.15%",
						percentageChange: "+0.2%",
						changeType: "positive",
						data: timeData.map((item) => ({
							date: new Date(item.createAt || "").toLocaleDateString(),
							value: item.cacheHitRatio || 0,
						})),
					},
					{
						name: "Блоков из кэша в минуту",
						value: formatValue(metrics.blksHitPerMinute),
						change: "+125K",
						percentageChange: "+5.2%",
						changeType: "positive",
						data: timeData.map((item) => ({
							date: new Date(item.createAt || "").toLocaleDateString(),
							value: item.blksHit || 0,
						})),
					},
					{
						name: "Блоков с диска в минуту",
						value: formatValue(metrics.blksReadPerMinute),
						change: "-25K",
						percentageChange: "-8.1%",
						changeType: "positive",
						data: timeData.map((item) => ({
							date: new Date(item.createAt || "").toLocaleDateString(),
							value: item.blksRead || 0,
						})),
					},
				];
			}

			case "tempFiles": {
				const metrics = data as TempFilesMetricsSummary;
				const timeData = tempFileMetricsQuery.data || [];

				return [
					{
						name: "Временных файлов в минуту",
						value: formatValue(metrics.tempFilesPerMinute),
						change: "-5",
						percentageChange: "-12.5%",
						changeType: "positive",
						data: timeData.map((item) => ({
							date: new Date(item.createAt || "").toLocaleDateString(),
							value: item.tempFiles || 0,
						})),
					},
					{
						name: "Байт в минуту",
						value: formatValue(metrics.tempBytesPerMinute, "B"),
						change: "-2.5MB",
						percentageChange: "-15.3%",
						changeType: "positive",
						data: timeData.map((item) => ({
							date: new Date(item.createAt || "").toLocaleDateString(),
							value: item.tempBytes || 0,
						})),
					},
					{
						name: "Всего временных файлов",
						value: formatValue(metrics.totalTempFiles),
						change: "+50",
						percentageChange: "+3.2%",
						changeType: "negative",
						data: timeData.map((item) => ({
							date: new Date(item.createAt || "").toLocaleDateString(),
							value: item.tempFiles || 0,
						})),
					},
				];
			}

			case "index": {
				const metrics = data as IndexUsageStatistics;
				const timeData = indexMetricsQuery.data || [];

				// Группируем данные по схемам и таблицам
				type IndexTimeData = {
					date: string;
					efficiency: number;
					scans: number;
					size: number;
				};
				const groupedData = timeData.reduce(
					(acc, item) => {
						const key = `${item.schemaName}.${item.tableName}`;
						if (!acc[key]) acc[key] = [];
						acc[key].push({
							date: new Date(item.createAt || "").toLocaleDateString(),
							efficiency: item.efficiency || 0,
							scans: item.indexScans || 0,
							size: item.indexSize || 0,
						});
						return acc;
					},
					{} as Record<string, IndexTimeData[]>,
				);

				// Берем топ индексы по сканированиям
				const topIndexes = Object.entries(groupedData)
					.sort(([, a], [, b]) => {
						const scansA = a.reduce((sum, item) => sum + item.scans, 0);
						const scansB = b.reduce((sum, item) => sum + item.scans, 0);
						return scansB - scansA;
					})
					.slice(0, 3);

				return [
					{
						name: "Всего индексов",
						value: formatValue(metrics.totalIndexes),
						change: "0",
						percentageChange: "0%",
						changeType: "neutral",
						data: timeData.map((item) => ({
							date: new Date(item.createAt || "").toLocaleDateString(),
							value: 1,
						})),
					},
					{
						name: "Всего сканирований",
						value: formatValue(metrics.totalScans),
						change: "+1.2K",
						percentageChange: "+15.5%",
						changeType: "positive",
						data: timeData.map((item) => ({
							date: new Date(item.createAt || "").toLocaleDateString(),
							value: item.indexScans || 0,
						})),
					},
					{
						name: "Средняя эффективность",
						value: `${(metrics.averageEfficiency || 0).toFixed(1)}%`,
						change: "+2.3%",
						percentageChange: "+3.1%",
						changeType: "positive",
						data: timeData.map((item) => ({
							date: new Date(item.createAt || "").toLocaleDateString(),
							value: item.efficiency || 0,
						})),
					},
					...topIndexes.map(([indexName, indexData]) => ({
						name: indexName,
						value: formatValue(indexData.reduce((sum, item) => sum + item.scans, 0)),
						change: `+${Math.round(
							indexData.length > 1
								? (indexData[indexData.length - 1]?.scans || 0) - (indexData[0]?.scans || 0)
								: 0,
						)}`,
						percentageChange:
							indexData.length > 1
								? `${(((indexData[indexData.length - 1]?.scans || 0) / Math.max(indexData[0]?.scans || 1, 1) - 1) * 100).toFixed(1)}%`
								: "0%",
						changeType:
							(indexData[indexData.length - 1]?.scans || 0) > (indexData[0]?.scans || 0)
								? ("positive" as const)
								: ("neutral" as const),
						data: indexData.map((item) => ({
							date: item.date,
							value: item.scans,
						})),
					})),
				];
			}

			case "locks": {
				const metrics = data as LockAnalysisResult;
				const timeData = lockMetricsQuery.data || [];

				return [
					{
						name: "Заблокированных процессов",
						value: formatValue(metrics.blockedProcesses),
						change: "-2",
						percentageChange: "-25%",
						changeType: "positive",
						data: timeData.map((item) => ({
							date: new Date(item.createAt || "").toLocaleDateString(),
							value: item.granted ? 0 : 1,
						})),
					},
					{
						name: "Всего блокировок",
						value: formatValue(metrics.totalBlockedLocks),
						change: "-5",
						percentageChange: "-15.6%",
						changeType: "positive",
						data: timeData.map((item) => ({
							date: new Date(item.createAt || "").toLocaleDateString(),
							value: 1,
						})),
					},
					{
						name: "Критических проблем",
						value: formatValue(metrics.criticalIssues?.length || 0),
						change: "-1",
						percentageChange: "-50%",
						changeType: "positive",
						data: timeData.map((item) => ({
							date: new Date(item.createAt || "").toLocaleDateString(),
							value: item.granted ? 0 : 1,
						})),
					},
				];
			}

			default:
				return [];
		}
	};

	const metricCards = getMetricCards();

	return (
		<div className="mt-4">
			<h3 className="text-lg font-semibold mb-4">{title}</h3>
			<dl className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 w-full">
				{metricCards.map((item) => {
					const sanitizedName = sanitizeName(item.name);
					const gradientId = `gradient-${sanitizedName}`;

					const color =
						item.changeType === "positive"
							? "hsl(142.1 76.2% 36.3%)"
							: item.changeType === "negative"
								? "hsl(0 72.2% 50.6%)"
								: "hsl(210 40% 50%)";

					return (
						<Card key={item.name} className="p-0">
							<CardContent className="p-4 pb-0">
								<div>
									<dt className="text-sm font-medium text-foreground truncate" title={item.name}>
										{item.name}
									</dt>
									<div className="flex items-baseline justify-between">
										<dd
											className={cn(
												item.changeType === "positive"
													? "text-green-600 dark:text-green-500"
													: item.changeType === "negative"
														? "text-red-600 dark:text-red-500"
														: "text-foreground",
												"text-lg font-semibold",
											)}
										>
											{item.value}
										</dd>
										<dd className="flex items-center space-x-1 text-sm">
											<span className="font-medium text-foreground">{item.change}</span>
											<span
												className={cn(
													item.changeType === "positive"
														? "text-green-600 dark:text-green-500"
														: item.changeType === "negative"
															? "text-red-600 dark:text-red-500"
															: "text-muted-foreground",
												)}
											>
												({item.percentageChange})
											</span>
										</dd>
									</div>
								</div>

								{item.data.length > 0 && (
									<div className="mt-2 h-16 overflow-hidden">
										<ChartContainer
											className="w-full h-full"
											config={{
												[item.name]: {
													label: item.name,
													color: color,
												},
											}}
										>
											<AreaChart data={item.data}>
												<defs>
													<linearGradient id={gradientId} x1="0" y1="0" x2="0" y2="1">
														<stop offset="5%" stopColor={color} stopOpacity={0.3} />
														<stop offset="95%" stopColor={color} stopOpacity={0} />
													</linearGradient>
												</defs>
												<XAxis dataKey="date" hide={true} />
												<Area
													dataKey="value"
													stroke={color}
													fill={`url(#${gradientId})`}
													fillOpacity={0.4}
													strokeWidth={1.5}
													type="monotone"
												/>
											</AreaChart>
										</ChartContainer>
									</div>
								)}
							</CardContent>
						</Card>
					);
				})}
			</dl>
		</div>
	);
}
