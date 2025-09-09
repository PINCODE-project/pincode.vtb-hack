import React from "react";
import { SidebarMenuButton } from "@pin-code/ui-kit";
import Link from "next/link";

export const SidebarLogo = () => {
	return (
		<SidebarMenuButton size="lg" asChild className="md:h-8 md:p-0">
			<Link href="/databases">
				<div className="bg-foreground text-sidebar-primary-foreground flex aspect-square size-8 items-center justify-center rounded-lg">
					<svg
						width="14"
						height="16"
						viewBox="0 0 210 232"
						fill="none"
						xmlns="http://www.w3.org/2000/svg"
						className="fill-[#33d3d4] dark:fill-background"
					>
						<path
							fillRule="evenodd"
							clipRule="evenodd"
							d="M86.7778 49.6006V0H123.222V49.6006L168.778 23.1894L187 54.8828L141.444 81.294L186.827 107.605L168.605 139.298L123.222 112.987V162H86.7778V112.987L41.3948 139.299L23.1726 107.605L68.5556 81.294L23 54.8828L41.2222 23.1894L86.7778 49.6006Z"
						/>
						<path fillRule="evenodd" clipRule="evenodd" d="M210 232L0 232L3.8147e-06 193L210 193V232Z" />
					</svg>
				</div>
				<div className="grid flex-1 text-left text-sm leading-tight">
					<span className="truncate font-medium">DB Explorer</span>
					<span className="truncate text-xs">Cloud</span>
				</div>
			</Link>
		</SidebarMenuButton>
	);
};
