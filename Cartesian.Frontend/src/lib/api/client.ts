import { Effect, Schema } from "effect";

const baseUrl =
	import.meta.env.services_url ??
	import.meta.env.services__https__0 ??
	import.meta.env.services__http__0 ??
	"http://localhost:5164";

export interface FetchConfig extends RequestInit {
	url: string;
	params?: Record<string, unknown>;
	data?: unknown;
}

export class FetchError extends Schema.TaggedError<FetchError>()("FetchError", {
	status: Schema.Number,
	message: Schema.String,
}) {}

export const customInstance = async <T>(config: FetchConfig): Promise<T> => {
	const program = Effect.gen(function* () {
		const { url, params, data, headers, ...fetchConfig } = config;

		let finalUrl = `${baseUrl}${url}`;

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
					body: data ? JSON.stringify(data) : undefined,
				}),
			catch: (error) => new FetchError({ status: 0, message: String(error) }),
		});

		if (!response.ok) {
			return yield* Effect.fail(
				new FetchError({ status: response.status, message: response.statusText }),
			);
		}

		const contentType = response.headers.get("content-type");
		if (contentType?.includes("application/json")) {
			return (yield* Effect.tryPromise({
				try: () => response.json(),
				catch: (error) =>
					new FetchError({ status: response.status, message: String(error) }),
			})) as T;
		}

		return (yield* Effect.tryPromise({
			try: () => response.text(),
			catch: (error) => new FetchError({ status: response.status, message: String(error) }),
		})) as T;
	});

	return Effect.runPromise(program);
};

export type ErrorType<Error> = Error;
