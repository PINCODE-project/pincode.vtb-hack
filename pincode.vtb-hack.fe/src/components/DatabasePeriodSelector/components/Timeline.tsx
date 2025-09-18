"use client";

import React from "react";
import { formatTimelineLabel, METRIC_CONFIGS } from "../utils/time";
import type { GroupedTimeMetric } from "../utils/time";
import type { DateRange } from "./RangeSelector";

export interface TimelineProps {
	/** Сгруппированные метрики для отображения */
	metrics: GroupedTimeMetric[];
	/** Диапазон времени для отображения */
	timeRange: DateRange;
	/** Выбранный диапазон */
	selectedRange: DateRange;
	/** Callback при изменении выбранного диапазона */
	onSelectionChange: (range: DateRange) => void;
	/** Callback при изменении viewport (масштабирование/панорамирование) */
	onViewportChange?: (range: DateRange) => void;
	/** Высота таймлайна */
	height?: number;
	/** Дополнительные CSS классы */
	className?: string;
}

export function Timeline({
	metrics,
	timeRange,
	selectedRange,
	onSelectionChange,
	onViewportChange,
	height = 50,
	className,
}: TimelineProps) {
	const svgRef = React.useRef<SVGSVGElement>(null);
	const [isDragging, setIsDragging] = React.useState<"start" | "end" | "period" | null>(null);
	const [dragStart, setDragStart] = React.useState<{ x: number; originalRange: DateRange } | null>(null);
	const [isPanning, setIsPanning] = React.useState(false);
	const [panStart, setPanStart] = React.useState<{ x: number; startTime: number } | null>(null);
	const [viewport, setViewport] = React.useState<DateRange>(timeRange);

	// Фильтруем метрики по текущему viewport
	const visibleMetrics = metrics.filter((metric) => metric.time >= viewport.start && metric.time <= viewport.end);

	// Преобразует время в X координату (используем viewport вместо timeRange)
	const timeToX = (time: Date): number => {
		if (!svgRef.current) return 0;
		const width = svgRef.current.clientWidth;
		const viewportDuration = viewport.end.getTime() - viewport.start.getTime();
		const ratio = (time.getTime() - viewport.start.getTime()) / viewportDuration;
		return ratio * width;
	};

	// Определяем тип клика по позиции
	const getClickType = (x: number): "start" | "end" | "period" | "outside" => {
		const selectionStartX = timeToX(selectedRange.start);
		const selectionEndX = timeToX(selectedRange.end);
		const handleSize = 8; // Размер области для захвата границы

		// Проверяем клик по левой границе
		if (Math.abs(x - selectionStartX) <= handleSize) {
			return "start";
		}
		// Проверяем клик по правой границе
		if (Math.abs(x - selectionEndX) <= handleSize) {
			return "end";
		}
		// Проверяем клик внутри периода
		if (x >= selectionStartX && x <= selectionEndX) {
			return "period";
		}
		// Клик вне периода
		return "outside";
	};

	// Обработка перетаскивания
	const handleMouseDown = (event: React.MouseEvent<SVGSVGElement>) => {
		if (!svgRef.current) return;

		const rect = svgRef.current.getBoundingClientRect();
		const x = event.clientX - rect.left;
		const clickType = getClickType(x);

		if (clickType === "outside") {
			// Клик вне периода - панорамирование таймлайна
			setIsPanning(true);
			setPanStart({ x, startTime: viewport.start.getTime() });
		} else {
			// Клик по периоду или его границам
			setIsDragging(clickType);
			setDragStart({ x, originalRange: selectedRange });
		}
	};

	const handleMouseMove = React.useCallback(
		(event: MouseEvent) => {
			if (!svgRef.current) return;

			const rect = svgRef.current.getBoundingClientRect();
			const currentX = event.clientX - rect.left;

			if (isDragging && dragStart) {
				const deltaX = currentX - dragStart.x;
				const deltaTime = (deltaX / rect.width) * (viewport.end.getTime() - viewport.start.getTime());

				if (isDragging === "start") {
					// Перетаскивание левой границы
					const newStart = new Date(dragStart.originalRange.start.getTime() + deltaTime);
					if (newStart < dragStart.originalRange.end) {
						onSelectionChange({
							start: newStart,
							end: dragStart.originalRange.end,
						});
					}
				} else if (isDragging === "end") {
					// Перетаскивание правой границы
					const newEnd = new Date(dragStart.originalRange.end.getTime() + deltaTime);
					if (newEnd > dragStart.originalRange.start) {
						onSelectionChange({
							start: dragStart.originalRange.start,
							end: newEnd,
						});
					}
				} else if (isDragging === "period") {
					// Перемещение всего периода
					const newStart = new Date(dragStart.originalRange.start.getTime() + deltaTime);
					const newEnd = new Date(dragStart.originalRange.end.getTime() + deltaTime);
					onSelectionChange({
						start: newStart,
						end: newEnd,
					});
				}
			} else if (isPanning && panStart) {
				// Панорамирование таймлайна
				const deltaX = currentX - panStart.x;
				const viewportDuration = viewport.end.getTime() - viewport.start.getTime();
				const deltaTime = (deltaX / rect.width) * viewportDuration;

				const newStartTime = panStart.startTime - deltaTime;

				const newViewport = {
					start: new Date(newStartTime),
					end: new Date(newStartTime + viewportDuration),
				};

				setViewport(newViewport);
				if (onViewportChange) {
					onViewportChange(newViewport);
				}
			}
		},
		[isDragging, dragStart, isPanning, panStart, onSelectionChange, onViewportChange, viewport],
	);

	const handleMouseUp = React.useCallback(() => {
		if (isDragging) {
			// После изменения периода автоматически масштабируем viewport
			// так чтобы период занимал треть таймлайна и был по центру
			const selectionDuration = selectedRange.end.getTime() - selectedRange.start.getTime();
			const targetViewportDuration = selectionDuration * 3; // Период занимает 1/3 от общей ширины

			const selectionCenter = selectedRange.start.getTime() + selectionDuration / 2;

			const newViewport = {
				start: new Date(selectionCenter - targetViewportDuration / 2),
				end: new Date(selectionCenter + targetViewportDuration / 2),
			};

			setViewport(newViewport);
			if (onViewportChange) {
				onViewportChange(newViewport);
			}
		}

		// Очищаем состояние перетаскивания
		setIsDragging(null);
		setDragStart(null);
		setIsPanning(false);
		setPanStart(null);
	}, [isDragging, selectedRange, onViewportChange]);

	// Добавляем и удаляем глобальные обработчики событий
	React.useEffect(() => {
		if (isDragging || isPanning) {
			document.addEventListener("mousemove", handleMouseMove);
			document.addEventListener("mouseup", handleMouseUp);

			return () => {
				document.removeEventListener("mousemove", handleMouseMove);
				document.removeEventListener("mouseup", handleMouseUp);
			};
		}
	}, [isDragging, isPanning, handleMouseMove, handleMouseUp]);

	// Убираем масштабирование колесом - только автомасштабирование от перемещения периода

	// Синхронизируем viewport с timeRange при изменении извне
	React.useEffect(() => {
		setViewport(timeRange);
	}, [timeRange]);

	// Убираем автоматическое центрирование при изменении selectedRange
	// Центрирование происходит только при отпускании после перемещения (в handleMouseUp)

	// Рассчитываем позицию выделенного диапазона
	const selectionStartX = timeToX(selectedRange.start);
	const selectionEndX = timeToX(selectedRange.end);
	const selectionWidth = selectionEndX - selectionStartX;

	// Генерируем статичные временные метки в фиксированных позициях
	const generateTimeLabels = (): { time: Date; x: number }[] => {
		if (!svgRef.current) return [];

		const width = svgRef.current.clientWidth || 800;
		const labels: { time: Date; x: number }[] = [];

		// Фиксированные позиции меток: 20%, 35%, 50%, 65%, 80% - всего 5 штук
		const positions = [0.2, 0.35, 0.5, 0.65, 0.8];

		positions.forEach((position) => {
			const x = width * position;
			// Вычисляем время для данной позиции на основе текущего viewport
			const viewportDuration = viewport.end.getTime() - viewport.start.getTime();
			const timeAtPosition = new Date(viewport.start.getTime() + viewportDuration * position);

			labels.push({
				time: timeAtPosition,
				x: x,
			});
		});

		return labels;
	};

	const timeLabels = generateTimeLabels();

	return (
		<div className={`w-full ${className}`}>
			{/* Легенда с типами метрик */}
			<div className="mb-2 flex flex-wrap gap-4 text-xs">
				{Object.entries(METRIC_CONFIGS)
					.filter(([type]) => visibleMetrics.some((metric) => metric.metricType === type))
					.map(([type, config]) => (
						<div key={type} className="flex items-center gap-1">
							<div className="w-3 h-3 rounded-full" style={{ backgroundColor: config.color }} />
							<span className="text-muted-foreground">{config.label}</span>
						</div>
					))}
			</div>

			{/* SVG таймлайн */}
			<svg
				ref={svgRef}
				width="100%"
				height={height}
				className={`border rounded ${
					isPanning ? "cursor-grabbing" : isDragging ? "cursor-grabbing" : "cursor-grab"
				}`}
				onMouseDown={handleMouseDown}
			>
				{/* Фон */}
				<rect width="100%" height="100%" fill="transparent" />

				{/* Визуальные насечки по центру */}
				{(() => {
					if (!svgRef.current) return null;
					const tickInterval = 40; // Интервал между насечками в пикселях
					const width = svgRef.current.clientWidth || 800;
					const ticks = [];
					const tickHeight = 8; // Небольшая высота насечек
					const centerY = (height - 20) / 2; // Центр таймлайна

					for (let x = 0; x <= width; x += tickInterval) {
						ticks.push(
							<line
								key={`tick-${x}`}
								x1={x}
								y1={centerY - tickHeight / 2}
								x2={x}
								y2={centerY + tickHeight / 2}
								stroke="#9CA3AF"
								strokeWidth={0.8}
								opacity={0.4}
							/>,
						);
					}
					return ticks;
				})()}

				{/* Выделенный диапазон */}
				{selectionWidth > 2 && (
					<g>
						{/* Основной прямоугольник периода */}
						<rect
							x={selectionStartX}
							y={0}
							width={selectionWidth}
							height={height}
							fill="rgba(59, 130, 246, 0.2)"
							stroke="rgba(59, 130, 246, 0.5)"
							strokeWidth={2}
							rx={4}
						/>

						{/* Левая граница (перетаскиваемая) */}
						<rect
							x={selectionStartX - 1}
							y={0}
							width={5}
							height={height}
							fill="var(--color-blue-400)"
							className="cursor-ew-resize"
							rx={2}
						>
							<title>Перетащите для изменения начала периода</title>
						</rect>

						{/* Правая граница (перетаскиваемая) */}
						<rect
							x={selectionEndX - 4}
							y={0}
							width={5}
							height={height}
							fill="var(--color-blue-400)"
							className="cursor-ew-resize"
							rx={2}
						>
							<title>Перетащите для изменения конца периода</title>
						</rect>

						{/* Центральная область для перемещения всего периода */}
						<rect
							x={selectionStartX + 4}
							y={0}
							width={selectionWidth - 8}
							height={height - 20}
							fill="transparent"
							className="cursor-move"
						>
							<title>Перетащите для перемещения периода</title>
						</rect>
					</g>
				)}

				{/* Временные метки по оси X - статичные позиции */}
				{timeLabels.map((label, index) => {
					const centerY = (height - 20) / 2; // Центр таймлайна
					return (
						<g key={index}>
							<line
								x1={label.x}
								y1={height - 15}
								x2={label.x}
								y2={height - 8}
								stroke="#9CA3AF"
								strokeWidth={1}
							/>
							<text
								x={label.x}
								y={centerY + 15} // Размещаем текст по центру таймлайна, но ниже полосочек
								textAnchor="middle"
								className="text-xs fill-muted-foreground"
								fontSize="9"
								style={{ fontWeight: 500, userSelect: "none", pointerEvents: "none" }}
							>
								{formatTimelineLabel(label.time)}
							</text>
						</g>
					);
				})}

				{/* Метрики как точки */}
				{visibleMetrics.map((metric, index) => {
					const x = timeToX(metric.time);
					const config = METRIC_CONFIGS[metric.metricType];

					// Группируем метрики, если они слишком близко друг к другу
					const nearbyMetrics = visibleMetrics.filter((m) => {
						const otherX = timeToX(m.time);
						return Math.abs(x - otherX) < 15 && m !== metric;
					});

					// Размещаем метрики по типам в разных "дорожках"
					const typeIndex = Object.keys(METRIC_CONFIGS).indexOf(metric.metricType);
					const baseY = 30 + typeIndex * 7; // Разные дорожки для разных типов

					// Дополнительное смещение для близких меток одного типа
					const sameTypeNearby = nearbyMetrics.filter((m) => m.metricType === metric.metricType);
					const offsetIndex = sameTypeNearby.findIndex((m) => m === metric);
					const y = baseY + offsetIndex * 6;

					return (
						<g key={`${metric.metricType}-${index}`}>
							<circle
								cx={x}
								cy={y}
								r={3} // Увеличиваем размер точек
								fill={config.color}
								className="cursor-pointer hover:opacity-80 transition-opacity"
							>
								<title>
									{config.label}: {formatTimelineLabel(metric.time)}
									{metric.originalTimes.length > 1 && ` (${metric.originalTimes.length} записей)`}
								</title>
							</circle>

							{/*/!* Дополнительный индикатор для групп - маленькая точка *!/*/}
							{/*{metric.originalTimes.length > 1 && (*/}
							{/*	<circle*/}
							{/*		cx={x + 3}*/}
							{/*		cy={y - 3}*/}
							{/*		r={2}*/}
							{/*		fill="white"*/}
							{/*		stroke={config.color}*/}
							{/*		strokeWidth={1}*/}
							{/*		className="pointer-events-none"*/}
							{/*	>*/}
							{/*		<title>{metric.originalTimes.length} записей</title>*/}
							{/*	</circle>*/}
							{/*)}*/}
						</g>
					);
				})}
			</svg>
		</div>
	);
}
