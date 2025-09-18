import React from "react";

import type { DateTime } from "@gravity-ui/date-utils";
import ReactDOM from "react-dom";

import type {
	MoveEndEvent,
	MoveMoveEvent,
	MoveStartEvent,
} from "../../../../../../../../../../../../Downloads/date-components-main/src/hooks/useMove.ts";
import { useResizeObserver } from "../../../../../../../../../../../../Downloads/date-components-main/src/hooks/useResizeObserver.ts";
import { block } from "../../../../../../../../../../../../Downloads/date-components-main/src/utils/cn.ts";
import type { RangeValue } from "../../../../../../../../../../../../Downloads/date-components-main/src/components/types";
import { NowLine } from "../NowLine/NowLine.tsx";
import { RulerViewport } from "../RulerViewport/RulerViewport.tsx";
import { MiddleTicks } from "../Ticks/MiddleTicks.tsx";
import { SlitTicks } from "../Ticks/SlitTicks.tsx";
import { UnavailableTicks } from "../Ticks/UnavialableTicks.tsx";
import { makeUnavailableTicksGeometry } from "../Ticks/utils.ts";

const b = block("ruler");

interface RulerProps {
	className?: string;
	children?: React.ReactNode;
	start: DateTime;
	end: DateTime;
	minValue?: DateTime;
	maxValue?: DateTime;
	onMoveStart?: (event: MoveStartEvent) => void;
	onMove?: (delta: number, event: MoveMoveEvent) => void;
	onMoveEnd?: (event: MoveEndEvent) => void;
	displayNow?: boolean;
	formatTime?: (time: DateTime) => string;
	timeZone?: string;
	dragDisabled?: boolean;
	renderAdditionalRulerContent?: (props: {
		interval: ViewportInterval;
		dimensions: ViewportDimensions;
	}) => React.ReactNode;
}

export interface ViewportDimensions {
	width: number;
	height: number;
}

export type ViewportInterval = RangeValue<DateTime>;

const viewportDimensionsContext = React.createContext<ViewportDimensions | null>(null);
const viewportIntervalContext = React.createContext<ViewportInterval | null>(null);

export function DateTimeRuler({
	className,
	children,
	start,
	end,
	minValue,
	maxValue,
	onMove,
	onMoveStart,
	onMoveEnd,
	displayNow,
	formatTime,
	timeZone,
	dragDisabled,
	renderAdditionalRulerContent,
}: RulerProps) {
	const [container, setContainer] = React.useState<HTMLDivElement | null>(null);
	const viewportInterval = React.useMemo(() => ({ start, end }), [start, end]);
	const [viewportDimensions, setViewportDimensions] = React.useState<ViewportDimensions>({
		width: 0,
		height: 0,
	});
	const onResize = React.useCallback(() => {
		if (!container) {
			return;
		}
		const { width, height } = container.getBoundingClientRect();
		setViewportDimensions({ width, height });
	}, [container]);

	const containerRef = React.useMemo(() => ({ current: container }), [container]);

	useResizeObserver({ ref: containerRef, onResize });

	const hasConstrains = Boolean(minValue || maxValue);

	return (
		<div className={b(null, className)}>
			<viewportIntervalContext.Provider value={viewportInterval}>
				<viewportDimensionsContext.Provider value={viewportDimensions}>
					<RulerViewport
						ref={setContainer}
						onMove={onMove}
						onMoveStart={onMoveStart}
						onMoveEnd={onMoveEnd}
						dragDisabled={dragDisabled}
					>
						<MiddleTicks timeZone={timeZone} />
						<SlitTicks formatTime={formatTime} timeZone={timeZone} />
						{hasConstrains ? (
							<UnavailableTicks
								theme="dim"
								minValue={minValue}
								maxValue={maxValue}
								geometry={makeUnavailableTicksGeometry({
									tickHeight: 10,
									viewportHeight: viewportDimensions.height,
								})}
								tickWidth={viewportDimensions.height + 10}
							/>
						) : null}
						{displayNow ? <NowLine /> : null}
						{renderAdditionalRulerContent?.({
							interval: viewportInterval,
							dimensions: viewportDimensions,
						})}
					</RulerViewport>
					{container ? ReactDOM.createPortal(children, container) : null}
				</viewportDimensionsContext.Provider>
			</viewportIntervalContext.Provider>
		</div>
	);
}

export function useViewportDimensions() {
	const viewport = React.useContext(viewportDimensionsContext);
	if (!viewport) {
		throw new Error("useViewportDimensions must be used within a RulerViewport");
	}
	return viewport;
}

export function useViewportInterval() {
	const viewport = React.useContext(viewportIntervalContext);
	if (!viewport) {
		throw new Error("useViewportInterval must be used within a RulerViewport");
	}
	return viewport;
}
