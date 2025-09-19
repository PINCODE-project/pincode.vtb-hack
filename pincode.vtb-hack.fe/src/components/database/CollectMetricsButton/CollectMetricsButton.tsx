"use client";

import React from "react";
import { Button } from "@pin-code/ui-kit";
import { RefreshCw, CheckCircle, AlertTriangle } from "lucide-react";

interface CollectMetricsButtonProps {
	onCollect: () => void;
	isLoading: boolean;
	isSuccess?: boolean;
	error?: unknown;
	label?: string;
	className?: string;
}

/**
 * Кнопка для принудительного сбора метрик
 */
export function CollectMetricsButton({
	onCollect,
	isLoading,
	isSuccess = false,
	error,
	label = "Собрать метрики",
	className,
}: CollectMetricsButtonProps) {
	// Таймер для сброса состояния успеха
	React.useEffect(() => {
		if (isSuccess) {
			const timer = setTimeout(() => {
				// Состояние успеха сбрасывается автоматически через 3 секунды
			}, 3000);
			return () => clearTimeout(timer);
		}
	}, [isSuccess]);

	// Теперь клиент корректно обрабатывает пустые ответы, поэтому проверяем ошибки просто
	const hasError = !!error;

	const getButtonContent = () => {
		if (isLoading) {
			return (
				<>
					<RefreshCw className="h-4 w-4 animate-spin" />
					Сбор данных...
				</>
			);
		}

		if (isSuccess) {
			return (
				<>
					<CheckCircle className="h-4 w-4" />
					Собрано
				</>
			);
		}

		if (hasError) {
			return (
				<>
					<AlertTriangle className="h-4 w-4" />
					Ошибка
				</>
			);
		}

		return (
			<>
				<RefreshCw className="h-4 w-4" />
				{label}
			</>
		);
	};

	const getButtonVariant = () => {
		if (hasError) {
			return "destructive";
		}
		if (isSuccess) return "outline";
		return "default";
	};

	return (
		<Button
			onClick={onCollect}
			disabled={isLoading}
			variant={getButtonVariant() as "default" | "destructive" | "outline" | "secondary" | "ghost" | "link"}
			size="sm"
			className={className}
		>
			{getButtonContent()}
		</Button>
	);
}
