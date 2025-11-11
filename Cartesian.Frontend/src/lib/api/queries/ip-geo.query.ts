import { Effect, Schema } from "effect";
import { createQuery } from "@tanstack/svelte-query";
import type { CreateQueryOptions, CreateQueryResult, QueryClient } from "@tanstack/svelte-query";

export const IpGeoSchema = Schema.Struct({
	status: Schema.String,
	countryCode: Schema.String,
	lat: Schema.Number,
	lon: Schema.Number,
});

export type IpGeo = typeof IpGeoSchema.Type;

export class IpGeoError extends Schema.TaggedError<IpGeoError>()("IpGeoError", {
	message: Schema.String,
}) {}

const fetchIpGeo = Effect.gen(function* () {
	const response = yield* Effect.tryPromise({
		try: () => fetch("http://ip-api.com/json/?fields=49346"),
		catch: (error) => new IpGeoError({ message: String(error) }),
	});

	if (!response.ok) {
		return yield* Effect.fail(
			new IpGeoError({ message: `HTTP ${response.status}: ${response.statusText}` }),
		);
	}

	const data = yield* Effect.tryPromise({
		try: () => response.json(),
		catch: (error) => new IpGeoError({ message: String(error) }),
	});

	return yield* Schema.decodeUnknown(IpGeoSchema)(data);
});

export function createIpGeoQuery<TData = IpGeo, TError = IpGeoError>(
	options?: { query?: Partial<CreateQueryOptions<IpGeo, TError, TData>> },
	queryClient?: QueryClient,
): CreateQueryResult<TData, TError> {
	const queryOptions = {
		queryKey: ["ip-geo"] as const,
		queryFn: () => Effect.runPromise(fetchIpGeo),
		staleTime: 1000 * 60 * 5,
		...options?.query,
	};

	return createQuery(() => ({ ...queryOptions, queryClient })) as CreateQueryResult<
		TData,
		TError
	>;
}
