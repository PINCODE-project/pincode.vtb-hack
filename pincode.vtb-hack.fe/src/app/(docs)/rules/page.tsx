"use client";

import React, { useState } from "react";
import {
	Card,
	CardContent,
	CardHeader,
	CardTitle,
	Button,
	Table,
	TableBody,
	TableCell,
	TableHead,
	TableHeader,
	TableRow,
	Badge,
	Dialog,
	DialogContent,
	DialogHeader,
	DialogTitle,
	DialogTrigger,
	Alert,
	AlertDescription,
} from "@pin-code/ui-kit";
import {
	useGetApiSqlAnalyzerFind,
	usePostApiSqlAnalyzer,
	usePatchApiSqlAnalyzer,
	useDeleteApiSqlAnalyzerId,
} from "@generated/hooks/SqlAnalyzeRule";
import type { SqlAnalyzeRuleDto } from "@generated/models/SqlAnalyzeRuleDto.ts";
import { Plus, Edit2, Trash2, Shield, Activity, AlertTriangle } from "lucide-react";
import { RuleForm, DeleteRuleDialog } from "./components";

type Rule = SqlAnalyzeRuleDto;

export default function RulesPage() {
	const [editingRule, setEditingRule] = useState<Rule | null>(null);
	const [deletingRule, setDeletingRule] = useState<Rule | null>(null);
	const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);

	// Получение списка правил
	const { data: rules, isLoading, error, refetch } = useGetApiSqlAnalyzerFind();

	// Мутации
	const createMutation = usePostApiSqlAnalyzer({
		mutation: {
			onSuccess: () => {
				refetch();
				setIsCreateDialogOpen(false);
			},
		},
	});

	const updateMutation = usePatchApiSqlAnalyzer({
		mutation: {
			onSuccess: () => {
				refetch();
				setEditingRule(null);
			},
		},
	});

	const deleteMutation = useDeleteApiSqlAnalyzerId({
		mutation: {
			onSuccess: () => {
				refetch();
				setDeletingRule(null);
			},
		},
	});

	// Получение цвета для уровня серьезности
	const getSeverityColor = (severity: number) => {
		if (severity <= 0) return "default";
		if (severity === 1) return "secondary";
		return "destructive";
	};

	// Получение иконки для уровня серьезности
	const getSeverityIcon = (severity: number) => {
		if (severity <= 0) return Shield;
		if (severity === 1) return Activity;
		return AlertTriangle;
	};

	// Получение текста для уровня серьезности
	const getSeverityText = (severity: number) => {
		if (severity <= 0) return "Информация";
		if (severity === 1) return "Предупреждение";
		return "Ошибка";
	};

	const handleCreate = (data: Omit<Rule, "id" | "createdAt">) => {
		createMutation.mutate({
			data: {
				name: data.name,
				severity: data.severity as any,
				problem: data.problem,
				recommendation: data.recommendation,
				regex: data.regex,
			},
		});
	};

	const handleUpdate = (data: Omit<Rule, "id" | "createdAt">) => {
		if (!editingRule) return;

		updateMutation.mutate({
			data: {
				id: editingRule.id,
				name: data.name,
				severity: data.severity as any,
				problem: data.problem,
				recommendation: data.recommendation,
				regex: data.regex,
				isActive: data.isActive,
			},
		});
	};

	const handleDelete = () => {
		if (!deletingRule) return;
		deleteMutation.mutate({ id: deletingRule.id });
	};

	if (isLoading) {
		return (
			<div className="p-6">
				<div className="mb-6">
					<h1 className="text-3xl font-bold">Правила анализа SQL</h1>
					<p className="text-muted-foreground mt-2">
						Управление кастомными правилами для анализа SQL запросов
					</p>
				</div>
				<Card>
					<CardContent className="flex items-center justify-center py-8">
						<div className="text-center">
							<div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto mb-4"></div>
							<p className="text-muted-foreground">Загрузка правил...</p>
						</div>
					</CardContent>
				</Card>
			</div>
		);
	}

	if (error) {
		return (
			<div className="p-6">
				<div className="mb-6">
					<h1 className="text-3xl font-bold">Правила анализа SQL</h1>
				</div>
				<Alert variant="destructive">
					<AlertDescription>Ошибка загрузки правил: {error.message}</AlertDescription>
				</Alert>
			</div>
		);
	}

	return (
		<div className="p-6 space-y-6">
			<div className="mb-6">
				<h1 className="text-3xl font-bold">Правила анализа SQL</h1>
				<p className="text-muted-foreground mt-2">Управление кастомными правилами для анализа SQL запросов</p>
			</div>

			<Card>
				<CardHeader>
					<div className="flex items-center justify-between">
						<CardTitle className="flex items-center gap-2">
							<Shield className="h-5 w-5" />
							Список правил ({rules?.length || 0})
						</CardTitle>
						<Dialog open={isCreateDialogOpen} onOpenChange={setIsCreateDialogOpen}>
							<DialogTrigger asChild>
								<Button>
									<Plus className="h-4 w-4 mr-2" />
									Добавить правило
								</Button>
							</DialogTrigger>
							<DialogContent className="max-w-2xl">
								<DialogHeader>
									<DialogTitle>Создание нового правила</DialogTitle>
								</DialogHeader>
								<RuleForm
									onSubmit={handleCreate}
									isLoading={createMutation.isPending}
									onCancel={() => setIsCreateDialogOpen(false)}
								/>
							</DialogContent>
						</Dialog>
					</div>
				</CardHeader>
				<CardContent>
					{!rules?.length ? (
						<div className="text-center py-8">
							<Shield className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
							<h3 className="text-lg font-semibold mb-2">Нет созданных правил</h3>
							<p className="text-muted-foreground">Создайте первое правило для анализа SQL запросов</p>
						</div>
					) : (
						<div className="w-full overflow-x-auto">
							<Table>
								<TableHeader>
									<TableRow>
										<TableHead className="w-[180px]">Название</TableHead>
										<TableHead className="w-[200px] min-w-[200px]">Проблема</TableHead>
										<TableHead className="w-[200px] min-w-[200px]">Рекомендация</TableHead>
										<TableHead className="w-[140px]">Серьезность</TableHead>
										<TableHead className="w-[100px]">Статус</TableHead>
										<TableHead className="w-[110px]">Создано</TableHead>
										<TableHead className="w-[100px] text-right">Действия</TableHead>
									</TableRow>
								</TableHeader>
								<TableBody>
									{rules.map((rule: Rule) => {
										const SeverityIcon = getSeverityIcon(rule.severity);
										return (
											<TableRow key={rule.id}>
												<TableCell className="font-medium w-[180px] align-top">
													<div
														className="break-words whitespace-normal"
														title={rule.name || "Без названия"}
													>
														{rule.name || "Без названия"}
													</div>
												</TableCell>
												<TableCell className="w-[200px] min-w-[200px] align-top">
													<div className="max-w-[200px]">
														<div className="text-sm leading-relaxed break-words whitespace-normal">
															{rule.problem || "Не указано"}
														</div>
													</div>
												</TableCell>
												<TableCell className="w-[200px] min-w-[200px] align-top">
													<div className="max-w-[200px]">
														<div className="text-sm leading-relaxed break-words whitespace-normal">
															{rule.recommendation || "Не указано"}
														</div>
													</div>
												</TableCell>
												<TableCell className="w-[140px]">
													<Badge
														variant={getSeverityColor(rule.severity)}
														className="flex items-center gap-1 w-fit text-xs"
													>
														<SeverityIcon className="h-3 w-3" />
														{getSeverityText(rule.severity)}
													</Badge>
												</TableCell>
												<TableCell className="w-[100px]">
													<Badge
														variant={rule.isActive ? "default" : "secondary"}
														className="text-xs"
													>
														{rule.isActive ? "Активно" : "Неактивно"}
													</Badge>
												</TableCell>
												<TableCell className="w-[110px]">
													<div className="text-sm">
														{new Date(rule.createdAt).toLocaleDateString("ru-RU", {
															day: "2-digit",
															month: "2-digit",
															year: "numeric",
														})}
													</div>
												</TableCell>
												<TableCell className="w-[100px] text-right">
													<div className="flex items-center gap-1 justify-end">
														<Button
															variant="ghost"
															size="sm"
															onClick={() => setEditingRule(rule)}
															className="h-8 w-8 p-0"
														>
															<Edit2 className="h-4 w-4" />
														</Button>
														<Button
															variant="ghost"
															size="sm"
															onClick={() => setDeletingRule(rule)}
															className="h-8 w-8 p-0"
														>
															<Trash2 className="h-4 w-4" />
														</Button>
													</div>
												</TableCell>
											</TableRow>
										);
									})}
								</TableBody>
							</Table>
						</div>
					)}
				</CardContent>
			</Card>

			{/* Диалог редактирования */}
			<Dialog open={!!editingRule} onOpenChange={() => setEditingRule(null)}>
				<DialogContent className="max-w-2xl">
					<DialogHeader>
						<DialogTitle>Редактирование правила</DialogTitle>
					</DialogHeader>
					{editingRule && (
						<RuleForm
							initialData={editingRule as any}
							onSubmit={handleUpdate}
							isLoading={updateMutation.isPending}
							onCancel={() => setEditingRule(null)}
						/>
					)}
				</DialogContent>
			</Dialog>

			{/* Диалог удаления */}
			<DeleteRuleDialog
				rule={deletingRule}
				isOpen={!!deletingRule}
				onClose={() => setDeletingRule(null)}
				onConfirm={handleDelete}
				isLoading={deleteMutation.isPending}
			/>
		</div>
	);
}
