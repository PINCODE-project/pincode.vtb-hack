"use client";

import React, { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import {
	Alert,
	AlertDescription,
	Button,
	EmojiPicker,
	EmojiPickerContent,
	EmojiPickerFooter,
	EmojiPickerSearch,
	Form as InternalForm,
	FormControl,
	FormField,
	FormItem,
	FormLabel,
	FormMessage,
	Input,
	Popover,
	PopoverContent,
	PopoverTrigger,
	toast,
} from "@pin-code/ui-kit";
import { DbConnectionCreateDto, usePostApiDbConnectionsCheck } from "@generated";
import { PasswordInput } from "@/components/ui/password-input";
import { TriangleAlert } from "lucide-react";

const formSchema = z.object({
	icon: z.string().optional(),
	name: z.string().min(2, "Минимальная длина - 2").max(100, "Максимальная длина - 100"),
	host: z.string().min(1, "Хост обязателен").max(255, "Максимальная длина - 255"),
	port: z.number().min(1, "Порт должен быть больше 0").max(65535, "Порт должен быть меньше 65536"),
	database: z.string().min(1, "Название базы данных обязательно").max(100, "Максимальная длина - 100"),
	username: z.string().min(1, "Имя пользователя обязательно").max(100, "Максимальная длина - 100"),
	password: z.string().min(1, "Пароль обязателен"),
});

type FormData = z.infer<typeof formSchema>;

type Props = {
	defaultValues?: Partial<FormData>;
	onSubmit: (data: DbConnectionCreateDto) => void;
	submitButtonText: string;
	isLoading?: boolean;
};

const convertFormDataToApiFormat = (formData: FormData): DbConnectionCreateDto => ({
	name: formData.icon ? `${formData.icon} ${formData.name}` : formData.name,
	host: formData.host,
	port: formData.port,
	database: formData.database,
	username: formData.username,
	password: formData.password,
});

export const Form = ({ defaultValues, submitButtonText, onSubmit, isLoading = false }: Props) => {
	const [emojiPickerOpen, setEmojiPickerOpen] = React.useState(false);
	const [isTestingConnection, setIsTestingConnection] = useState(false);
	const [testResult, setTestResult] = useState<{ isValid?: boolean; errorMessage?: string | null } | null>(null);

	const form = useForm<FormData>({
		resolver: zodResolver(formSchema),
		defaultValues: {
			icon: "👨‍💻",
			name: "",
			host: "localhost",
			port: 5432,
			database: "",
			username: "",
			password: "",
			...defaultValues,
		},
	});

	const testConnectionMutation = usePostApiDbConnectionsCheck({
		mutation: {
			onSuccess: (data) => {
				setTestResult({
					isValid: data.isValid,
					errorMessage: data.errorMessage,
				});
				setIsTestingConnection(false);
			},
			onError: (error) => {
				setTestResult({
					isValid: false,
					errorMessage: error.message || "Ошибка при создании подключения",
				});
				toast(`Ошибка подключения: ${error.message || "Неизвестная ошибка"}`);
				setIsTestingConnection(false);
			},
		},
	});

	const handleTestConnection = async () => {
		const isValid = await form.trigger();
		if (!isValid) {
			toast("Заполните все обязательные поля корректно");
			return;
		}

		setIsTestingConnection(true);
		setTestResult(null);

		const formData = form.getValues();
		testConnectionMutation.mutate({ data: convertFormDataToApiFormat(formData) });
	};

	const handleEmojiSelect = ({ emoji }: { emoji: string; label: string }) => {
		form.setValue("icon", emoji);
		setEmojiPickerOpen(false);
	};

	return (
		<InternalForm {...form}>
			<form
				onSubmit={form.handleSubmit((data) => onSubmit(convertFormDataToApiFormat(data)))}
				className="grid gap-4 py-4"
			>
				<FormItem>
					<FormLabel>Название проекта</FormLabel>
					<div className="flex w-full gap-3">
						<FormField
							control={form.control}
							name="icon"
							render={({ field }) => (
								<FormItem>
									<FormControl>
										<Popover open={emojiPickerOpen} onOpenChange={setEmojiPickerOpen}>
											<PopoverTrigger asChild>
												<Button
													variant="outline"
													className="justify-start text-left font-normal w-[40px] px-2"
													disabled={isLoading}
													type="button"
												>
													<span className="mr-2 text-xl">{field.value || "👨‍💻"}</span>
												</Button>
											</PopoverTrigger>
											<PopoverContent align="start" className="w-fit p-0">
												<EmojiPicker className="h-84" onEmojiSelect={handleEmojiSelect}>
													<EmojiPickerSearch />
													<EmojiPickerContent />
													<EmojiPickerFooter />
												</EmojiPicker>
											</PopoverContent>
										</Popover>
									</FormControl>
									<FormMessage />
								</FormItem>
							)}
						/>

						<FormField
							control={form.control}
							name="name"
							render={({ field }) => (
								<FormItem className="w-full">
									<FormControl>
										<Input {...field} placeholder="Введите название проекта" />
									</FormControl>
									<FormMessage />
								</FormItem>
							)}
						/>
					</div>
				</FormItem>

				<div className="grid grid-cols-2 gap-4">
					<FormField
						control={form.control}
						name="host"
						render={({ field }) => (
							<FormItem>
								<FormLabel>Хост</FormLabel>
								<FormControl>
									<Input {...field} placeholder="localhost" disabled={isLoading} />
								</FormControl>
								<FormMessage />
							</FormItem>
						)}
					/>

					<FormField
						control={form.control}
						name="port"
						render={({ field }) => (
							<FormItem>
								<FormLabel>Порт</FormLabel>
								<FormControl>
									<Input
										{...field}
										type="number"
										placeholder="5432"
										disabled={isLoading}
										onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
									/>
								</FormControl>
								<FormMessage />
							</FormItem>
						)}
					/>
				</div>

				<FormField
					control={form.control}
					name="database"
					render={({ field }) => (
						<FormItem>
							<FormLabel>База данных</FormLabel>
							<FormControl>
								<Input {...field} placeholder="Название базы данных" disabled={isLoading} />
							</FormControl>
							<FormMessage />
						</FormItem>
					)}
				/>

				<FormField
					control={form.control}
					name="username"
					render={({ field }) => (
						<FormItem>
							<FormLabel>Имя пользователя</FormLabel>
							<FormControl>
								<Input {...field} placeholder="Имя пользователя" disabled={isLoading} />
							</FormControl>
							<FormMessage />
						</FormItem>
					)}
				/>

				<FormField
					control={form.control}
					name="password"
					render={({ field }) => (
						<FormItem>
							<FormLabel>Пароль</FormLabel>
							<FormControl>
								<PasswordInput {...field} placeholder="Пароль" disabled={isLoading} />
							</FormControl>
							<FormMessage />
						</FormItem>
					)}
				/>

				<Alert variant="default">
					<TriangleAlert color="var(--color-amber-600)" width={14} height={14} />
					<AlertDescription className="flex">
						Должен быть установлен pg_stat_statements!
					</AlertDescription>
				</Alert>

				<div className="pt-4 flex justify-between items-center gap-3">
					<div>
						{testResult && (
							<div
								className={`rounded-md text-sm ${testResult.isValid ? "text-green-700" : "text-red-700"}`}
							>
								{testResult.isValid
									? "✅  Подключение успешно!"
									: `❌ Ошибка: ${testResult.errorMessage}`}
							</div>
						)}
					</div>

					<div className="flex justify-between gap-3">
						<Button
							type="button"
							variant="outline"
							onClick={handleTestConnection}
							disabled={isLoading || isTestingConnection}
						>
							{isTestingConnection ? (
								<div className="h-5 w-5 animate-spin rounded-full border-2 border-primary border-r-transparent mx-auto"></div>
							) : (
								"Протестировать"
							)}
						</Button>
						<Button type="submit" disabled={isLoading || isTestingConnection || !testResult?.isValid}>
							{isLoading ? (
								<div className="h-5 w-5 animate-spin rounded-full border-2 border-secondary border-r-transparent mx-auto"></div>
							) : (
								submitButtonText
							)}
						</Button>
					</div>
				</div>
			</form>
		</InternalForm>
	);
};
