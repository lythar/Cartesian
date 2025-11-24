import { createGetEventApiGeojson, type GetEventApiGeojsonParams } from "../cartesian-client";

export const createEventsGeojsonQuery = (
	params?: GetEventApiGeojsonParams,
	options?: {
		query?: {
			enabled?: boolean;
			refetchInterval?: number;
			staleTime?: number;
		};
	},
) => {
	return createGetEventApiGeojson(params, {
		query: {
			refetchInterval: 10 * 60 * 1000,
			staleTime: 5 * 60 * 1000,
			...options?.query,
		},
	});
};
