"use client";

import React from "react";
import { Button } from "@pin-code/ui-kit";
import { TIME_PRESETS } from "../utils/time";
import type { DateRange } from "./RangeSelector";

export interface TimePresetsProps {
	/** Callback при выборе пресета */
	onPresetSelect: (range: DateRange) => void;
	/** Текущий выбранный диапазон для подсветки активного пресета */
	currentRange?: DateRange;
	/** Отключены ли пресеты */
	disabled?: boolean;
	/** Ориентация кнопок */
	orientation?: "horizontal" | "vertical";
	/** Дополнительные CSS классы */
	className?: string;
}

export function TimePresets({
	onPresetSelect,
	currentRange,
	disabled = false,
	orientation = "horizontal",
	className,
}: TimePresetsProps) {
	const handlePresetClick = (preset: (typeof TIME_PRESETS)[0]) => {
		const range = preset.getValue();
		onPresetSelect(range);
	};

	const isPresetActive = (preset: (typeof TIME_PRESETS)[0]): boolean => {
		if (!currentRange) return false;

		const presetRange = preset.getValue();
		const timeDiffStart = Math.abs(currentRange.start.getTime() - presetRange.start.getTime());
		const timeDiffEnd = Math.abs(currentRange.end.getTime() - presetRange.end.getTime());

		// Считаем пресет активным, если разница не более 1 минуты
		const tolerance = 60000; // 1 минута в миллисекундах
		return timeDiffStart < tolerance && timeDiffEnd < tolerance;
	};

	const containerClasses = orientation === "horizontal" ? "flex flex-row gap-2 flex-wrap" : "flex flex-col gap-2";

	return (
		<div className={`${containerClasses} ${className}`}>
			{TIME_PRESETS.map((preset) => {
				const isActive = isPresetActive(preset);

				return (
					<Button
						key={preset.label}
						variant={"ghost"}
						size="sm"
						disabled={disabled}
						onClick={() => handlePresetClick(preset)}
						className="whitespace-nowrap"
					>
						{preset.label}
					</Button>
				);
			})}
		</div>
	);
}
