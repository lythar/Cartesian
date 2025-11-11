import { Effect, Schema } from "effect";
import { createQuery } from "@tanstack/svelte-query";
import type { CreateQueryOptions, CreateQueryResult, QueryClient } from "@tanstack/svelte-query";
import { IpGeoError, IpGeoSchema, type IpGeo } from "$lib/effects/schemas/ip-geo.schema";
import { fetchJson, FetchJsonError } from "$lib/effects/utils/fetch-json.effect";

export const fetchIpGeo = (ip?: string) =>
	fetchJson<unknown>({
		url: `http://ip-api.com/json/${ip ?? ""}`,
		params: { fields: "49346" },
	}).pipe(
		Effect.flatMap((data) => Schema.decodeUnknown(IpGeoSchema)(data)),
		Effect.catchAll((error) => {
			const message =
				error instanceof FetchJsonError ? error.message : "Invalid geolocation data";
			return Effect.fail(new IpGeoError({ message }));
		}),
	);

export function createIpGeoQuery<TData = IpGeo, TError = IpGeoError>(
	options?: { query?: Partial<CreateQueryOptions<IpGeo, TError, TData>> },
	queryClient?: QueryClient,
): CreateQueryResult<TData, TError> {
	const queryOptions = {
		queryKey: ["ip-geo"] as const,
		queryFn: () => Effect.runPromise(fetchIpGeo()),
		staleTime: 1000 * 60 * 5,
		...options?.query,
	};

	return createQuery(() => ({ ...queryOptions, queryClient })) as CreateQueryResult<
		TData,
		TError
	>;
}
