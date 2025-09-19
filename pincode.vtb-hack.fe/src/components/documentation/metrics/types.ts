export interface MetricItem {
	title: string;
	metric?: string;
	threshold?: string;
	level?: "critical" | "high" | "medium" | "low";
	description: string;
	recommendations: string[];
}

export interface MetricSection {
	title: string;
	items: MetricItem[];
}
