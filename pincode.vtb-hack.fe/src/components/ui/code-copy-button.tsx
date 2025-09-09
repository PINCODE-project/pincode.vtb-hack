"use client";

import React, { useState, useCallback } from "react";
import { Button } from "@pin-code/ui-kit";
import { Copy, Check } from "lucide-react";
import { format } from "sql-formatter";

interface CodeCopyButtonProps {
	/** Код для копирования */
	code: string;
	/** Уникальный идентификатор для отслеживания состояния копирования */
	copyId?: string;
	/** Язык кода для форматирования (по умолчанию 'sql') */
	language?: "sql" | "javascript" | "typescript" | "json" | "text";
	/** CSS классы для кнопки */
	className?: string;
	/** Обработчик клика для предотвращения всплытия событий */
	onClick?: (event: React.MouseEvent) => void;
	/** Показывать ли кнопку постоянно (по умолчанию false - показывать только при hover) */
	alwaysVisible?: boolean;
	/** Позиция кнопки (по умолчанию 'top-right') */
	position?: "top-right" | "top-left" | "bottom-right" | "bottom-left";
	/** Размер кнопки */
	size?: "default" | "sm" | "lg" | "icon";
}

export function CodeCopyButton({
	code,
	copyId,
	language = "sql",
	className = "",
	onClick,
	alwaysVisible = false,
	position = "top-right",
	size = "sm",
}: CodeCopyButtonProps) {
	const [copiedId, setCopiedId] = useState<string | null>(null);

	// Форматирование кода в зависимости от языка
	const formatCode = useCallback(
		(rawCode: string) => {
			if (!rawCode) return "";

			try {
				if (language === "sql") {
					return format(rawCode, {
						language: "postgresql",
						tabWidth: 2,
						useTabs: false,
						keywordCase: "upper",
						identifierCase: "lower",
						indentStyle: "tabularLeft",
						functionCase: "upper",
					});
				}
				// Для других языков возвращаем как есть
				return rawCode;
			} catch {
				return rawCode; // Если форматирование не удалось, возвращаем как есть
			}
		},
		[language],
	);

	// Обработчик копирования
	const handleCopy = useCallback(
		async (event: React.MouseEvent) => {
			// Останавливаем всплытие события
			event.stopPropagation();

			// Вызываем внешний обработчик если есть
			onClick?.(event);

			if (!code) return;

			try {
				const formattedCode = formatCode(code);
				await navigator.clipboard.writeText(formattedCode);

				// Устанавливаем состояние копирования
				const id = copyId || "default";
				setCopiedId(id);

				// Сбрасываем состояние через 2 секунды
				setTimeout(() => {
					setCopiedId(null);
				}, 2000);
			} catch (error) {
				console.error("Ошибка при копировании кода:", error);
			}
		},
		[code, copyId, formatCode, onClick],
	);

	// Определяем позицию кнопки
	const getPositionClasses = () => {
		switch (position) {
			case "top-left":
				return "top-3 left-3";
			case "bottom-right":
				return "bottom-3 right-3";
			case "bottom-left":
				return "bottom-3 left-3";
			case "top-right":
			default:
				return "top-3 right-3";
		}
	};

	// Определяем видимость кнопки
	const visibilityClasses = alwaysVisible ? "opacity-100" : "opacity-0 group-hover:opacity-100";

	const isCopied = copiedId === (copyId || "default");

	return (
		<Button
			variant="default"
			size={size}
			className={`absolute ${getPositionClasses()} z-10 ${visibilityClasses} transition-opacity text-white dark:text-black ${className}`}
			onClick={handleCopy}
			title={isCopied ? "Скопировано!" : "Копировать код"}
		>
			{isCopied ? <Check className="h-4 w-4" /> : <Copy className="h-4 w-4" />}
		</Button>
	);
}
