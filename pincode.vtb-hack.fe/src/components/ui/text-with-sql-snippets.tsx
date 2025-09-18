"use client";

import React from "react";
import { extractSqlSnippets, replaceSqlSnippetsWithPlaceholders } from "@/utils/sqlSnippetDetector";
import { SqlSnippetBadge } from "./sql-snippet-badge";

export interface TextWithSqlSnippetsProps {
	text: string;
	className?: string;
}

/**
 * Компонент для отображения текста с автоматическим извлечением и отображением SQL сниппетов
 */
export function TextWithSqlSnippets({ text, className }: TextWithSqlSnippetsProps) {
	// Извлекаем SQL сниппеты
	const sqlSnippets = extractSqlSnippets(text);

	// Если нет SQL сниппетов, просто возвращаем текст
	if (sqlSnippets.length === 0) {
		return <span className={className}>{text}</span>;
	}

	// Заменяем SQL сниппеты на плейсхолдеры
	const textWithPlaceholders = replaceSqlSnippetsWithPlaceholders(text, sqlSnippets);

	// Разбиваем текст по плейсхолдерам
	const parts = textWithPlaceholders.split(/(__SQL_SNIPPET_\d+__)/);

	return (
		<span className={className}>
			{parts.map((part, index) => {
				// Проверяем, является ли часть плейсхолдером
				const placeholderMatch = part.match(/^__SQL_SNIPPET_(\d+)__$/);

				if (placeholderMatch) {
					const snippetIndex = parseInt(placeholderMatch[1], 10);
					const snippet = sqlSnippets[snippetIndex];

					if (snippet) {
						return (
							<SqlSnippetBadge
								key={`snippet-${index}`}
								sql={snippet.sql}
								type={snippet.type}
								className="mx-1"
							/>
						);
					}
				}

				// Обычный текст
				return <span key={`text-${index}`}>{part}</span>;
			})}
		</span>
	);
}
