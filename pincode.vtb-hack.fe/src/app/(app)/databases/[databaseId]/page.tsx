"use client";

import React from "react";
import { useParams } from "next/navigation";
import { Accordion } from "@pin-code/ui-kit";

import { useGetApiAutovacuumAnalysis } from "@/generated/hooks/AutovacuumAnalysis/useGetApiAutovacuumAnalysis";
import { useGetApiCacheAnalysis } from "@/generated/hooks/CacheAnalysis/useGetApiCacheAnalysis";
import { useGetApiIndexRecommendations } from "@/generated/hooks/IndexAnalyze/useGetApiIndexRecommendations";
import { useGetTempFileAnalysis } from "@/generated/hooks/TempFilesAnalyzeMonitoring/useGetTempFileAnalysis";
import { useGetApiAutovacuumTime } from "@/generated/hooks/AutovacuumAnalysis/useGetApiAutovacuumTime";
import { useGetApiCacheTime } from "@/generated/hooks/CacheAnalysis/useGetApiCacheTime";
import { useGetApiIndexTime } from "@/generated/hooks/IndexAnalyze/useGetApiIndexTime";
import { useGetTempFileTime } from "@/generated/hooks/TempFilesAnalyzeMonitoring/useGetTempFileTime";
import { useGetApiLockTime } from "@/generated/hooks/PgLockAnalysis/useGetApiLockTime";
import { useGetApiLockAnalysis } from "@/generated/hooks/PgLockAnalysis/useGetApiLockAnalysis";

import {
	AutovacuumAnalysisSection,
	CacheAnalysisSection,
	IndexAnalysisSection,
	TempFilesAnalysisSection,
	LockAnalysisSection,
} from "@/components/database";
import { DatabasePeriodSelector } from "@/components/DatabasePeriodSelector";
import type { DateRange } from "@/components/DatabasePeriodSelector";

/**
 * Страница базы данных с анализом и рекомендациями по оптимизации
 */
export default function DatabasePage() {
	const params = useParams();
	const databaseId = params.databaseId as string;

	// Состояние для выбранного периода
	const [selectedPeriod, setSelectedPeriod] = React.useState<DateRange>(() => {
		const now = new Date();
		return {
			start: new Date(now.getTime() - 6 * 60 * 60 * 1000), // 6 часов назад
			end: now,
		};
	});

	// Получаем данные о различных аспектах БД с учетом выбранного периода
	const autovacuumQuery = useGetApiAutovacuumAnalysis({
		dbConnectionId: databaseId,
		startDate: selectedPeriod.start.toISOString(),
		endDate: selectedPeriod.end.toISOString(),
	});
	const cacheQuery = useGetApiCacheAnalysis({
		dbConnectionId: databaseId,
		startDate: selectedPeriod.start.toISOString(),
		endDate: selectedPeriod.end.toISOString(),
	});
	const indexQuery = useGetApiIndexRecommendations({
		dbConnectionId: databaseId,
		startDate: selectedPeriod.start.toISOString(),
		endDate: selectedPeriod.end.toISOString(),
	});
	const tempFilesQuery = useGetTempFileAnalysis({
		dbConnectionId: databaseId,
		startDate: selectedPeriod.start.toISOString(),
		endDate: selectedPeriod.end.toISOString(),
	});
	const lockQuery = useGetApiLockAnalysis({
		dbConnectionId: databaseId,
		startDate: selectedPeriod.start.toISOString(),
		endDate: selectedPeriod.end.toISOString(),
	});

	// Получаем временные метки для каждого типа данных
	const autovacuumTimeQuery = useGetApiAutovacuumTime({ dbConnectionId: databaseId });
	const cacheTimeQuery = useGetApiCacheTime({ dbConnectionId: databaseId });
	const indexTimeQuery = useGetApiIndexTime({ dbConnectionId: databaseId });
	const tempFileTimeQuery = useGetTempFileTime({ dbConnectionId: databaseId });
	const lockTimeQuery = useGetApiLockTime({ dbConnectionId: databaseId });

	// Собираем временные данные для селектора периода
	const timeData = React.useMemo(
		() => ({
			autovacuum: autovacuumTimeQuery.data,
			cache: cacheTimeQuery.data,
			index: indexTimeQuery.data,
			tempfiles: tempFileTimeQuery.data,
			locks: lockTimeQuery.data,
		}),
		[
			autovacuumTimeQuery.data,
			cacheTimeQuery.data,
			indexTimeQuery.data,
			tempFileTimeQuery.data,
			lockTimeQuery.data,
		],
	);

	return (
		<div className="space-y-6 min-h-[100svh] flex flex-col">
			<div className="p-6 mb-6 flex justify-between items-start">
				<div>
					<h1 className="text-3xl font-bold">Анализ базы данных</h1>
					<p className="text-muted-foreground mt-2">
						Рекомендации по оптимизации и улучшению производительности
					</p>
				</div>
			</div>

			{/* Селектор периода данных - на всю ширину */}
			<DatabasePeriodSelector
				selectedRange={selectedPeriod}
				onRangeChange={setSelectedPeriod}
				timeData={timeData}
				disabled={
					autovacuumTimeQuery.isLoading ||
					cacheTimeQuery.isLoading ||
					indexTimeQuery.isLoading ||
					tempFileTimeQuery.isLoading ||
					lockTimeQuery.isLoading
				}
			/>

			{/* Секции анализа с отступами */}
			<div className="container mx-auto px-6">
				<Accordion type="multiple" className="space-y-4">
					{/* Обновленные секции анализа */}
					<AutovacuumAnalysisSection query={autovacuumQuery} />
					<CacheAnalysisSection query={cacheQuery} />
					<LockAnalysisSection query={lockQuery} />
					<IndexAnalysisSection query={indexQuery} />
					<TempFilesAnalysisSection query={tempFilesQuery} />
				</Accordion>
			</div>
		</div>
	);
}
