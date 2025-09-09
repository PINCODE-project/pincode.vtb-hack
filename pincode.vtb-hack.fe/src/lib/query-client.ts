import { isServer, MutationCache, QueryCache, QueryClient } from "@tanstack/react-query";

const onError = () => {
	if (!isServer) {
		// toaster.add({ theme: "danger", name: error.name, title: error.message });
	}
};

export const makeQueryClient = () =>
	new QueryClient({
		defaultOptions: {
			queries: {
				staleTime: 60 * 1000,
			},
		},
		queryCache: new QueryCache({ onError }),
		mutationCache: new MutationCache({ onError }),
	});

let browserQueryClient: QueryClient | undefined;

export function getQueryClient() {
	if (isServer) {
		return makeQueryClient();
	}
	if (!browserQueryClient) browserQueryClient = makeQueryClient();
	return browserQueryClient;
}
