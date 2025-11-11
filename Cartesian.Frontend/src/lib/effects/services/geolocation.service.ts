import { Context, Effect, Layer, Schema } from "effect";

export class GeolocationError extends Schema.TaggedError<GeolocationError>()("GeolocationError", {
	code: Schema.Number,
	message: Schema.String,
}) {}

export interface GeolocationPosition {
	latitude: number;
	longitude: number;
	accuracy: number;
	altitude: number | null;
	altitudeAccuracy: number | null;
	heading: number | null;
	speed: number | null;
	timestamp: number;
}

export interface GeolocationOptions {
	enableHighAccuracy?: boolean;
	timeout?: number;
	maximumAge?: number;
}

export class GeolocationService extends Context.Tag("GeolocationService")<
	GeolocationService,
	{
		getCurrentPosition: (
			options?: GeolocationOptions,
		) => Effect.Effect<GeolocationPosition, GeolocationError>;
		watchPosition: (
			options?: GeolocationOptions,
		) => Effect.Effect<GeolocationPosition, GeolocationError>;
	}
>() {}

const makeGeolocationService = Effect.sync(() => {
	const getCurrentPosition = (
		options?: GeolocationOptions,
	): Effect.Effect<GeolocationPosition, GeolocationError> => {
		return Effect.tryPromise({
			try: () =>
				new Promise<GeolocationPosition>((resolve, reject) => {
					if (!navigator.geolocation) {
						reject(new Error("Geolocation is not supported by this browser"));
						return;
					}

					navigator.geolocation.getCurrentPosition(
						(position) => {
							console.log("Obtained position:", position);
							resolve({
								latitude: position.coords.latitude,
								longitude: position.coords.longitude,
								accuracy: position.coords.accuracy,
								altitude: position.coords.altitude,
								altitudeAccuracy: position.coords.altitudeAccuracy,
								heading: position.coords.heading,
								speed: position.coords.speed,
								timestamp: position.timestamp,
							});
						},
						(error) => {
							reject(error);
						},
						options,
					);
				}),
			catch: (error) => {
				const geoError = error as GeolocationPositionError;
				return new GeolocationError({
					code: geoError.code,
					message: geoError.message,
				});
			},
		});
	};

	const watchPosition = (
		options?: GeolocationOptions,
	): Effect.Effect<GeolocationPosition, GeolocationError> => {
		return Effect.tryPromise({
			try: () =>
				new Promise<GeolocationPosition>((resolve, reject) => {
					if (!navigator.geolocation) {
						reject(new Error("Geolocation is not supported by this browser"));
						return;
					}

					navigator.geolocation.watchPosition(
						(position) => {
							resolve({
								latitude: position.coords.latitude,
								longitude: position.coords.longitude,
								accuracy: position.coords.accuracy,
								altitude: position.coords.altitude,
								altitudeAccuracy: position.coords.altitudeAccuracy,
								heading: position.coords.heading,
								speed: position.coords.speed,
								timestamp: position.timestamp,
							});
						},
						(error) => {
							reject(error);
						},
						options,
					);
				}),
			catch: (error) => {
				const geoError = error as GeolocationPositionError;
				return new GeolocationError({
					code: geoError.code,
					message: geoError.message,
				});
			},
		});
	};

	return {
		getCurrentPosition,
		watchPosition,
	} as const;
});

export const GeolocationServiceLive = Layer.effect(GeolocationService, makeGeolocationService);
