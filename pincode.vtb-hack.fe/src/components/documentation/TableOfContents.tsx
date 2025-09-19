"use client";

import React from "react";
import * as CollapsiblePrimitive from "@radix-ui/react-collapsible";

interface TableOfContentsProps extends React.ComponentPropsWithoutRef<typeof CollapsiblePrimitive.Root> {
	items: Array<{
		title: string;
		url: string;
		depth: number;
	}>;
	title?: string;
}

/**
 * Компонент оглавления с иерархическим отображением ссылок
 */
export function TableOfContents({ items, title = "Оглавление", ...props }: TableOfContentsProps) {
	return (
		<div className="flex flex-col p-4 pt-0 text-sm text-muted-foreground" {...props}>
			<h2 className="text-md">{title}</h2>
			{items.map((item) => (
				<a
					key={item.url}
					href={item.url}
					className="border-l py-1.5 transition-colors hover:text-accent-foreground"
					style={{
						paddingInlineStart: `${Math.max(item.depth, 0) * 0.75}rem`,
					}}
				>
					{item.title}
				</a>
			))}
		</div>
	);
}
