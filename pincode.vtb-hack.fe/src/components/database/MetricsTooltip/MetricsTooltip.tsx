import { Button } from "@pin-code/ui-kit";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@pin-code/ui-kit";
import { Info } from "lucide-react";
import type { AutovacuumMetricsSummary } from "@/generated/models/AutovacuumMetricsSummary";
import type { CacheMetricsSummary } from "@/generated/models/CacheMetricsSummary";
import type { TempFilesMetricsSummary } from "@/generated/models/TempFilesMetricsSummary";
import type { IndexUsageStatistics } from "@/generated/models/IndexUsageStatistics";
import { AutovacuumMetrics, CacheMetrics, TempFilesMetrics, IndexMetrics } from "../Metrics";

interface MetricsTooltipProps {
	data: AutovacuumMetricsSummary | CacheMetricsSummary | TempFilesMetricsSummary | IndexUsageStatistics;
	title: string;
	type: "autovacuum" | "cache" | "tempFiles" | "index";
}

/**
 * Компонент для отображения тултипа с метриками
 */
export function MetricsTooltip({ data, title, type }: MetricsTooltipProps) {
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
			default:
				return <pre className="text-xs overflow-auto max-h-48">{JSON.stringify(data, null, 2)}</pre>;
		}
	};

	return (
		<TooltipProvider>
			<Tooltip>
				<TooltipTrigger asChild>
					<Button variant="ghost" size="sm" className="h-6 w-6 p-0">
						<Info className="h-4 w-4" />
					</Button>
				</TooltipTrigger>
				<TooltipContent className="metrics-tooltip max-w-4xl">
					<div className="p-3">
						<h4 className="font-medium mb-3">{title}</h4>
						{renderContent()}
					</div>
				</TooltipContent>
			</Tooltip>
		</TooltipProvider>
	);
}
