"use client";

import React, { useState } from "react";
import { Badge } from "@pin-code/ui-kit";
import { Copy, Check, Code2, Database } from "lucide-react";
import { cn } from "@pin-code/ui-kit";

export interface SqlSnippetBadgeProps {
	sql: string;
	type: "command" | "query";
	className?: string;
}

/**
 * Компонент для отображения SQL сниппета в виде кликабельного бейджа
 */
export function SqlSnippetBadge({ sql, type, className }: SqlSnippetBadgeProps) {
	const [copied, setCopied] = useState(false);

	const handleCopy = async () => {
		try {
			await navigator.clipboard.writeText(sql);
			setCopied(true);
			setTimeout(() => setCopied(false), 2000);
		} catch (error) {
			console.error("Ошибка копирования в буфер обмена:", error);
		}
	};

	const getDisplayText = () => {
		// Обрезаем длинные команды для отображения
		if (sql.length > 50) {
			return sql.substring(0, 47) + "...";
		}
		return sql;
	};

	const getIcon = () => {
		return type === "command" ? <Database className="h-3 w-3" /> : <Code2 className="h-3 w-3" />;
	};

	const getVariant = () => {
		return type === "command" ? "default" : "secondary";
	};

	return (
		<Badge
			variant={getVariant()}
			className={cn(
				"inline-flex items-center gap-1 cursor-pointer hover:opacity-80 transition-opacity font-mono text-xs px-2 py-1 max-w-fit",
				className,
			)}
			onClick={handleCopy}
			title={`Нажмите чтобы скопировать: ${sql}`}
		>
			{getIcon()}
			<span className="truncate">{getDisplayText()}</span>
			{copied ? <Check className="h-3 w-3 text-green-500 ml-1" /> : <Copy className="h-3 w-3 ml-1" />}
		</Badge>
	);
}
