import { addComponentKeysets } from "@gravity-ui/uikit/i18n";

import { NAMESPACE } from "../../../../../../../../../../../Downloads/date-components-main/src/utils/cn.ts";

import en from "./en.json";
import ru from "./ru.json";

export const i18n = addComponentKeysets({ en, ru }, `${NAMESPACE}range-date-selection`);
