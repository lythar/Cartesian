import { Effect } from "effect";
import { fetchIpGeo } from "$lib/api/queries/ip-geo.query";
import type { PageServerLoad } from "./$types";
import { getEventApiEventId, getEventApiEventIdImages } from "$lib/api/cartesian-client";

declare const __PUBLIC_IP__: string | undefined;

export const load: PageServerLoad = async ({ getClientAddress, url }) => {
	const isDev = typeof __PUBLIC_IP__ !== "undefined";
	const clientIp = isDev ? __PUBLIC_IP__ : getClientAddress();

	const geoData = await Effect.runPromise(
		fetchIpGeo(clientIp).pipe(Effect.catchAll(() => Effect.succeed(null))),
	);

	let metaEvent = null;
	const eventId = url.searchParams.get("event");

	if (eventId) {
		try {
			const [event, images] = await Promise.all([
				getEventApiEventId(eventId),
				getEventApiEventIdImages(eventId).catch(() => []),
			]);

			metaEvent = {
				name: event.name,
				description: event.description,
				image: images.length > 0 ? images[0] : null,
				author: event.author,
			};
		} catch (e) {
			console.error("Failed to fetch event for meta tags", e);
		}
	}

	return {
		ipGeo: geoData,
		metaEvent,
	};
};
