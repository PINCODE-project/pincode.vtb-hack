"use client";

import React from "react";
import CodeMirror from "@uiw/react-codemirror";
import { sql } from "@codemirror/lang-sql";
import { basicDark } from "@uiw/codemirror-theme-basic";
import { EditorView, keymap } from "@codemirror/view";
import { autocompletion, completionKeymap } from "@codemirror/autocomplete";
import { useTheme } from "next-themes";
import { cn } from "@pin-code/ui-kit";
import { sqlCompletions } from "./sql-autocomplete";

interface SqlEditorProps {
	value: string;
	onChange: (value: string) => void;
	onPaste?: (e: React.ClipboardEvent<HTMLDivElement>) => void;
	placeholder?: string;
	className?: string;
	minHeight?: string;
	readOnly?: boolean;
	actions?: React.ReactNode;
}

export function SqlEditor({
	value,
	onChange,
	onPaste,
	placeholder = "Введите ваш SQL запрос здесь...",
	className,
	minHeight = "300px",
	readOnly = false,
	actions,
}: SqlEditorProps) {
	const { theme } = useTheme();

	const extensions = [
		sql(),
		autocompletion({
			override: [sqlCompletions],
			closeOnBlur: false,
			activateOnTyping: true,
			maxRenderedOptions: 15,
		}),
		keymap.of(completionKeymap),
		EditorView.theme({
			"&": {
				fontSize: "14px",
				fontFamily: "ui-monospace, SFMono-Regular, 'SF Mono', monospace",
			},
			".cm-content": {
				padding: "16px",
				minHeight: minHeight,
			},
			".cm-focused": {
				outline: "none",
			},
			".cm-editor": {
				borderRadius: "8px",
			},
			".cm-scroller": {
				fontFamily: "ui-monospace, SFMono-Regular, 'SF Mono', monospace",
			},
			".cm-tooltip-autocomplete": {
				"& > ul > li[aria-selected]": {
					backgroundColor: "var(--color-accent)",
					color: "var(--color-accent-foreground)",
				},
			},
			".cm-completionLabel": {
				fontFamily: "ui-monospace, SFMono-Regular, 'SF Mono', monospace",
			},
		}),
		EditorView.lineWrapping,
	];

	return (
		<div
			className={cn("group relative overflow-hidden rounded-md border border-input bg-background", className)}
			onPaste={onPaste}
		>
			<CodeMirror
				value={value}
				onChange={onChange}
				extensions={extensions}
				theme={theme === "dark" ? basicDark : undefined}
				placeholder={placeholder}
				readOnly={readOnly}
				basicSetup={{
					lineNumbers: true,
					foldGutter: true,
					dropCursor: true,
					allowMultipleSelections: true,
					indentOnInput: true,
					bracketMatching: true,
					closeBrackets: true,
					autocompletion: true,
					highlightSelectionMatches: true,
					rectangularSelection: true,
					crosshairCursor: true,
				}}
			/>
			{actions && (
				<div className="absolute bottom-3 right-6 flex gap-2 justify-end opacity-0 group-hover:opacity-100 transition-opacity duration-200">
					{actions}
				</div>
			)}
		</div>
	);
}
