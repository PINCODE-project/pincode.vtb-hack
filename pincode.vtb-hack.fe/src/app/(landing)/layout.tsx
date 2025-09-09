import React from "react";
import { cn } from "@pin-code/ui-kit";
import { Providers } from "@/components";

export default function LandingLayout({ children }: { children: React.ReactNode }) {
	return (
		<body className={cn("dark relative h-full font-sans antialiased")}>
			<Providers>{children}</Providers>
		</body>
	);
}
