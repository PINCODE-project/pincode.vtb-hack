"use client";

import React from "react";
import {
	Dialog,
	DialogContent,
	DialogDescription,
	DialogFooter,
	DialogHeader,
	DialogTitle,
	Button,
	Badge,
} from "@pin-code/ui-kit";
import { Loader2, AlertTriangle, Shield, Activity } from "lucide-react";
import type { SqlAnalyzeRuleDto } from "@generated/models/SqlAnalyzeRuleDto.ts";

type Rule = SqlAnalyzeRuleDto;

type DeleteRuleDialogProps = {
	rule: Rule | null;
	isOpen: boolean;
	onClose: () => void;
	onConfirm: () => void;
	isLoading?: boolean;
};

export function DeleteRuleDialog({ rule, isOpen, onClose, onConfirm, isLoading = false }: DeleteRuleDialogProps) {
	if (!rule) return null;

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

	const SeverityIcon = getSeverityIcon(rule.severity);

	return (
		<Dialog open={isOpen} onOpenChange={onClose}>
			<DialogContent>
				<DialogHeader>
					<DialogTitle className="flex items-center gap-2">
						<AlertTriangle className="h-5 w-5 text-destructive" />
						Удаление правила
					</DialogTitle>
					<DialogDescription>
						Вы уверены, что хотите удалить это правило? Это действие нельзя отменить.
					</DialogDescription>
				</DialogHeader>

				<div className="space-y-4">
					<div className="rounded-lg border p-4 bg-muted/20">
						<div className="space-y-3">
							<div className="flex items-center justify-between">
								<h4 className="font-semibold">{rule.name || "Без названия"}</h4>
								<div className="flex items-center gap-2">
									<Badge
										variant={getSeverityColor(rule.severity)}
										className="flex items-center gap-1"
									>
										<SeverityIcon className="h-3 w-3" />
										{getSeverityText(rule.severity)}
									</Badge>
									<Badge variant={rule.isActive ? "default" : "secondary"}>
										{rule.isActive ? "Активно" : "Неактивно"}
									</Badge>
								</div>
							</div>

							<div>
								<p className="text-sm font-medium text-muted-foreground mb-1">Проблема:</p>
								<p className="text-sm">{rule.problem || "Не указано"}</p>
							</div>

							<div>
								<p className="text-sm font-medium text-muted-foreground mb-1">Рекомендация:</p>
								<p className="text-sm">{rule.recommendation || "Не указано"}</p>
							</div>

							<div>
								<p className="text-sm font-medium text-muted-foreground mb-1">Регулярное выражение:</p>
								<code className="text-sm bg-muted px-2 py-1 rounded">{rule.regex || "Не указано"}</code>
							</div>

							<div>
								<p className="text-sm font-medium text-muted-foreground mb-1">Создано:</p>
								<p className="text-sm">
									{new Date(rule.createdAt).toLocaleDateString("ru-RU", {
										day: "2-digit",
										month: "2-digit",
										year: "numeric",
										hour: "2-digit",
										minute: "2-digit",
									})}
								</p>
							</div>
						</div>
					</div>
				</div>

				<DialogFooter>
					<Button variant="outline" onClick={onClose} disabled={isLoading}>
						Отмена
					</Button>
					<Button variant="destructive" onClick={onConfirm} disabled={isLoading}>
						{isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
						Удалить правило
					</Button>
				</DialogFooter>
			</DialogContent>
		</Dialog>
	);
}
