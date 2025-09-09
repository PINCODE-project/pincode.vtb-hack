"use client";

import { Button, Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, toast } from "@pin-code/ui-kit";
import { AlertTriangle } from "lucide-react";
import { useDeleteApiDbConnectionsId } from "@generated";
import { getQueryClient } from "@/lib/query-client";
import { useParams, useRouter } from "next/navigation";

type Props = {
	isOpen: boolean;
	onClose: () => void;
	databaseId: string;
	databaseName: string;
};

export const DeleteDatabaseModal = ({ isOpen, onClose, databaseId, databaseName }: Props) => {
	const queryClient = getQueryClient();
	const router = useRouter();
	const params = useParams();
	const currentDatabaseId = params?.databaseId;

	const deleteConnectionMutation = useDeleteApiDbConnectionsId({
		mutation: {
			onSuccess: async () => {
				await queryClient.invalidateQueries();
				onClose();
				toast("Подключение к базе данных успешно удалено!");

				if (currentDatabaseId === databaseId) {
					router.push("/databases");
				}
			},
			onError: (error) => {
				toast(`Ошибка при удалении подключения: ${error.message || "Неизвестная ошибка"}`);
			},
		},
	});

	const handleDelete = () => {
		deleteConnectionMutation.mutate({ id: databaseId });
	};

	return (
		<Dialog open={isOpen} onOpenChange={onClose}>
			<DialogContent className="max-w-md bg-[var(--card)] rounded-xl">
				<DialogHeader>
					<div className="flex items-center gap-3">
						<div className="flex h-10 w-10 items-center justify-center rounded-full bg-red-50 dark:bg-red-900/20">
							<AlertTriangle className="h-5 w-5 text-red-600 dark:text-red-400" />
						</div>
						<div>
							<DialogTitle>Удалить подключение</DialogTitle>
							<DialogDescription className="mt-1">Это действие нельзя отменить</DialogDescription>
						</div>
					</div>
				</DialogHeader>

				<div className="mt-4">
					<p className="text-sm text-muted-foreground">
						Вы уверены, что хотите удалить подключение к базе данных{" "}
						<span className="font-medium text-foreground">&#34;{databaseName}&#34;</span>?
					</p>
					<p className="mt-2 text-sm text-muted-foreground">
						Все связанные данные и история анализа будут удалены безвозвратно.
					</p>
				</div>

				<div className="mt-6 flex gap-3 justify-end">
					<Button variant="outline" onClick={onClose} disabled={deleteConnectionMutation.isPending}>
						Отмена
					</Button>
					<Button variant="destructive" onClick={handleDelete} disabled={deleteConnectionMutation.isPending}>
						{deleteConnectionMutation.isPending ? (
							<div className="h-4 w-4 animate-spin rounded-full border-2 border-white border-r-transparent"></div>
						) : (
							"Удалить"
						)}
					</Button>
				</div>
			</DialogContent>
		</Dialog>
	);
};
