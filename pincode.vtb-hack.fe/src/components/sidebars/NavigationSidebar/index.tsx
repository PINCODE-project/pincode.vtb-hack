"use client";

import { ComponentProps, ReactNode } from "react";
import {
	Sidebar,
	SidebarContent,
	SidebarFooter,
	SidebarGroup,
	SidebarGroupContent,
	SidebarHeader,
	SidebarMenu,
	SidebarMenuItem,
} from "@pin-code/ui-kit";
import { ThemeToggle } from "@components/theme-toggle";
import { SidebarLogo } from "./SidebarLogo";
import { SidebarNavigation } from "./SidebarNavigation";
import { OnboardingButton } from "./OnboardingButton";

type Props = ComponentProps<typeof Sidebar> & {
	children?: ReactNode;
};

export function NavigationSidebar({ children, ...props }: Props) {
	return (
		<Sidebar className="overflow-hidden *:data-[sidebar=sidebar]:flex-row" {...props}>
			<Sidebar collapsible="none" className="w-[180px]! border-r">
				<SidebarHeader>
					<SidebarMenu>
						<SidebarMenuItem>
							<SidebarLogo />
						</SidebarMenuItem>
					</SidebarMenu>
				</SidebarHeader>
				<SidebarContent>
					<SidebarGroup>
						<SidebarGroupContent className="px-1.5 md:px-0">
							<SidebarNavigation />
						</SidebarGroupContent>
					</SidebarGroup>
				</SidebarContent>
				<SidebarFooter>
					<div className="flex gap-2 flex-row">
						<OnboardingButton />
						<ThemeToggle />
					</div>
				</SidebarFooter>
			</Sidebar>

			{children}
		</Sidebar>
	);
}
