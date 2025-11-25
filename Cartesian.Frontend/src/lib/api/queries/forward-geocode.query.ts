import { Effect, Schema } from "effect";
import { createQuery } from "@tanstack/svelte-query";
import type { CreateQueryOptions, CreateQueryResult, QueryClient } from "@tanstack/svelte-query";
import { env } from "$env/dynamic/public";

const getMapboxToken = () => env.PUBLIC_MAPBOX_ACCESS_TOKEN || "";

const FeaturePropertiesSchema = Schema.Struct({
	name: Schema.optional(Schema.String),
	place_formatted: Schema.optional(Schema.String),
	full_address: Schema.optional(Schema.String),
});

const GeometrySchema = Schema.Struct({
	type: Schema.Literal("Point"),
	coordinates: Schema.Tuple(Schema.Number, Schema.Number),
});

const FeatureSchema = Schema.Struct({
	id: Schema.String,
	properties: FeaturePropertiesSchema,
	geometry: GeometrySchema,
});

export const ForwardGeocodeSchema = Schema.Struct({
	features: Schema.Array(FeatureSchema),
});

export interface ForwardGeocode extends Schema.Schema.Type<typeof ForwardGeocodeSchema> {}

export class ForwardGeocodeError extends Schema.TaggedError<ForwardGeocodeError>()(
	"ForwardGeocodeError",
	{
		message: Schema.String,
	},
) {}

export const fetchForwardGeocode = (query: string) =>
	Effect.gen(function* () {
		if (!query || query.length < 3) return { features: [] };

		const url = `https://api.mapbox.com/search/geocode/v6/forward?q=${encodeURIComponent(query)}&access_token=${getMapboxToken()}`;

		const response = yield* Effect.tryPromise({
			try: () => fetch(url),
			catch: (error) =>
				new ForwardGeocodeError({ message: `Failed to fetch: ${String(error)}` }),
		});

		if (!response.ok) {
			return yield* Effect.fail(
				new ForwardGeocodeError({
					message: `HTTP ${response.status}: ${response.statusText}`,
				}),
			);
		}

		const data = yield* Effect.tryPromise({
			try: () => response.json(),
			catch: (error) =>
				new ForwardGeocodeError({ message: `Failed to parse JSON: ${String(error)}` }),
		});

		return yield* Schema.decodeUnknown(ForwardGeocodeSchema)(data).pipe(
			Effect.catchAll(() =>
				Effect.fail(new ForwardGeocodeError({ message: "Invalid geocoding data" })),
			),
		);
	});

export const fetchReverseGeocode = (longitude: number, latitude: number) =>
	Effect.gen(function* () {
		if (longitude === undefined || latitude === undefined) return { features: [] };

		const url = `https://api.mapbox.com/search/geocode/v6/reverse?longitude=${longitude}&latitude=${latitude}&access_token=${getMapboxToken()}`;

		const response = yield* Effect.tryPromise({
			try: () => fetch(url),
			catch: (error) =>
				new ForwardGeocodeError({ message: `Failed to fetch: ${String(error)}` }),
		});

		if (!response.ok) {
			return yield* Effect.fail(
				new ForwardGeocodeError({
					message: `HTTP ${response.status}: ${response.statusText}`,
				}),
			);
		}

		const data = yield* Effect.tryPromise({
			try: () => response.json(),
			catch: (error) =>
				new ForwardGeocodeError({ message: `Failed to parse JSON: ${String(error)}` }),
		});

		return yield* Schema.decodeUnknown(ForwardGeocodeSchema)(data).pipe(
			Effect.catchAll(() =>
				Effect.fail(new ForwardGeocodeError({ message: "Invalid geocoding data" })),
			),
		);
	});

export function createForwardGeocodeQuery<TData = ForwardGeocode, TError = ForwardGeocodeError>(
	query: () => string,
	options?: { query?: Partial<CreateQueryOptions<ForwardGeocode, TError, TData>> },
	queryClient?: QueryClient,
): CreateQueryResult<TData, TError> {
	return createQuery(() => {
		const q = query();
		return {
			queryKey: ["forward-geocode", q] as const,
			queryFn: () => Effect.runPromise(fetchForwardGeocode(q)),
			staleTime: 1000 * 60 * 60,
			enabled: !!q && q.length >= 3,
			...options?.query,
			queryClient,
		};
	}) as CreateQueryResult<TData, TError>;
}

export function createReverseGeocodeQuery<TData = ForwardGeocode, TError = ForwardGeocodeError>(
	coordinates: () => { longitude: number; latitude: number } | null | undefined,
	options?: { query?: Partial<CreateQueryOptions<ForwardGeocode, TError, TData>> },
	queryClient?: QueryClient,
): CreateQueryResult<TData, TError> {
	return createQuery(() => {
		const coords = coordinates();
		return {
			queryKey: ["reverse-geocode", coords?.longitude, coords?.latitude] as const,
			queryFn: () =>
				Effect.runPromise(fetchReverseGeocode(coords!.longitude, coords!.latitude)),
			staleTime: 1000 * 60 * 60,
			enabled: !!coords && coords.longitude !== undefined && coords.latitude !== undefined,
			...options?.query,
			queryClient,
		};
	}) as CreateQueryResult<TData, TError>;
}
