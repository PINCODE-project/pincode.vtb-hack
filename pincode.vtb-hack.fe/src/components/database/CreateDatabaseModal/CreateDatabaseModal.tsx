"use client";
import { Dialog, DialogContent, DialogHeader, DialogTitle, toast } from "@pin-code/ui-kit";
import { DbConnectionCreateDto } from "@generated";
import { usePostApiDbConnections } from "@generated/hooks/DbConnection/usePostApiDbConnections";
import { useCreateDatabaseModalStore } from "@store/database/create-modal.ts";

import { Form } from "./Form";
import { getQueryClient } from "@/lib/query-client.ts";
import { useRouter } from "next/navigation";

type Props = {
	isCreating?: boolean;
};

export const CreateDatabaseModal = ({ isCreating = true }: Props) => {
	const router = useRouter();
	const { isOpen, close } = useCreateDatabaseModalStore();
	const queryClient = getQueryClient();

	const createDatabaseMutation = usePostApiDbConnections({
		mutation: {
			onSuccess: async (data) => {
				await queryClient.invalidateQueries();
				close();
				toast("Подключение к базе данных успешно создано!");
				router.push(`/databases/${data.data}`);
			},
			onError: () => {
				toast("Ошибка при создании подключения к базе данных!");
			},
		},
	});

	const onSubmit = async (data: DbConnectionCreateDto) => {
		if (isCreating) {
			createDatabaseMutation.mutate({ data });
		} else {
			throw new Error("Редактирование подключения пока не реализовано");
		}
	};

	return (
		<Dialog open={isOpen} onOpenChange={close}>
			<DialogContent className="max-w-md bg-[var(--card)] rounded-xl" >
				<DialogHeader>
					<DialogTitle>{isCreating ? "Создание" : "Изменение"} подключения к БД</DialogTitle>
				</DialogHeader>
				<Form
					onSubmit={onSubmit}
					submitButtonText={isCreating ? "Создать" : "Сохранить"}
					isLoading={createDatabaseMutation.isPending}
				/>
			</DialogContent>
		</Dialog>
	);
};
