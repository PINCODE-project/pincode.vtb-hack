"use client";

import {
	DropdownMenu,
	DropdownMenuContent,
	DropdownMenuItem,
	DropdownMenuSeparator,
	DropdownMenuTrigger,
	SidebarMenuAction,
	useSidebar,
} from "@pin-code/ui-kit";
import { Edit, MoreHorizontal, Trash2 } from "lucide-react";
import { DeleteDatabaseModal } from "@components/database/DeleteDatabaseModal";
import { EditDatabaseModal } from "@components/database/EditDatabaseModal";
import type { DbConnectionDto } from "@generated";
import { useDeleteDatabaseModalStore } from "@store/database/delete-modal.ts";
import { useEditDatabaseModalStore } from "@store/database/edit-modal.ts";

type Props = {
	database: DbConnectionDto;
};

export const DatabaseMenu = ({ database }: Props) => {
	const { isMobile } = useSidebar();
	const {
		isOpen: isDeleteDatabaseModalOpen,
		databaseId: deleteDatabaseId,
		databaseName: deleteDatabaseName,
		open: openDeleteDatabaseModal,
		close: closeDeleteDatabaseModal,
	} = useDeleteDatabaseModalStore();
	const { open: openEditDatabaseModal } = useEditDatabaseModalStore();

	return (
		<DropdownMenu>
			<DropdownMenuTrigger asChild>
				<SidebarMenuAction showOnHover>
					<MoreHorizontal />
					<span className="sr-only">More</span>
				</SidebarMenuAction>
			</DropdownMenuTrigger>
			<DropdownMenuContent
				className="w-56 rounded-lg"
				side={isMobile ? "bottom" : "right"}
				align={isMobile ? "end" : "start"}
			>
				<DropdownMenuItem onClick={() => openEditDatabaseModal(database)}>
					<Edit className="text-muted-foreground" />
					<span>Редактировать</span>
				</DropdownMenuItem>
				<DropdownMenuSeparator />
				<DropdownMenuItem onClick={() => openDeleteDatabaseModal(database.id, database.name || "Без названия")}>
					<Trash2 className="text-muted-foreground" />
					<span>Удалить</span>
				</DropdownMenuItem>
			</DropdownMenuContent>

			<DeleteDatabaseModal
				isOpen={isDeleteDatabaseModalOpen}
				onClose={closeDeleteDatabaseModal}
				databaseId={deleteDatabaseId || ""}
				databaseName={deleteDatabaseName || "Без названия"}
			/>
			<EditDatabaseModal />
		</DropdownMenu>
	);
};
