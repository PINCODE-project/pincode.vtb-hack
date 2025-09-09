import { Badge } from "@pin-code/ui-kit";
import { CheckCircle, AlertCircle, XCircle, Info } from "lucide-react";

/**
 * Утилиты для UI элементов в компонентах анализа базы данных
 */

/**
 * Получить бейдж для уровня критичности
 */
export const getSeverityBadge = (severity: string | null | undefined) => {
	switch (severity?.toLowerCase()) {
		case "critical":
		case "high":
			return <Badge variant="destructive">{severity}</Badge>;
		case "warning":
		case "medium":
			return <Badge variant="secondary">{severity}</Badge>;
		case "info":
		case "low":
			return <Badge variant="outline">{severity}</Badge>;
		default:
			return <Badge variant="outline">Unknown</Badge>;
	}
};

/**
 * Получить иконку статуса
 */
export const getStatusIcon = (status: string | null | undefined) => {
	switch (status?.toLowerCase()) {
		case "healthy":
			return <CheckCircle className="h-5 w-5 text-green-500" />;
		case "warning":
			return <AlertCircle className="h-5 w-5 text-yellow-500" />;
		case "critical":
			return <XCircle className="h-5 w-5 text-red-500" />;
		default:
			return <Info className="h-5 w-5 text-blue-500" />;
	}
};
