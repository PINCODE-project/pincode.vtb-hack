"use client";

import {
	Collapsible,
	Sidebar,
	SidebarContent,
	SidebarGroup,
	SidebarGroupContent,
	SidebarGroupLabel,
	SidebarHeader,
	SidebarInput,
	SidebarMenu,
	SidebarMenuButton,
	SidebarMenuItem,
} from "@pin-code/ui-kit";
import { DatabaseMenu } from "@components/sidebars/DatabasesSidebar/DatabaseMenu.tsx";
import { CreateDatabaseButton } from "@components/sidebars/DatabasesSidebar/CreateDatabaseButton.tsx";
import Link from "next/link";
import { useGetApiDbConnectionsFind } from "@generated";
import { useParams, usePathname } from "next/navigation";
import { ChangeEvent, useState } from "react";

export const DatabasesSidebar = () => {
	const pathname = usePathname();
	const { databaseId } = useParams();

	const [search, setSearch] = useState("");

	const { data: databases } = useGetApiDbConnectionsFind({ Search: search });

	return (
		<Sidebar collapsible="none" className="hidden flex-1 md:flex w-[calc(var(--sidebar-width)-180px)] border-l">
			<SidebarHeader className="gap-3.5 border-b p-4">
				<SidebarInput
					placeholder="Поиск БД"
					value={search}
					onChange={(e: ChangeEvent<HTMLInputElement>) => setSearch(e.target.value)}
				/>
			</SidebarHeader>
			<SidebarContent>
				<SidebarGroup>
					<SidebarGroupLabel>Базы данных</SidebarGroupLabel>
					<SidebarGroupContent>
						<SidebarMenu>
							{databases?.map((database) => (
								<Collapsible key={database.id}>
									<SidebarMenuItem>
										<SidebarMenuButton isActive={databaseId === database.id} asChild>
											<Link href={`/${pathname.split("/")[1]}/${database.id}`}>
												{database.name}
											</Link>
										</SidebarMenuButton>

										<DatabaseMenu database={database} />
									</SidebarMenuItem>
								</Collapsible>
							))}

							<CreateDatabaseButton />
						</SidebarMenu>
					</SidebarGroupContent>
				</SidebarGroup>
			</SidebarContent>
		</Sidebar>
	);
};
