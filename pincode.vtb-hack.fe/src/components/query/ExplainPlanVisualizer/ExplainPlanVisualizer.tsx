import React from "react";
import { Badge } from "@pin-code/ui-kit";
import { Clock, Filter, Hash, RotateCcw, Search, Shuffle, SortAsc, Table, TrendingUp, Zap } from "lucide-react";

// Компонент маленькой круговой диаграммы
const CircularProgress: React.FC<{ value: number; size?: number; strokeWidth?: number; className?: string }> = ({
	value,
	size = 16,
	strokeWidth = 2,
	className = "",
}) => {
	const radius = (size - strokeWidth) / 2;
	const circumference = radius * 2 * Math.PI;
	const strokeDasharray = circumference;
	const strokeDashoffset = circumference - (value / 100) * circumference;

	return (
		<div className={`inline-flex items-center justify-center ${className}`}>
			<svg width={size} height={size} className="transform -rotate-90">
				{/* Фоновый круг */}
				<circle
					cx={size / 2}
					cy={size / 2}
					r={radius}
					stroke="currentColor"
					strokeWidth={strokeWidth}
					fill="none"
					className="text-muted-foreground/20"
				/>
				{/* Прогресс */}
				<circle
					cx={size / 2}
					cy={size / 2}
					r={radius}
					stroke="currentColor"
					strokeWidth={strokeWidth}
					fill="none"
					strokeDasharray={strokeDasharray}
					strokeDashoffset={strokeDashoffset}
					strokeLinecap="round"
					className={
						value < 25
							? "text-green-500"
							: value < 50
								? "text-yellow-500"
								: value < 75
									? "text-orange-500"
									: "text-red-500"
					}
				/>
			</svg>
		</div>
	);
};

// Типы для узлов плана выполнения PostgreSQL
interface ExplainNode {
	"Node Type": string;
	"Startup Cost"?: number;
	"Total Cost"?: number;
	"Plan Rows"?: number;
	"Plan Width"?: number;
	"Actual Startup Time"?: number;
	"Actual Total Time"?: number;
	"Actual Rows"?: number;
	"Actual Loops"?: number;
	"Relation Name"?: string;
	"Index Name"?: string;
	"Sort Key"?: string[];
	"Hash Cond"?: string;
	"Join Type"?: string;
	Filter?: string;
	Plans?: ExplainNode[];
	[key: string]: any;
}

interface ExplainResult {
	Plan: ExplainNode;
	"Planning Time"?: number;
	"Execution Time"?: number;
}

interface ExplainPlanVisualizerProps {
	explainResult: string;
}

