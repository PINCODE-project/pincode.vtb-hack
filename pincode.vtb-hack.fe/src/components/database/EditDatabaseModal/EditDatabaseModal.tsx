"use client";
import { Dialog, DialogContent, DialogHeader, DialogTitle, toast } from "@pin-code/ui-kit";
import { DbConnectionUpdateDto, usePatchApiDbConnections } from "@generated";
import { useEditDatabaseModalStore } from "@store/database/edit-modal.ts";

import { Form } from "../CreateDatabaseModal/Form";
import { getQueryClient } from "@/lib/query-client.ts";

export const EditDatabaseModal = () => {
	const { isOpen, close, database } = useEditDatabaseModalStore();
	const queryClient = getQueryClient();

	const updateDatabaseMutation = usePatchApiDbConnections({
		mutation: {
			onSuccess: async () => {
				await queryClient.invalidateQueries();
				close();
				toast("Подключение к базе данных успешно обновлено!");
			},
			onError: () => {
				toast("Ошибка при обновлении подключения к базе данных!");
			},
		},
	});

	const onSubmit = async (data: Omit<DbConnectionUpdateDto, "id">) => {
		if (!database?.id) {
			toast("Ошибка: ID базы данных не найден");
			return;
		}

		updateDatabaseMutation.mutate({
			data: {
				...data,
				id: database.id,
			},
		});
	};

	const getDefaultValues = () => {
		if (!database) return undefined;

		let icon = "👨‍💻";
		let name = database.name || "";

		const emojiMatch = name.match(/^([\p{Emoji_Presentation}\p{Emoji}\uFE0F\u200D]+)\s*(.*)$/u);
		if (emojiMatch) {
			icon = emojiMatch[1];
			name = emojiMatch[2] || "";
		}

		return {
			icon,
			name,
			host: database.host || "",
			port: database.port || 5432,
			database: database.database || "",
			username: database.username || "",
			password: "",
		};
	};

	return (
		<Dialog open={isOpen} onOpenChange={close}>
			<DialogContent className="max-w-md bg-[var(--card)] rounded-xl">
				<DialogHeader>
					<DialogTitle>Изменение подключения к БД</DialogTitle>
				</DialogHeader>
				<Form
					defaultValues={getDefaultValues()}
					onSubmit={onSubmit}
					submitButtonText="Сохранить"
					isLoading={updateDatabaseMutation.isPending}
				/>
			</DialogContent>
		</Dialog>
	);
};
