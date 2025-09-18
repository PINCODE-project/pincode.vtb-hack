import React from "react";

import { block } from "../../../../../../../../../../../../Downloads/date-components-main/src/utils/cn.ts";
import { SECOND } from "../../../../../../../../../../../../Downloads/date-components-main/src/components/utils/constants.ts";
import { useViewportDimensions, useViewportInterval } from "../Ruler/Ruler.tsx";

const MIN_UPDATE_DISTANCE = 4; // in px

import "./NowLine.scss";

const b = block("timeline-now-line");

export function NowLine() {
	const viewport = useViewportDimensions();
	const { start: startDate, end: endDate } = useViewportInterval();
	const [, rerender] = React.useState({});

	const nowTime = Date.now();
	const needUpdateNow = endDate.valueOf() > nowTime;
	const interval = Math.max(
		SECOND,
		(MIN_UPDATE_DISTANCE * endDate.diff(startDate)) / viewport.width,
		startDate.valueOf() - nowTime,
	);

	React.useEffect(() => {
		let timer: number | null = null;
		if (needUpdateNow) {
			timer = window.setInterval(() => {
				rerender({});
			}, interval);
		}
		return () => {
			if (timer) {
				window.clearInterval(timer);
				timer = null;
			}
		};
	}, [needUpdateNow, interval]);

	if (nowTime < startDate.valueOf() || endDate.valueOf() < nowTime) {
		return null;
	}

	const nowX = ((nowTime - startDate.valueOf()) / (endDate.valueOf() - startDate.valueOf())) * viewport.width;

	return <path d={`M${nowX},0l0,${viewport.height}`} className={b()} />;
}
