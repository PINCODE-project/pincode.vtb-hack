"use client";

import React, { useState } from "react";
import { Alert, AlertDescription } from "@pin-code/ui-kit";
import { useGetApiQueriesFind } from "@/generated/hooks/QueryAnalysis";
import { Clock } from "lucide-react";
import { useGetApiDbConnectionsFind } from "@generated";
import { QueriesHistory } from "@/components/queries";

export default function QueriesPage() {
	const { data: allQueries, isLoading: isLoadingQueries, error: queriesError } = useGetApiQueriesFind();
	const { data: databases } = useGetApiDbConnectionsFind();
	const [isAnimating, setIsAnimating] = useState(false);

	const handleAlertClick = () => {
		setIsAnimating(true);
		setTimeout(() => setIsAnimating(false), 600);
	};

	return (
		<div className="p-6 space-y-6 flex flex-col w-full">
			<div className="mb-6">
				<h1 className="text-3xl font-bold">SQL Запросы</h1>
				<p className="text-muted-foreground mt-2">
					Создавайте и анализируйте SQL запросы для оптимизации производительности
				</p>
			</div>

			<div className="flex flex-col items-center gap-6 max-w-4xl mx-auto">
				<div className="w-fit">
					<Alert
						className="mb-6 cursor-pointer transition-colors hover:bg-accent/50"
						onClick={handleAlertClick}
					>
						<AlertDescription>
							<div className="flex items-center gap-2">
								<span
									className={`text-2xl inline-block transition-transform duration-300 ${
										isAnimating ? "animate-finger-point" : ""
									}`}
								>
									👈
								</span>
								<span className="text-lg font-medium">Чтобы проанализировать новый запрос выберите БД слева</span>
							</div>
						</AlertDescription>
					</Alert>
				</div>

				<div className="w-full">
					<div className="flex items-center gap-2 mb-6">
						<Clock className="h-5 w-5" />
						<h2 className="text-xl font-semibold">История запросов</h2>
					</div>

					<QueriesHistory
						queries={allQueries}
						databases={databases}
						isLoading={isLoadingQueries}
						error={queriesError}
						showDatabaseNames={true}
						emptyStateMessage="Нет сохраненных запросов"
						gridCols="grid-cols-1 md:grid-cols-2"
					/>
				</div>
			</div>
		</div>
	);
}
