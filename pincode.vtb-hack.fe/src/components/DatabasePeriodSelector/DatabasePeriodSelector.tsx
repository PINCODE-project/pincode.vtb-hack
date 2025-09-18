"use client";

import React from "react";
import { Card, CardContent } from "@pin-code/ui-kit";
import { RangeSelector } from "./components/RangeSelector";
import { TimePresets } from "./components/TimePresets";
import { Timeline } from "./components/Timeline";
import { combineMetricTimestamps, calculateTimelineRange } from "./utils/time";
import type { DateRange } from "./components/RangeSelector";
import type { GroupedTimeMetric } from "./utils/time";

import "./DatabasePeriodSelector.scss";

export interface DatabasePeriodSelectorProps {
	/** Текущий выбранный диапазон */
	selectedRange: DateRange;
	/** Callback при изменении диапазона */
	onRangeChange: (range: DateRange) => void;
	/** Временные метки от разных источников */
	timeData?: {
		autovacuum?: string[];
		cache?: string[];
		index?: string[];
		tempfiles?: string[];
		locks?: string[];
	};
	/** Отключен ли компонент */
	disabled?: boolean;
	/** Дополнительные CSS классы */
	className?: string;
}

export function DatabasePeriodSelector({
	selectedRange,
	onRangeChange,
	timeData = {},
	disabled = false,
	className,
}: DatabasePeriodSelectorProps) {
	// Объединяем и группируем временные метки
	const groupedMetrics: GroupedTimeMetric[] = React.useMemo(() => {
		return combineMetricTimestamps(timeData);
	}, [timeData]);

	// Состояние viewport для таймлайна (может отличаться от общего диапазона)
	const [timelineViewport, setTimelineViewport] = React.useState<DateRange>(() => {
		return calculateTimelineRange(groupedMetrics);
	});

	// Состояние для отслеживания того, загружены ли данные впервые
	const [isInitialDataLoaded, setIsInitialDataLoaded] = React.useState(false);

	// Обновляем viewport когда меняются метрики
	React.useEffect(() => {
		if (groupedMetrics.length > 0) {
			setTimelineViewport(calculateTimelineRange(groupedMetrics));
		}
	}, [groupedMetrics]);

	// Центрируем таймлайн только при первом получении данных
	React.useEffect(() => {
		if (groupedMetrics.length > 0 && !isInitialDataLoaded) {
			// Автоцентрирование: период занимает 1/3 от общей ширины и находится по центру
			const selectionDuration = selectedRange.end.getTime() - selectedRange.start.getTime();
			const targetViewportDuration = selectionDuration * 3;

			const selectionCenter = selectedRange.start.getTime() + selectionDuration / 2;

			// Панорамирование таймлайна для центрирования
			const newStartTime = selectionCenter - targetViewportDuration / 2;

			const centeredViewport = {
				start: new Date(newStartTime),
				end: new Date(newStartTime + targetViewportDuration),
			};

			setTimelineViewport(centeredViewport);
			setIsInitialDataLoaded(true); // Помечаем, что данные загружены и центрирование выполнено
		}
	}, [groupedMetrics.length, selectedRange, isInitialDataLoaded]);

	// Определяем минимальную и максимальную даты для календаря
	const { minDate, maxDate } = React.useMemo(() => {
		if (groupedMetrics.length === 0) {
			return {
				minDate: undefined,
				maxDate: new Date(),
			};
		}

		const times = groupedMetrics.map((m) => m.time);
		return {
			minDate: new Date(Math.min(...times.map((t) => t.getTime()))),
			maxDate: new Date(Math.max(...times.map((t) => t.getTime()))),
		};
	}, [groupedMetrics]);

	// Обработчик изменения диапазона с валидацией
	const handleRangeChange = (newRange: DateRange) => {
		// Убеждаемся, что start меньше end
		if (newRange.start > newRange.end) {
			onRangeChange({
				start: newRange.end,
				end: newRange.start,
			});
		} else {
			onRangeChange(newRange);
		}
	};

	// Обработчик выбора пресета
	const handlePresetSelect = (range: DateRange) => {
		handleRangeChange(range);
	};

	return (
		<div className={`w-full database-period-selector ${className}`}>
			<div className="space-y-4 p-6">
				{/* Верхняя панель с селектором диапазона и пресетами */}
				<div className="flex flex-col sm:flex-row gap-4 items-start sm:items-center justify-between">
					{/* Селектор диапазона */}
					<div className="flex-1 min-w-0">
						<RangeSelector
							value={selectedRange}
							onChange={handleRangeChange}
							minDate={minDate}
							maxDate={maxDate}
							disabled={disabled}
							className="w-full sm:w-auto min-w-[280px]"
						/>
					</div>

					{/* Пресеты */}
					<div className="flex-shrink-0">
						<TimePresets
							onPresetSelect={handlePresetSelect}
							currentRange={selectedRange}
							disabled={disabled}
							orientation="horizontal"
						/>
					</div>
				</div>

				{/* Таймлайн */}
				{groupedMetrics.length > 0 && (
					<div className="space-y-2">
						<Timeline
							metrics={groupedMetrics}
							timeRange={timelineViewport}
							selectedRange={selectedRange}
							onSelectionChange={handleRangeChange}
							onViewportChange={setTimelineViewport}
							height={50}
						/>
					</div>
				)}

				{/* Сообщение, если нет данных */}
				{groupedMetrics.length === 0 && (
					<div className="text-center py-8 text-muted-foreground">
						<p>Нет доступных временных меток</p>
						<p className="text-xs mt-1">Выберите период, для которого доступны данные анализа</p>
					</div>
				)}
			</div>
		</div>
	);
}
