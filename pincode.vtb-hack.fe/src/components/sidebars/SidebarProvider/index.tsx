"use client";

import { SidebarProvider as SP } from "@pin-code/ui-kit";
import { ComponentProps, FC } from "react";

export const SidebarProvider: FC<ComponentProps<"div">> = ({ children, ...props }) => {
	return <SP {...props}>{children}</SP>;
};
