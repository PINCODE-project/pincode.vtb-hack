import { Card } from "@pin-code/ui-kit";
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from "@pin-code/ui-kit";
import { BarChart3 } from "lucide-react";
import type { AutovacuumMetricsSummary } from "@/generated/models/AutovacuumMetricsSummary";
import type { CacheMetricsSummary } from "@/generated/models/CacheMetricsSummary";
import type { TempFilesMetricsSummary } from "@/generated/models/TempFilesMetricsSummary";
import type { IndexUsageStatistics } from "@/generated/models/IndexUsageStatistics";
import type { LockAnalysisResult } from "@/generated/models/LockAnalysisResult";
import { AutovacuumMetrics, CacheMetrics, TempFilesMetrics, IndexMetrics, LockMetrics } from "../Metrics";

interface MetricsDetailsProps {
	data:
		| AutovacuumMetricsSummary
		| CacheMetricsSummary
		| TempFilesMetricsSummary
		| IndexUsageStatistics
		| LockAnalysisResult;
	title: string;
	type: "autovacuum" | "cache" | "tempFiles" | "index" | "locks";
}

/**
 * Компонент для отображения детальных метрик в аккордеоне
 */
export function MetricsDetails({ data, title, type }: MetricsDetailsProps) {
	if (!data) return null;

	const renderContent = () => {
		switch (type) {
			case "autovacuum":
				return <AutovacuumMetrics metrics={data as AutovacuumMetricsSummary} />;
			case "cache":
				return <CacheMetrics metrics={data as CacheMetricsSummary} />;
			case "tempFiles":
				return <TempFilesMetrics metrics={data as TempFilesMetricsSummary} />;
			case "index":
				return <IndexMetrics metrics={data as IndexUsageStatistics} />;
			case "locks":
				return <LockMetrics metrics={data as LockAnalysisResult} />;
			default:
				return <pre className="text-xs overflow-auto max-h-48">{JSON.stringify(data, null, 2)}</pre>;
		}
	};

	return (
		<Card className="mt-4">
			<Accordion type="single" collapsible className="w-full">
				<AccordionItem value="metrics" className="border-none">
					<AccordionTrigger className="hover:no-underline px-4 py-3">
						<div className="flex items-center space-x-2">
							<BarChart3 className="h-4 w-4 text-muted-foreground" />
							<span className="text-sm font-medium">{title}</span>
						</div>
					</AccordionTrigger>
					<AccordionContent className="px-4 pt-4 pb-4">{renderContent()}</AccordionContent>
				</AccordionItem>
			</Accordion>
		</Card>
	);
}
