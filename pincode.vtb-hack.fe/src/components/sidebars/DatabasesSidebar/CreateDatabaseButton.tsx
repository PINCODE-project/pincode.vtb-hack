import { SidebarMenuButton, SidebarMenuItem } from "@pin-code/ui-kit";
import { Plus } from "lucide-react";
import { useCreateDatabaseModalStore } from "@store/database/create-modal.ts";

export const CreateDatabaseButton = () => {
	const { open } = useCreateDatabaseModalStore();

	return (
		<SidebarMenuItem>
			<SidebarMenuButton className="text-sidebar-foreground/70" onClick={open}>
				<Plus />
				<span>Добавить подключение</span>
			</SidebarMenuButton>
		</SidebarMenuItem>
	);
};
