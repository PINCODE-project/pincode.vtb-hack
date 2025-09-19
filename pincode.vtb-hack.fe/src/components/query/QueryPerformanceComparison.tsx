"use client";
import React from "react";
import { Badge, Card, CardContent, cn } from "@pin-code/ui-kit";
import { Minus, TrendingDown, TrendingUp } from "lucide-react";
import type { PlanComparisonDto, PlanPointComparsionResult } from "@/generated/models";

interface QueryPerformanceComparisonProps {
	comparison: PlanComparisonDto | null | undefined;
}

// Компонент для отображения одной метрики сравнения
const MetricComparison: React.FC<{
	title: string;
	unit: string;
	metric: PlanPointComparsionResult | undefined;
}> = ({ title, unit, metric }) => {
	if (!metric || metric.old === undefined || metric.new === undefined) {
		return null;
	}

	const difference = metric.differencePercent;
	const hasPercentChange = difference !== null && Math.abs(difference || 0) > 0.01; // Учитываем погрешность
	const hasValueChange = metric.old !== metric.new; // Проверяем изменение значений

	// Вычисляем процент самостоятельно если сервер не предоставил
	const getCalculatedPercent = () => {
		if (hasPercentChange) return difference!;
		if (!hasValueChange) return 0;
		if (metric.old === 0) return metric.new! > 0 ? Infinity : 0;
		return ((metric.new! - metric.old!) / metric.old!) * 100;
	};

	const calculatedPercent = getCalculatedPercent();

	// Определяем тип изменения: положительный процент = зеленый, отрицательный = красный
	let changeType: "positive" | "negative" | "neutral" = "neutral";
	if (hasValueChange) {
		changeType = calculatedPercent > 0 ? "positive" : "negative";
	}

	const formatNumber = (num: number) => {
		if (num >= 1000000) {
			return (num / 1000000).toFixed(1) + "M";
		}
		if (num >= 1000) {
			return (num / 1000).toFixed(1) + "K";
		}
		return num.toFixed(2);
	};

	const getBadgeIcon = () => {
		if (!hasValueChange)
			return <Minus className="mr-0.5 -ml-1 h-4 w-4 shrink-0 self-center text-muted-foreground" />;

		if (changeType === "positive") {
			return <TrendingUp className="mr-0.5 -ml-1 h-4 w-4 shrink-0 self-center text-green-500" />;
		} else {
			return <TrendingDown className="mr-0.5 -ml-1 h-4 w-4 shrink-0 self-center text-red-500" />;
		}
	};

	const getBadgeText = () => {
		if (!hasValueChange) return "";
		if (calculatedPercent === Infinity) return "+∞%";
		return `${calculatedPercent > 0 ? "+" : ""}${calculatedPercent.toFixed(1)}%`;
	};

	return (
		<Card className="p-6 py-4 w-full">
			<CardContent className="p-0">
				<div className="flex items-center justify-between">
					<dt className="text-sm font-medium text-muted-foreground">{title}</dt>
					<Badge
						variant="outline"
						className={cn(
							"font-medium inline-flex items-center px-1.5 ps-2.5 py-0.5 text-xs",
							changeType === "positive"
								? "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400"
								: changeType === "negative"
									? "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400"
									: "bg-muted text-muted-foreground",
						)}
					>
						{getBadgeIcon()}
						<span className="sr-only">
							{changeType === "positive"
								? "Увеличилось"
								: changeType === "negative"
									? "Уменьшилось"
									: "Без изменений"}{" "}
							{hasPercentChange ? "на" : ""}
						</span>
						{getBadgeText()}
					</Badge>
				</div>
				<dd className="text-xl font-semibold text-foreground mt-2">
					{formatNumber(metric.old)} {unit} → {formatNumber(metric.new)} {unit}
				</dd>
			</CardContent>
		</Card>
	);
};

// Компонент для отображения типов JOIN
const JoinTypesComparison: React.FC<{
	oldJoinTypes: string | null | undefined;
	newJoinTypes: string | null | undefined;
}> = ({ oldJoinTypes, newJoinTypes }) => {
	if (!oldJoinTypes && !newJoinTypes) {
		return null;
	}

	const hasChange = oldJoinTypes !== newJoinTypes;

	return (
		<Card className="p-6 py-4 w-full">
			<CardContent className="p-0">
				<div className="flex items-center justify-between">
					<dt className="text-sm font-medium text-muted-foreground">Типы соединений (JOIN)</dt>
					<Badge
						variant="outline"
						className={cn(
							"font-medium inline-flex items-center px-1.5 ps-2.5 py-0.5 text-xs",
							hasChange
								? "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400"
								: "bg-muted text-muted-foreground",
						)}
					>
						<Minus className="mr-0.5 -ml-1 h-4 w-4 shrink-0 self-center text-muted-foreground" />
						<span className="sr-only">{hasChange ? "Изменены" : "Без изменений"}</span>
						{hasChange ? "Изменены" : "Без изменений"}
					</Badge>
				</div>
				<dd className="text-xl font-semibold text-foreground mt-2">
					{oldJoinTypes || "Не определено"} → {newJoinTypes || "Не определено"}
				</dd>
			</CardContent>
		</Card>
	);
};

export const QueryPerformanceComparison: React.FC<QueryPerformanceComparisonProps> = ({ comparison }) => {
	if (!comparison) {
		return null;
	}

	const hasAnyMetrics =
		comparison.cost ||
		comparison.rows ||
		comparison.width ||
		comparison.seqScanCount ||
		comparison.nodeCount ||
		comparison.oldJoinTypes ||
		comparison.newJoinTypes;

	if (!hasAnyMetrics) {
		return (
			<div className="text-center py-8 text-muted-foreground">
				<TrendingUp className="h-8 w-8 mx-auto mb-2" />
				<p>Данные сравнения недоступны</p>
			</div>
		);
	}

	return (
		<div className="space-y-6">
			<div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
				<MetricComparison title="Стоимость" unit="" metric={comparison.cost} />
				<MetricComparison title="Количество строк" unit="" metric={comparison.rows} />
				<MetricComparison title="Ширина строки" unit="байт" metric={comparison.width} />
				<MetricComparison title="Количество Seq Scan" unit="" metric={comparison.seqScanCount} />
				<MetricComparison title="Количество узлов" unit="" metric={comparison.nodeCount} />
				{(comparison.oldJoinTypes || comparison.newJoinTypes) && (
					<JoinTypesComparison
						oldJoinTypes={comparison.oldJoinTypes}
						newJoinTypes={comparison.newJoinTypes}
					/>
				)}
			</div>
		</div>
	);
};
