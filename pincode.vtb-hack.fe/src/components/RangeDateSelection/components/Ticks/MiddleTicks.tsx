import { useViewportDimensions } from "../Ruler/Ruler.tsx";

import { Ticks, makeMiddleTicksGeometry } from "./Ticks.tsx";

export function MiddleTicks({ timeZone }: { timeZone?: string }) {
	const { height } = useViewportDimensions();
	return (
		<Ticks
			minTickWidth={8}
			maxTickWidth={20}
			theme="dim"
			geometry={makeMiddleTicksGeometry({ tickHeight: 4, viewportHeight: height })}
			timeZone={timeZone}
		/>
	);
}
