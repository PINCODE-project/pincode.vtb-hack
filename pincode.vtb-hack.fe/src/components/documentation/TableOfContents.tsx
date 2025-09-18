"use client";

import React, { useState } from "react";
import { ChevronDown } from "lucide-react";
import { Card, CardContent, CardHeader, cn } from "@pin-code/ui-kit";

interface TableOfContentsItem {
	title: string;
	url: string;
	depth: number;
}

interface TableOfContentsProps {
	items: TableOfContentsItem[];
	title?: string;
	defaultOpen?: boolean;
	className?: string;
}

/**
 * Компонент оглавления в виде карточки-аккордеона
 */
export function TableOfContents({ items, title = "Оглавление", defaultOpen = true, className }: TableOfContentsProps) {
	const [isOpen, setIsOpen] = useState(defaultOpen);

	const handleToggle = () => {
		setIsOpen(!isOpen);
	};

	return (
		<Card className={cn("not-prose", className)}>
			<CardHeader className="cursor-pointer select-none py-3 px-4" onClick={handleToggle}>
				<div className="flex items-center justify-between">
					<h3 className="font-semibold text-base">{title}</h3>
					<ChevronDown className={cn("h-4 w-4 transition-transform duration-200", isOpen && "rotate-180")} />
				</div>
			</CardHeader>

			{isOpen && (
				<CardContent className="px-4 pb-4 pt-0">
					<nav className="space-y-1">
						{items.map((item, index) => (
							<a
								key={index}
								href={item.url}
								className={cn(
									"block py-1.5 text-sm text-muted-foreground transition-colors hover:text-accent-foreground border-l border-transparent hover:border-accent",
									"pl-4", // базовый отступ
								)}
								style={{
									paddingLeft: `${Math.max(item.depth - 1, 0) * 0.75 + 1}rem`,
								}}
							>
								{item.title}
							</a>
						))}
					</nav>
				</CardContent>
			)}
		</Card>
	);
}