export const ExplainPlanVisualizer: React.FC<ExplainPlanVisualizerProps> = ({ explainResult }) => {
	// Парсим JSON результат
	const parseExplainResult = (): ExplainResult | null => {
		try {
			const parsed = JSON.parse(explainResult);
			// Обрабатываем случай когда это массив с одним элементом
			return Array.isArray(parsed) ? parsed[0] : parsed;
		} catch (error) {
			console.error("Ошибка парсинга EXPLAIN результата:", error);
			return null;
		}
	};

	// Получаем иконку для типа узла
	const getNodeIcon = (nodeType: string) => {
		const type = nodeType.toLowerCase();

		if (type.includes("seq scan") || type.includes("scan")) {
			return <Table className="h-4 w-4" />;
		}
		if (type.includes("index")) {
			return <Search className="h-4 w-4" />;
		}
		if (type.includes("hash") || type.includes("join")) {
			return <Hash className="h-4 w-4" />;
		}
		if (type.includes("sort")) {
			return <SortAsc className="h-4 w-4" />;
		}
		if (type.includes("aggregate") || type.includes("group")) {
			return <TrendingUp className="h-4 w-4" />;
		}
		if (type.includes("nested loop")) {
			return <RotateCcw className="h-4 w-4" />;
		}
		if (type.includes("merge")) {
			return <Shuffle className="h-4 w-4" />;
		}
		if (type.includes("filter")) {
			return <Filter className="h-4 w-4" />;
		}

		return <Zap className="h-4 w-4" />;
	};

	// Получаем цвет для стоимости операции
	const getCostColor = (cost: number, maxCost: number) => {
		const percentage = (cost / maxCost) * 100;
		if (percentage < 25) return "bg-green-500";
		if (percentage < 50) return "bg-yellow-500";
		if (percentage < 75) return "bg-orange-500";
		return "bg-red-500";
	};

	// Рекурсивно находим максимальную стоимость
	const findMaxCost = (node: ExplainNode): number => {
		let maxCost = node["Total Cost"] || 0;

		if (node.Plans) {
			node.Plans.forEach((child) => {
				const childMax = findMaxCost(child);
				maxCost = Math.max(maxCost, childMax);
			});
		}

		return maxCost;
	};

	// Рендерим узел плана
	const renderPlanNode = (node: ExplainNode, level: number = 0, maxCost: number) => {
		const totalCost = node["Total Cost"] || 0;
		const actualTime = node["Actual Total Time"] || 0;
		const planRows = node["Plan Rows"] || 0;
		const actualRows = node["Actual Rows"] || 0;

		return (
			<div key={`${node["Node Type"]}-${level}`} className="space-y-1">
				<div className="border rounded-md p-2 bg-card text-sm" style={{ marginLeft: `${level * 16}px` }}>
					{/* Заголовок с основной информацией */}
					<div className="flex items-center justify-between mb-1">
						<div className="flex items-center gap-1.5 flex-wrap">
							{getNodeIcon(node["Node Type"])}
							<span className="font-medium">{node["Node Type"]}</span>
							{node["Relation Name"] && (
								<Badge variant="outline" className="text-xs px-1 py-0 h-4">
									{node["Relation Name"]}
								</Badge>
							)}
							{node["Index Name"] && (
								<Badge variant="secondary" className="text-xs px-1 py-0 h-4">
									{node["Index Name"]}
								</Badge>
							)}
						</div>

						{totalCost > 0 && (
							<Badge
								variant="outline"
								className={`${getCostColor(totalCost, maxCost)} text-white border-0 text-xs px-1.5 py-0 h-5`}
							>
								{totalCost.toFixed(1)}
							</Badge>
						)}
					</div>

					{/* Компактные метрики в одну строку */}
					<div className="flex items-center justify-between">
						<div className="flex items-center gap-3 text-xs text-muted-foreground flex-wrap">
							{actualTime > 0 && (
								<span className="flex items-center gap-1">
									<Clock className="h-3 w-3" />
									{actualTime.toFixed(1)}ms
								</span>
							)}

							{planRows > 0 && (
								<span className="flex items-center gap-1">План строк: {planRows.toLocaleString()}</span>
							)}

							{actualRows >= 0 && node["Actual Rows"] !== undefined && (
								<span className="flex items-center gap-1">
									Факт строк: {actualRows.toLocaleString()}
								</span>
							)}

							{node["Plan Width"] && (
								<span className="flex items-center gap-1">Ширина: {node["Plan Width"]} байт</span>
							)}
						</div>

						{/* Круговая диаграмма стоимости */}
						{totalCost > 0 && maxCost > 0 && (
							<div className="flex items-center gap-1">
								<CircularProgress value={(totalCost / maxCost) * 100} size={16} strokeWidth={2} />
								<span className="text-xs text-muted-foreground">
									{((totalCost / maxCost) * 100).toFixed(0)}%
								</span>
							</div>
						)}
					</div>

					{/* Дополнительная информация - только если есть */}
					{(node["Sort Key"] || node["Hash Cond"] || node["Join Type"] || node["Filter"]) && (
						<div className="mt-1 space-y-0.5 text-xs">
							{node["Sort Key"] && (
								<div>
									<span className="text-muted-foreground">Sort: </span>
									<code className="bg-muted px-1 rounded text-xs">
										{Array.isArray(node["Sort Key"])
											? node["Sort Key"].join(", ")
											: node["Sort Key"]}
									</code>
								</div>
							)}

							{node["Hash Cond"] && (
								<div>
									<span className="text-muted-foreground">Hash: </span>
									<code className="bg-muted px-1 rounded text-xs">{node["Hash Cond"]}</code>
								</div>
							)}

							{node["Join Type"] && (
								<div>
									<span className="text-muted-foreground">Join: </span>
									<Badge variant="secondary" className="text-xs px-1 py-0 h-4">
										{node["Join Type"]}
									</Badge>
								</div>
							)}

							{node["Filter"] && (
								<div>
									<span className="text-muted-foreground">Filter: </span>
									<code className="bg-muted px-1 rounded text-xs">{node["Filter"]}</code>
								</div>
							)}
						</div>
					)}
				</div>

				{/* Рекурсивно рендерим дочерние узлы */}
				{node.Plans && node.Plans.length > 0 && (
					<div className="space-y-1">
						{node.Plans.map((childNode) => renderPlanNode(childNode, level + 1, maxCost))}
					</div>
				)}
			</div>
		);
	};

	const explainData = parseExplainResult();

	if (!explainData) {
		return <div className="p-4 text-center text-muted-foreground">Не удалось распарсить результат EXPLAIN</div>;
	}

	const maxCost = findMaxCost(explainData.Plan);

	return (
		<div className="space-y-3">
			{/* Общая информация о выполнении */}
			{(explainData["Planning Time"] || explainData["Execution Time"]) && (
				<div className="border rounded-md p-2 bg-muted/30 text-sm">
					<div className="flex items-center gap-4">
						<div className="flex items-center gap-2 font-medium">
							<Clock className="h-4 w-4" />
							Время выполнения:
						</div>
						<div className="flex items-center gap-4 text-muted-foreground">
							{explainData["Planning Time"] && (
								<span>
									Планирование:{" "}
									<span className="font-medium">{explainData["Planning Time"].toFixed(1)}ms</span>
								</span>
							)}
							{explainData["Execution Time"] && (
								<span>
									Выполнение:{" "}
									<span className="font-medium">{explainData["Execution Time"].toFixed(1)}ms</span>
								</span>
							)}
						</div>
					</div>
				</div>
			)}

			{/* План выполнения */}
			<div>
				<h4 className="font-medium mb-2 flex items-center gap-2 text-sm">
					<TrendingUp className="h-4 w-4" />
					План выполнения
				</h4>
				{renderPlanNode(explainData.Plan, 0, maxCost)}
			</div>
		</div>
	);
};
