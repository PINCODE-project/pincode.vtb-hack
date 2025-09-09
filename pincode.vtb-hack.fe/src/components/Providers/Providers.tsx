"use client";

import React from "react";
import { QueryClientProvider } from "@tanstack/react-query";
import { getQueryClient } from "@/lib/query-client.ts";

import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { ThemeProvider } from "@components/theme-provider.tsx";
import { Toaster } from "@pin-code/ui-kit";

export function Providers({ children }: { children: React.ReactNode }) {
	const queryClient = getQueryClient();

	return (
		<ThemeProvider attribute="class" defaultTheme="system" enableSystem disableTransitionOnChange>
			<QueryClientProvider client={queryClient}>
				{children}
				{process.env.NODE_ENV === "development" && <ReactQueryDevtools initialIsOpen={false} />}
				<Toaster />
			</QueryClientProvider>
		</ThemeProvider>
	);
}
