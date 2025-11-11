import { Effect } from "effect";
import { fetchIpGeo } from "$lib/api/queries/ip-geo.query";
import type { PageServerLoad } from "./$types";

declare const __PUBLIC_IP__: string | undefined;

export const load: PageServerLoad = async ({ getClientAddress }) => {
	const isDev = typeof __PUBLIC_IP__ !== "undefined";
	const clientIp = isDev ? __PUBLIC_IP__ : getClientAddress();

	const geoData = await Effect.runPromise(
		fetchIpGeo(clientIp).pipe(Effect.catchAll(() => Effect.succeed(null))),
	);

	return {
		ipGeo: geoData,
	};
};
