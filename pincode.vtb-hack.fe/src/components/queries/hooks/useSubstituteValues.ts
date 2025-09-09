"use client";

import { useMutation } from "@tanstack/react-query";
import type { QueryClient } from "@tanstack/react-query";

interface SubstituteValuesRequest {
	dbConnectionId?: string;
	sql?: string | null;
}

// Кастомная функция для вызова API, которая возвращает просто строку
async function substituteValuesToSql(data: SubstituteValuesRequest): Promise<string> {
	const response = await fetch("https://sql-analyzer.pincode-infra.ru/api/substitute-values-to-sql/substitute", {
		method: "POST",
		headers: {
			"Content-Type": "application/json",
		},
		body: JSON.stringify(data),
	});

	if (!response.ok) {
		throw new Error(`Ошибка API: ${response.status} ${response.statusText}`);
	}

	// Получаем ответ как простой текст, а не как JSON
	const textResult = await response.text();
	return textResult;
}

export function useSubstituteValues<TContext>(
	options: {
		mutation?: {
			onSuccess?: (data: string) => void;
			onError?: (error: Error) => void;
			mutationKey?: unknown[];
		} & { client?: QueryClient };
	} = {},
) {
	const { mutation = {} } = options;
	const { client: queryClient, ...mutationOptions } = mutation;

	return useMutation<string, Error, { data: SubstituteValuesRequest }, TContext>(
		{
			mutationFn: async ({ data }) => {
				return substituteValuesToSql(data);
			},
			...mutationOptions,
		},
		queryClient,
	);
}
