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
	const res = await baseClient(...args);

	if (res.status < 200 || res.status >= 300) {
		throw createApiError(res as ApiErrorResponse);
	}

	return res;
}) as typeof baseClient;

client.setConfig = baseClient.setConfig;
client.getConfig = baseClient.getConfig;

export default client;
