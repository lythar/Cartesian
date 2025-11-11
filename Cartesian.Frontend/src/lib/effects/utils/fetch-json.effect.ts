import { Effect, Schema } from "effect";

export interface FetchJsonConfig extends RequestInit {
	url: string;
	params?: Record<string, unknown>;
}

export class FetchJsonError extends Schema.TaggedError<FetchJsonError>()("FetchJsonError", {
	status: Schema.Number,
	message: Schema.String,
}) {}

export const fetchJson = <T>(config: FetchJsonConfig): Effect.Effect<T, FetchJsonError> => {
	return Effect.gen(function* () {
		const { url, params, headers, ...fetchConfig } = config;

		let finalUrl = url;

		if (params) {
			const searchParams = new URLSearchParams();
			Object.entries(params).forEach(([key, value]) => {
				if (value !== undefined && value !== null) {
					searchParams.append(key, String(value));
				}
			});
			const queryString = searchParams.toString();
			if (queryString) {
				finalUrl += `?${queryString}`;
			}
		}

		const response = yield* Effect.tryPromise({
			try: () =>
				fetch(finalUrl, {
					...fetchConfig,
					headers: {
						"Content-Type": "application/json",
						...headers,
					},
				}),
			catch: (error) => new FetchJsonError({ status: 0, message: String(error) }),
		});

		if (!response.ok) {
			return yield* Effect.fail(
				new FetchJsonError({ status: response.status, message: response.statusText }),
			);
		}

		return (yield* Effect.tryPromise({
			try: () => response.json(),
			catch: (error) =>
				new FetchJsonError({ status: response.status, message: String(error) }),
		})) as T;
	});
};
