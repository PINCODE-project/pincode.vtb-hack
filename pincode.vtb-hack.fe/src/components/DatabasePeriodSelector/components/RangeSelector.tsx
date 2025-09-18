"use client";

import React from "react";
import { Calendar } from "lucide-react";
import {
	Button,
	Calendar as CalendarComponent,
	Popover,
	PopoverContent,
	PopoverTrigger,
	Input,
	Label,
} from "@pin-code/ui-kit";
import { formatDateRange } from "../utils/time";

export interface DateRange {
	start: Date;
	end: Date;
}

export interface RangeSelectorProps {
	/** Текущий выбранный диапазон */
	value: DateRange;
	/** Callback при изменении диапазона */
	onChange: (range: DateRange) => void;
	/** Минимальная дата для выбора */
	minDate?: Date;
	/** Максимальная дата для выбора */
	maxDate?: Date;
	/** Отключен ли компонент */
	disabled?: boolean;
	/** Дополнительные CSS классы */
	className?: string;
}

export function RangeSelector({ value, onChange, minDate, maxDate, disabled = false, className }: RangeSelectorProps) {
	const [isOpen, setIsOpen] = React.useState(false);
	const [internalRange, setInternalRange] = React.useState<{
		from: Date | undefined;
		to: Date | undefined;
	}>({
		from: value.start,
		to: value.end,
	});

	const [startTime, setStartTime] = React.useState(() => {
		return value.start.toTimeString().slice(0, 5); // HH:MM
	});

	const [endTime, setEndTime] = React.useState(() => {
		return value.end.toTimeString().slice(0, 5); // HH:MM
	});

	// Синхронизируем внутреннее состояние с внешним
	React.useEffect(() => {
		setInternalRange({
			from: value.start,
			to: value.end,
		});
		setStartTime(value.start.toTimeString().slice(0, 5));
		setEndTime(value.end.toTimeString().slice(0, 5));
	}, [value.start, value.end]);

	const handleDateSelect = (selectedRange: { from: Date | undefined; to: Date | undefined }) => {
		setInternalRange(selectedRange);
		// Календарь больше не закрывается автоматически
		// Пользователь должен нажать "Применить"
	};

	const handleCancel = () => {
		// Возвращаем к исходному состоянию
		setInternalRange({
			from: value.start,
			to: value.end,
		});
		setStartTime(value.start.toTimeString().slice(0, 5));
		setEndTime(value.end.toTimeString().slice(0, 5));
		setIsOpen(false);
	};

	const combineDateTime = (date: Date, timeString: string): Date => {
		const [hours, minutes] = timeString.split(":").map(Number);
		const combined = new Date(date);
		combined.setHours(hours, minutes, 0, 0);
		return combined;
	};

	const handleApply = () => {
		if (internalRange.from && internalRange.to) {
			const startWithTime = combineDateTime(internalRange.from, startTime);
			const endWithTime = combineDateTime(internalRange.to, endTime);

			const newRange: DateRange = {
				start: startWithTime,
				end: endWithTime,
			};
			onChange(newRange);
		}
		setIsOpen(false);
	};

	return (
		<Popover open={isOpen} onOpenChange={setIsOpen}>
			<PopoverTrigger asChild>
				<Button
					size="sm"
					variant="outline"
					disabled={disabled}
					className={`justify-start text-left font-normal ${className}`}
				>
					<Calendar className="mr-2 h-4 w-4" />
					{formatDateRange(value.start, value.end)}
				</Button>
			</PopoverTrigger>
			<PopoverContent className="w-auto p-0" align="start">
				<div className="p-4">
					<CalendarComponent
						mode="range"
						selected={{
							from: internalRange.from,
							to: internalRange.to,
						}}
						onSelect={(selectedRange) => {
							if (selectedRange) {
								handleDateSelect({
									from: selectedRange.from,
									to: selectedRange.to,
								});
							}
						}}
						disabled={(date) => {
							if (minDate && date < minDate) return true;
							if (maxDate && date > maxDate) return true;
							return false;
						}}
						numberOfMonths={2}
						defaultMonth={value.start}
						captionLayout="dropdown"
					/>
					{/* Инпуты времени */}
					<div className="mt-4 space-y-3 border-t pt-4">
						<div className="grid grid-cols-2 gap-4">
							<div className="space-y-2">
								<Label htmlFor="start-time" className="text-sm font-medium">
									Начало
								</Label>
								<Input
									id="start-time"
									type="time"
									value={startTime}
									onChange={(e) => setStartTime(e.target.value)}
									className="text-sm"
								/>
							</div>
							<div className="space-y-2">
								<Label htmlFor="end-time" className="text-sm font-medium">
									Конец
								</Label>
								<Input
									id="end-time"
									type="time"
									value={endTime}
									onChange={(e) => setEndTime(e.target.value)}
									className="text-sm"
								/>
							</div>
						</div>

						<div className="flex justify-end gap-2 pt-2">
							<Button variant="outline" size="sm" onClick={handleCancel}>
								Отмена
							</Button>
							<Button size="sm" onClick={handleApply} disabled={!internalRange.from || !internalRange.to}>
								Применить
							</Button>
						</div>
					</div>
				</div>
			</PopoverContent>
		</Popover>
	);
}
