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
import type { DbConnectionDto } from "@generated";
import { useDeleteDatabaseModalStore } from "@store/database/delete-modal.ts";

type Props = {
	database: DbConnectionDto;
};

export const DatabaseMenu = ({ database }: Props) => {
	const { isMobile } = useSidebar();
	const {
		isOpen: isDeleteDatabaseModalOpen,
		open: openDeleteDatabaseModal,
		close: closeDeleteDatabaseModal,
	} = useDeleteDatabaseModalStore();

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
				<DropdownMenuItem>
					<Edit className="text-muted-foreground" />
					<span>Редактировать</span>
				</DropdownMenuItem>
				<DropdownMenuSeparator />
				<DropdownMenuItem onClick={openDeleteDatabaseModal}>
					<Trash2 className="text-muted-foreground" />
					<span>Удалить</span>
				</DropdownMenuItem>
			</DropdownMenuContent>

			<DeleteDatabaseModal
				isOpen={isDeleteDatabaseModalOpen}
				onClose={closeDeleteDatabaseModal}
				databaseId={database.id}
				databaseName={database.name || "Без названия"}
			/>
		</DropdownMenu>
	);
};
