"use client";
import React from "react";

import "@pin-code/ui-kit/styles";
import "@styles/globals.css";
import { cn, SidebarInset, SidebarProvider } from "@pin-code/ui-kit";
import { Providers } from "@/components";
import { NavigationSidebar } from "@components/sidebars/NavigationSidebar";
import { DatabasesSidebar } from "@components/sidebars/DatabasesSidebar";
import { CreateDatabaseModal } from "@components/database/CreateDatabaseModal";
import { Onboarding } from "@components/Onboarding";

export default function AppLayout({ children }: { children: React.ReactNode }) {
	return (
		<body className={cn("relative h-full font-sans antialiased")}>
			<Providers>
				<SidebarProvider
					style={
						{
							"--sidebar-width": "450px",
						} as React.CSSProperties
					}
				>
					<NavigationSidebar>
						<DatabasesSidebar />
					</NavigationSidebar>
					<SidebarInset>
						{children}

						<CreateDatabaseModal />
						<Onboarding />
					</SidebarInset>
				</SidebarProvider>
			</Providers>
		</body>
	);
}
