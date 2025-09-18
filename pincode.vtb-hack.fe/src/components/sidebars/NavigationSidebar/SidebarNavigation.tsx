import { SidebarMenu, SidebarMenuButton, SidebarMenuItem } from "@pin-code/ui-kit";
import { Code, Database, BookOpen } from "lucide-react";
import { useParams, usePathname } from "next/navigation";
import Link from "next/link";

export const SidebarNavigation = () => {
	const { databaseId } = useParams();
	const pathname = usePathname();
	const isOnDatabasePage = pathname.startsWith("/databases");
	const isOnQueriesPage = pathname.startsWith("/queries");
	const isOnDocumentationPage = pathname.startsWith("/documentation");

	return (
		<SidebarMenu>
			<SidebarMenuItem>
				<SidebarMenuButton
					tooltip={{ children: "Базы данных" }}
					isActive={isOnDatabasePage}
					className="px-2.5 md:px-2"
					asChild
				>
					<Link href={"/databases"}>
						<Database />
						<span>Базы данных</span>
					</Link>
				</SidebarMenuButton>
			</SidebarMenuItem>

			<SidebarMenuItem>
				<SidebarMenuButton
					tooltip={{ children: "SQL запросы" }}
					isActive={isOnQueriesPage}
					className="px-2.5 md:px-2"
					asChild
				>
					<Link href={isOnDatabasePage && databaseId ? `/queries/${databaseId}` : "/queries"}>
						<Code />
						<span>SQL запросы</span>
					</Link>
				</SidebarMenuButton>
			</SidebarMenuItem>

			<SidebarMenuItem>
				<SidebarMenuButton
					tooltip={{ children: "Документация" }}
					isActive={isOnDocumentationPage}
					className="px-2.5 md:px-2"
					asChild
				>
					<Link href={"/documentation"}>
						<BookOpen />
						<span>Документация</span>
					</Link>
				</SidebarMenuButton>
			</SidebarMenuItem>
		</SidebarMenu>
	);
};
