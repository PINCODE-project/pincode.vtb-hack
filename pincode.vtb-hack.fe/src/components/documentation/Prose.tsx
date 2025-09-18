import React from "react";
import { cn } from "@pin-code/ui-kit";

/**
 * Компонент для стилизованного текстового контента с prose стилями
 */
export function Prose({ className, ...props }: React.ComponentProps<"div">) {
	return (
		<div
			className={cn(
				"prose prose-sm max-w-none text-foreground prose-zinc dark:prose-invert",
				"prose-a:font-medium prose-a:break-words prose-a:text-foreground prose-a:underline prose-a:underline-offset-4",
				"prose-code:rounded-md prose-code:border prose-code:bg-muted/50 prose-code:px-[0.3rem] prose-code:py-[0.2rem] prose-code:text-sm prose-code:font-normal prose-code:before:content-none prose-code:after:content-none",
				className,
			)}
			{...props}
		/>
	);
}
