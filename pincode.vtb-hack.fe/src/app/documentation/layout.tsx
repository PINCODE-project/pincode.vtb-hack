"use client";
import React from "react";

import "@pin-code/ui-kit/styles";
import "@styles/globals.css";
import { cn, SidebarInset, SidebarProvider } from "@pin-code/ui-kit";
import { Providers } from "@/components";
import { NavigationSidebar } from "@components/sidebars/NavigationSidebar";
import { Onboarding } from "@components/Onboarding";

export default function DocumentationLayout({ children }: { children: React.ReactNode }) {
	return (
		<body className={cn("relative h-full font-sans antialiased")}>
			<Providers>
				<SidebarProvider
					style={
						{
							"--sidebar-width": "180px",
						} as React.CSSProperties
					}
				>
					<NavigationSidebar></NavigationSidebar>
					<SidebarInset>
						{children}
						<Onboarding />
					</SidebarInset>
				</SidebarProvider>
			</Providers>
		</body>
	);
}
