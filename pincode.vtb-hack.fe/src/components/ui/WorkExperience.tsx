"use client";

import React from "react";
import * as CollapsiblePrimitive from "@radix-ui/react-collapsible";
import { cn, Collapsible, CollapsibleContent, CollapsibleTrigger } from "@pin-code/ui-kit";
import { ChevronsDownUpIcon, ChevronsUpDownIcon, LucideIcon } from "lucide-react";

export type ExperiencePositionItemType = {
	id: string;
	title: string;
	description?: React.ReactNode;
	icon?: LucideIcon;
	isExpanded?: boolean;
};

/**
 * Компонент для отображения списка элементов опыта/позиций с возможностью раскрытия
 */
export function WorkExperience({
	className,
	positions,
}: {
	className?: string;
	positions: ExperiencePositionItemType[];
}) {
	return (
		<div className={cn("px-4", className)}>
			<div className="relative space-y-4 before:absolute before:left-3 before:h-full before:w-px before:bg-border">
				{positions.map((position) => (
					<ExperiencePositionItem key={position.id} position={position} />
				))}
			</div>
		</div>
	);
}

/**
 * Компонент отдельного элемента позиции в WorkExperience
 */
export function ExperiencePositionItem({ position }: { position: ExperiencePositionItemType }) {
	const ExperienceIcon = position.icon!;

	return (
		<Collapsible defaultOpen={position.isExpanded} asChild>
			<div className="relative last:before:absolute last:before:h-full last:before:w-4 last:before:bg-card">
				<CollapsibleTrigger
					className={cn(
						"group/experience not-prose block w-full text-left select-none",
						"relative before:absolute before:-top-1 before:-right-1 before:-bottom-0.5 before:left-7 before:rounded-lg hover:before:bg-muted/50",
					)}
				>
					<div className="relative z-1 mb-1 flex items-center gap-3">
						<div
							className="flex size-6 shrink-0 items-center justify-center rounded-lg bg-muted text-muted-foreground"
							aria-hidden
						>
							<ExperienceIcon className="size-4" />
						</div>

						<h4 className="flex-1 text-base font-medium text-balance">{position.title}</h4>

						<div className="shrink-0 text-muted-foreground [&_svg]:size-4" aria-hidden>
							<ChevronsDownUpIcon className="hidden group-data-[state=open]/experience:block" />
							<ChevronsUpDownIcon className="hidden group-data-[state=closed]/experience:block" />
						</div>
					</div>
				</CollapsibleTrigger>

				<CollapsibleContent className="overflow-hidden duration-300 data-[state=closed]:animate-collapsible-up data-[state=open]:animate-collapsible-down">
					{position.description && (
						<div className="pt-2 pl-9 whitespace-pre-wrap prose prose-sm max-w-none text-foreground prose-zinc dark:prose-invert prose-a:font-medium prose-a:break-words prose-a:text-foreground prose-a:underline prose-a:underline-offset-4 prose-code:rounded-md prose-code:border prose-code:bg-muted/50 prose-code:px-[0.3rem] prose-code:py-[0.2rem] prose-code:text-sm prose-code:font-normal prose-code:before:content-none prose-code:after:content-none">
							{position.description}
						</div>
					)}
				</CollapsibleContent>
			</div>
		</Collapsible>
	);
}
