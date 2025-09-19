"use client";

import React from "react";
import { Badge } from "@pin-code/ui-kit";
import {
	Code,
	DatabaseZap,
	FileClock,
	Key,
	ListFilter,
	LockIcon,
	AlertTriangle,
	CheckCircle2,
	AlertCircle,
	Info,
} from "lucide-react";
import { type CollapsibleListItemType, CollapsibleList } from "@components/ui/collapsible-list.tsx";
import { ThemeToggle } from "@components/theme-toggle.tsx";
import {
	autovacuumMetrics,
	cacheMetrics,
	explainRules,
	getLevelBadge,
	indexMetrics,
	lockMetrics,
	sqlRules,
	tempFilesMetrics,
} from "@components/documentation";

/**
 * Получает иконку в зависимости от уровня серьезности
 */
const getLevelIcon = (level?: "critical" | "high" | "medium" | "low") => {
	switch (level) {
		case "critical":
			return <AlertTriangle className="w-5 h-5 text-red-500" />;
		case "high":
			return <AlertCircle className="w-5 h-5 text-orange-500" />;
		case "medium":
			return <Info className="w-5 h-5 text-yellow-500" />;
		case "low":
			return <CheckCircle2 className="w-5 h-5 text-green-500" />;
		default:
			return <Info className="w-5 h-5 text-gray-500" />;
	}
};

/**
 * Получает цвет градиента для карточки в зависимости от уровня
 */
const getLevelGradient = (level?: "critical" | "high" | "medium" | "low") => {
	switch (level) {
		case "critical":
			return "from-red-50 to-red-100 border-red-200 dark:from-red-950/30 dark:to-red-900/20 dark:border-red-800/30";
		case "high":
			return "from-orange-50 to-orange-100 border-orange-200 dark:from-orange-950/30 dark:to-orange-900/20 dark:border-orange-800/30";
		case "medium":
			return "from-yellow-50 to-yellow-100 border-yellow-200 dark:from-yellow-950/30 dark:to-yellow-900/20 dark:border-yellow-800/30";
		case "low":
			return "from-green-50 to-green-100 border-green-200 dark:from-green-950/30 dark:to-green-900/20 dark:border-green-800/30";
		default:
			return "from-gray-50 to-gray-100 border-gray-200 dark:from-gray-950/30 dark:to-gray-900/20 dark:border-gray-800/30";
	}
};

/**
 * Компонент карточки метрики
 */
interface MetricCardProps {
	title: string;
	metric?: string;
	threshold?: string;
	level?: "critical" | "high" | "medium" | "low";
	description: string;
	recommendations: string[];
}

const MetricCard: React.FC<MetricCardProps> = ({ title, metric, threshold, level, description, recommendations }) => {
	return (
		<div className={`rounded-xl border-2 p-4 ${getLevelGradient(level)}`}>
			<div className="flex items-start justify-between mb-2">
				<div className="flex items-center gap-3">
					{getLevelIcon(level)}
					<h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">{title}</h3>
				</div>
				{getLevelBadge(level)}
			</div>

			<p className="text-sm text-gray-600 dark:text-gray-300 mb-4 leading-relaxed">{description}</p>

			<div className="space-y-3 mb-4">
				{metric && (
					<div className="flex items-center gap-2">
						<span className="text-sm font-medium text-gray-700 dark:text-gray-300">Метрика:</span>
						<Badge variant="outline" className="text-xs">
							{metric}
						</Badge>
					</div>
				)}
				{threshold && (
					<div className="flex items-center gap-2">
						<span className="text-sm font-medium text-gray-700 dark:text-gray-300">
							Пороговое значение:
						</span>
						<span className="text-sm text-gray-600 dark:text-gray-300">{threshold}</span>
					</div>
				)}
			</div>

			<div className="space-y-2">
				<h4 className="text-sm font-semibold text-gray-900 dark:text-gray-100 flex items-center gap-2">
					Рекомендации
				</h4>
				<ul className="space-y-2">
					{recommendations.map((rec, index) => (
						<li
							key={index}
							className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-300 mb-1"
						>
							<div className="w-0.5 h-0.5 rounded-full bg-gray-400 dark:bg-gray-500 flex-shrink-0"></div>
							<span className="leading-relaxed">{rec}</span>
						</li>
					))}
				</ul>
			</div>
		</div>
	);
};

/**
 * Страница документации с правилами и рекомендациями
 */
