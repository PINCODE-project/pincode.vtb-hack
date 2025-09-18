import baseClient, { type ResponseConfig, type ResponseErrorConfig } from "@kubb/plugin-client/clients/fetch";

export type { RequestConfig, ResponseConfig, ResponseErrorConfig } from "@kubb/plugin-client/clients/fetch";

interface ErrorResponse {
	detail: string | { code: string; reason: string };
}

type ApiErrorResponse = ResponseErrorConfig & {
	data?: ErrorResponse;
	status: number;
};

function createApiError(error: ApiErrorResponse): Error {
	const defaultMessage = `Ошибка запроса (${error.status})`;

	return new Error(defaultMessage);
}

baseClient.setConfig({
	headers: {
		"Content-Type": "application/json",
	},
});

const client = (async (...args: Parameters<typeof baseClient>): Promise<ResponseConfig> => {
	try {
		const res = await baseClient(...args);

		if (res.status < 200 || res.status >= 300) {
			throw createApiError(res as ApiErrorResponse);
		}

		return res;
	} catch (error: unknown) {
		// Проверяем, если это ошибка парсинга JSON для пустого ответа с успешным статусом
		if (
			error instanceof Error &&
			(error.message?.includes("JSON.parse: unexpected end of data") ||
				error.message?.includes("Unexpected end of JSON input") ||
				error.message?.includes("Unexpected token") ||
				error.name === "SyntaxError")
		) {
			// Пытаемся сделать запрос снова, но с обработкой пустого ответа
			const [config] = args;

			if (!config.url) {
				throw error; // Если нет URL, пробрасываем исходную ошибку
			}

			const response = await fetch(config.url, {
				method: config.method,
				headers: config.headers,
				body: config.data ? JSON.stringify(config.data) : undefined,
			});

			// Если статус успешный, возвращаем пустой успешный ответ
			if (response.status >= 200 && response.status < 300) {
				return {
					data: null,
					status: response.status,
					statusText: response.statusText,
					headers: response.headers,
				} as ResponseConfig;
			}

			// Если статус неуспешный, пробрасываем ошибку
			throw createApiError({
				status: response.status,
				statusText: response.statusText,
			} as ApiErrorResponse);
		}

		// Пробрасываем другие ошибки
		throw error;
	}
}) as typeof baseClient;

client.setConfig = baseClient.setConfig;
client.getConfig = baseClient.getConfig;

export default client;
