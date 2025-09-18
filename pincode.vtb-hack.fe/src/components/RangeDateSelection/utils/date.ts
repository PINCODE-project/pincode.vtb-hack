import { dateTimeUtc } from "@gravity-ui/date-utils";
import type { DateTime } from "@gravity-ui/date-utils";

import { SECOND } from "../../../../../../../../../../../Downloads/date-components-main/src/components/utils/constants.ts";

export function alignDateTime(date: DateTime, align = SECOND) {
	return dateTimeUtc({
		input: Math.round(date.utc(true).valueOf() / align) * align,
	}).timeZone(date.timeZone(), true);
}