export default function DocumentationPage() {
	// Создаем элементы для первого CollapsibleList - Анализ настроек БД
	const databaseSettingsItems: CollapsibleListItemType[] = [
		{
			id: "autovacuum-metrics",
			title: "Метрики Autovacuum",
			icon: ListFilter,
			isExpanded: false,
			content: (
				<div className="flex flex-col gap-2">
					{autovacuumMetrics.items.map((item, index) => (
						<MetricCard
							key={index}
							title={item.title}
							metric={item.metric}
							threshold={item.threshold}
							level={item.level}
							description={item.description}
							recommendations={item.recommendations}
						/>
					))}
				</div>
			),
		},
		{
			id: "cache-metrics",
			title: "Метрики Cache",
			icon: DatabaseZap,
			isExpanded: false,
			content: (
				<div className="flex flex-col gap-2">
					{cacheMetrics.items.map((item, index) => (
						<MetricCard
							key={index}
							title={item.title}
							metric={item.metric}
							threshold={item.threshold}
							level={item.level}
							description={item.description}
							recommendations={item.recommendations}
						/>
					))}
				</div>
			),
		},
		{
			id: "lock-metrics",
			title: "Метрики Блокировок",
			icon: LockIcon,
			isExpanded: false,
			content: (
				<div className="flex flex-col gap-2">
					{lockMetrics.items.map((item, index) => (
						<MetricCard
							key={index}
							title={item.title}
							metric={item.metric}
							threshold={item.threshold}
							level={item.level}
							description={item.description}
							recommendations={item.recommendations}
						/>
					))}
				</div>
			),
		},
		{
			id: "index-metrics",
			title: "Метрики Индексов",
			icon: Key,
			isExpanded: false,
			content: (
				<div className="flex flex-col gap-2">
					{indexMetrics.items.map((item, index) => (
						<MetricCard
							key={index}
							title={item.title}
							metric={item.metric}
							threshold={item.threshold}
							level={item.level}
							description={item.description}
							recommendations={item.recommendations}
						/>
					))}
				</div>
			),
		},
		{
			id: "temp-files-metrics",
			title: "Метрики Временных файлов",
			icon: FileClock,
			isExpanded: false,
			content: (
				<div className="flex flex-col gap-2">
					{tempFilesMetrics.items.map((item, index) => (
						<MetricCard
							key={index}
							title={item.title}
							metric={item.metric}
							threshold={item.threshold}
							level={item.level}
							description={item.description}
							recommendations={item.recommendations}
						/>
					))}
				</div>
			),
		},
	];

	// Создаем элементы для второго CollapsibleList - SQL анализ
	const sqlAnalysisItems: CollapsibleListItemType[] = [
		{
			id: "sql-rules-analysis",
			title: "Правила анализа SQL-запроса",
			icon: Code,
			isExpanded: false,
			content: (
				<div className="overflow-x-auto rounded-lg border mr-1">
					<table className="w-full">
						<thead className="bg-muted">
							<tr>
								<th className="px-4 py-3 text-left font-medium">Правило</th>
								<th className="px-4 py-3 text-left font-medium">Описание проблемы</th>
								<th className="px-4 py-3 text-left font-medium">Рекомендация</th>
							</tr>
						</thead>
						<tbody>
							{sqlRules.map((rule, index) => (
								<tr key={index} className={index % 2 === 0 ? "bg-background" : "bg-muted/30"}>
									<td className="px-4 py-3 font-medium text-sm">
										<Badge variant="outline" className="text-xs">
											{rule.rule}
										</Badge>
									</td>
									<td className="px-4 py-3 text-sm">{rule.description}</td>
									<td className="px-4 py-3 text-sm">{rule.recommendation}</td>
								</tr>
							))}
						</tbody>
					</table>
				</div>
			),
		},
		{
			id: "explain-rules-analysis",
			title: "Правила анализа результата Explain",
			icon: Code,
			isExpanded: false,
			content: (
				<div className="overflow-x-auto rounded-lg border mr-1">
					<table className="w-full">
						<thead className="bg-muted">
							<tr>
								<th className="px-4 py-3 text-left font-medium">Правило</th>
								<th className="px-4 py-3 text-left font-medium">Описание проблемы</th>
								<th className="px-4 py-3 text-left font-medium">Рекомендация</th>
							</tr>
						</thead>
						<tbody>
							{explainRules.map((rule, index) => (
								<tr key={index} className={index % 2 === 0 ? "bg-background" : "bg-muted/30"}>
									<td className="px-4 py-3 font-medium text-sm">
										<Badge variant="outline" className="text-xs">
											{rule.rule}
										</Badge>
									</td>
									<td className="px-4 py-3 text-sm">{rule.description}</td>
									<td className="px-4 py-3 text-sm">{rule.recommendation}</td>
								</tr>
							))}
						</tbody>
					</table>
				</div>
			),
		},
	];

	return (
		<div className="flex h-screen">
			{/* Основной контент */}
			<div className="flex-1 overflow-auto">
				<div className="max-w-4xl mx-auto p-8">
					<div className="mb-20">
						<div className="flex items-start justify-between mb-4">
							<div>
								<h1 className="text-3xl font-bold tracking-tight mb-2">Документация DB Explorer</h1>
								<p className="text-muted-foreground">
									На этой странице собраны все правила анализа и рекомендации, применяемые
									алгоритмическим модулем сервиса. Здесь вы можете ознакомиться с логикой, по которой
									формируются выводы системы, а также с набором методик и практик, лежащих в основе
									автоматизированного анализа
								</p>
							</div>
							<ThemeToggle />
						</div>
					</div>

					{/* Анализ настроек БД */}
					<section id="database-settings-analysis" className="mb-20">
						<h2 className="text-3xl font-bold mb-2 flex items-center gap-2">Анализ настроек БД</h2>
						<p className="text-muted-foreground mb-8">
							Метрики и рекомендации для оптимизации производительности PostgreSQL
						</p>
						<CollapsibleList items={databaseSettingsItems} className="mb-8" />
					</section>

					{/* Алгоритмический анализ SQL запросов */}
					<section id="sql-analysis-section" className="mb-12">
						<h2 className="text-3xl font-bold mb-2 flex items-center gap-2">
							Алгоритмический анализ SQL запросов
						</h2>
						<p className="text-muted-foreground mb-8">
							Правила анализа SQL-запросов и планов выполнения для выявления проблем производительности
						</p>
						<CollapsibleList items={sqlAnalysisItems} className="mb-8" />
					</section>
				</div>
			</div>
		</div>
	);
}
