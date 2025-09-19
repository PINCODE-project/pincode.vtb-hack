"use client";

import React from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import {
	Form,
	FormControl,
	FormDescription,
	FormField,
	FormItem,
	FormLabel,
	FormMessage,
	Button,
	Input,
	Textarea,
	Select,
	SelectContent,
	SelectItem,
	SelectTrigger,
	SelectValue,
	Switch,
	DialogFooter,
} from "@pin-code/ui-kit";
import type { SqlAnalyzeRuleDto } from "@generated/models/SqlAnalyzeRuleDto.ts";
import { Loader2 } from "lucide-react";

const ruleSchema = z.object({
	name: z.string().min(1, "Название обязательно").max(100, "Максимум 100 символов"),
	severity: z.number().min(0).max(2),
	problem: z.string().min(1, "Описание проблемы обязательно").max(500, "Максимум 500 символов"),
	recommendation: z.string().min(1, "Рекомендация обязательна").max(500, "Максимум 500 символов"),
	regex: z.string().min(1, "Регулярное выражение обязательно"),
	isActive: z.boolean(),
});

type RuleFormData = z.infer<typeof ruleSchema>;

type Rule = SqlAnalyzeRuleDto;

type RuleFormProps = {
	initialData?: Rule;
	onSubmit: (data: Omit<Rule, "id" | "createdAt">) => void;
	onCancel: () => void;
	isLoading?: boolean;
};

export function RuleForm({ initialData, onSubmit, onCancel, isLoading = false }: RuleFormProps) {
	const form = useForm<RuleFormData>({
		resolver: zodResolver(ruleSchema),
		defaultValues: {
			name: initialData?.name || "",
			severity: initialData?.severity || 0,
			problem: initialData?.problem || "",
			recommendation: initialData?.recommendation || "",
			regex: initialData?.regex || "",
			isActive: initialData?.isActive ?? true,
		},
	});

	const handleSubmit = (data: RuleFormData) => {
		onSubmit(data);
	};

	// Тестирование регулярного выражения
	const testRegex = (regex: string) => {
		try {
			new RegExp(regex);
			return true;
		} catch {
			return false;
		}
	};

	const severityOptions = [
		{ value: 0, label: "Информация", description: "Информационное сообщение" },
		{ value: 1, label: "Предупреждение", description: "Потенциальная проблема" },
		{ value: 2, label: "Ошибка", description: "Серьезная ошибка" },
	];

	return (
		<Form {...form}>
			<form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-6">
				<div className="grid grid-cols-2 gap-4">
					<FormField
						control={form.control}
						name="name"
						render={({ field }) => (
							<FormItem>
								<FormLabel>Название правила</FormLabel>
								<FormControl>
									<Input placeholder="Например: SELECT *" {...field} />
								</FormControl>
								<FormDescription>Краткое описательное название правила</FormDescription>
								<FormMessage />
							</FormItem>
						)}
					/>

					<FormField
						control={form.control}
						name="severity"
						render={({ field }) => (
							<FormItem>
								<FormLabel>Уровень серьезности</FormLabel>
								<Select
									onValueChange={(value) => field.onChange(Number(value))}
									value={field.value.toString()}
								>
									<FormControl>
										<SelectTrigger>
											<SelectValue placeholder="Выберите уровень" />
										</SelectTrigger>
									</FormControl>
									<SelectContent>
										{severityOptions.map((option) => (
											<SelectItem key={option.value} value={option.value.toString()}>
												<div>
													<div className="font-medium">{option.label}</div>
													{/*<div className="text-xs text-muted-foreground">*/}
													{/*	{option.description}*/}
													{/*</div>*/}
												</div>
											</SelectItem>
										))}
									</SelectContent>
								</Select>
								<FormDescription>Определяет важность нарушения правила</FormDescription>
								<FormMessage />
							</FormItem>
						)}
					/>
				</div>

				<FormField
					control={form.control}
					name="problem"
					render={({ field }) => (
						<FormItem>
							<FormLabel>Описание проблемы</FormLabel>
							<FormControl>
								<Textarea
									placeholder="Опишите, в чем состоит проблема использования данной конструкции"
									className="min-h-[80px]"
									{...field}
								/>
							</FormControl>
							<FormDescription>Объясните, почему данная SQL конструкция проблематична</FormDescription>
							<FormMessage />
						</FormItem>
					)}
				/>

				<FormField
					control={form.control}
					name="recommendation"
					render={({ field }) => (
						<FormItem>
							<FormLabel>Рекомендация</FormLabel>
							<FormControl>
								<Textarea
									placeholder="Предложите, как можно исправить или улучшить SQL запрос"
									className="min-h-[80px]"
									{...field}
								/>
							</FormControl>
							<FormDescription>Дайте совет, как правильно переписать код</FormDescription>
							<FormMessage />
						</FormItem>
					)}
				/>

				<FormField
					control={form.control}
					name="regex"
					render={({ field }) => (
						<FormItem>
							<FormLabel>Регулярное выражение</FormLabel>
							<FormControl>
								<Input placeholder="\\bSELECT\\s+\\*" className="font-mono" {...field} />
							</FormControl>
							<FormDescription>
								{field.value && !testRegex(field.value) ? (
									<span className="text-destructive">⚠️ Неверное регулярное выражение</span>
								) : field.value && testRegex(field.value) ? (
									<span className="text-green-600">✅ Регулярное выражение корректно</span>
								) : (
									"Регулярное выражение для поиска проблемной конструкции в SQL"
								)}
							</FormDescription>
							<FormMessage />
						</FormItem>
					)}
				/>

				<FormField
					control={form.control}
					name="isActive"
					render={({ field }) => (
						<FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
							<div className="space-y-0.5">
								<FormLabel className="text-base">Активное правило</FormLabel>
								<FormDescription>
									Определяет, будет ли правило использоваться при анализе SQL
								</FormDescription>
							</div>
							<FormControl>
								<Switch checked={field.value} onCheckedChange={field.onChange} />
							</FormControl>
						</FormItem>
					)}
				/>

				<DialogFooter>
					<Button variant="outline" onClick={onCancel} type="button">
						Отмена
					</Button>
					<Button type="submit" disabled={isLoading || !testRegex(form.watch("regex"))}>
						{isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
						{initialData ? "Сохранить изменения" : "Создать правило"}
					</Button>
				</DialogFooter>
			</form>
		</Form>
	);
}
