import React from "react";
import { Badge } from "@pin-code/ui-kit";

/**
 * Получает стилизованный Badge для уровня серьезности
 */
export const getLevelBadge = (level?: "critical" | "high" | "medium" | "low") => {
	const variant =
		level === "critical" || level === "high" ? "destructive" : level === "medium" ? "secondary" : "default";
	return level ? (
		<Badge variant={variant} className="text-xs">
			{level}
		</Badge>
	) : null;
};
