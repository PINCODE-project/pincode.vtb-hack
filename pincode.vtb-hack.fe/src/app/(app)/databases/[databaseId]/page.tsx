"use client";

import { useParams } from "next/navigation";
import { Accordion } from "@pin-code/ui-kit";

import { useGetApiAutovacuumAnalysis } from "@/generated/hooks/AutovacuumAnalysis/useGetApiAutovacuumAnalysis";
import { useGetApiCacheAnalysis } from "@/generated/hooks/CacheAnalysis/useGetApiCacheAnalysis";
import { useGetApiIndexRecommendations } from "@/generated/hooks/IndexAnalyze/useGetApiIndexRecommendations";
import { useGetTempFileAnalysis } from "@/generated/hooks/TempFilesAnalyzeMonitoring/useGetTempFileAnalysis";

import {
	AutovacuumAnalysisSection,
	CacheAnalysisSection,
	IndexAnalysisSection,
	TempFilesAnalysisSection,
} from "@/components/database";

/**
 * Страница базы данных с анализом и рекомендациями по оптимизации
 */
export default function DatabasePage() {
	const params = useParams();
	const databaseId = params.databaseId as string;

	// Получаем данные о различных аспектах БД
	const autovacuumQuery = useGetApiAutovacuumAnalysis({ dbConnectionId: databaseId });
	const cacheQuery = useGetApiCacheAnalysis({ dbConnectionId: databaseId });
	const indexQuery = useGetApiIndexRecommendations({ dbConnectionId: databaseId });
	const tempFilesQuery = useGetTempFileAnalysis({ dbConnectionId: databaseId });

	return (
		<div className="container mx-auto p-6 space-y-6">
			<div className="flex items-center justify-between">
				<div>
					<h1 className="text-3xl font-bold tracking-tight">Анализ базы данных</h1>
					<p className="text-muted-foreground">Рекомендации по оптимизации и улучшению производительности</p>
				</div>
			</div>

			<Accordion type="multiple" className="space-y-4">
				{/* Обновленные секции анализа */}
				<AutovacuumAnalysisSection query={autovacuumQuery} />
				<CacheAnalysisSection query={cacheQuery} />
				<IndexAnalysisSection query={indexQuery} />
				<TempFilesAnalysisSection query={tempFilesQuery} />
			</Accordion>
		</div>
	);
}
