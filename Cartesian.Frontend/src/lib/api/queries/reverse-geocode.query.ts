import { Effect, Schema } from "effect";
import { createQuery } from "@tanstack/svelte-query";
import type { CreateQueryOptions, CreateQueryResult, QueryClient } from "@tanstack/svelte-query";
import { env } from "$env/dynamic/public";

const getMapboxToken = () => env.PUBLIC_MAPBOX_ACCESS_TOKEN || "";

const FeaturePropertiesSchema = Schema.Struct({
	name: Schema.optional(Schema.String),
	place_formatted: Schema.optional(Schema.String),
});

const FeatureSchema = Schema.Struct({
	properties: FeaturePropertiesSchema,
});

export const ReverseGeocodeSchema = Schema.Struct({
	features: Schema.Array(FeatureSchema),
});

export interface ReverseGeocode extends Schema.Schema.Type<typeof ReverseGeocodeSchema> {}

export class ReverseGeocodeError extends Schema.TaggedError<ReverseGeocodeError>()(
	"ReverseGeocodeError",
	{
		message: Schema.String,
	},
) {}

export const fetchReverseGeocode = (longitude: number, latitude: number) =>
	Effect.gen(function* () {
		const url = `https://api.mapbox.com/search/geocode/v6/reverse?longitude=${longitude}&latitude=${latitude}&access_token=${getMapboxToken()}`;

		const response = yield* Effect.tryPromise({
			try: () => fetch(url),
			catch: (error) =>
				new ReverseGeocodeError({ message: `Failed to fetch: ${String(error)}` }),
		});

		if (!response.ok) {
			return yield* Effect.fail(
				new ReverseGeocodeError({
					message: `HTTP ${response.status}: ${response.statusText}`,
				}),
			);
		}

		const data = yield* Effect.tryPromise({
			try: () => response.json(),
			catch: (error) =>
				new ReverseGeocodeError({ message: `Failed to parse JSON: ${String(error)}` }),
		});

		return yield* Schema.decodeUnknown(ReverseGeocodeSchema)(data).pipe(
			Effect.catchAll(() =>
				Effect.fail(new ReverseGeocodeError({ message: "Invalid geocoding data" })),
			),
		);
	});

export function createReverseGeocodeQuery<TData = ReverseGeocode, TError = ReverseGeocodeError>(
	longitude: number,
	latitude: number,
	options?: { query?: Partial<CreateQueryOptions<ReverseGeocode, TError, TData>> },
	queryClient?: QueryClient,
): CreateQueryResult<TData, TError> {
	const queryOptions = {
		queryKey: ["reverse-geocode", longitude, latitude] as const,
		queryFn: () => Effect.runPromise(fetchReverseGeocode(longitude, latitude)),
		staleTime: 1000 * 60 * 60,
		...options?.query,
	};

	return createQuery(() => ({ ...queryOptions, queryClient })) as CreateQueryResult<
		TData,
		TError
	>;
}
