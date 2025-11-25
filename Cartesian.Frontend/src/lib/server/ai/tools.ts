import { tool } from "ai";
import { z } from "zod";
import { env as publicEnv } from "$env/dynamic/public";

const getMapboxToken = () => publicEnv.PUBLIC_MAPBOX_ACCESS_TOKEN || "";
const getServicesUrl = () => publicEnv.PUBLIC_SERVICES_URL || "http://localhost:5164";

export const mapboxSearchTool = tool({
	description:
		"Search for physical locations, places, businesses, addresses, or points of interest using Mapbox. Use this for queries like 'pizza near me', 'museums in Paris', 'where is Central Park', etc.",
	inputSchema: z.object({
		query: z.string().describe("The search query for the location"),
		proximity: z
			.object({
				longitude: z.number(),
				latitude: z.number(),
			})
			.optional()
			.describe("Optional coordinates to bias results towards a specific location"),
	}),
	execute: async ({ query, proximity }) => {
		let url = `https://api.mapbox.com/search/geocode/v6/forward?q=${encodeURIComponent(query)}&access_token=${getMapboxToken()}&limit=5`;

		if (proximity) {
			url += `&proximity=${proximity.longitude},${proximity.latitude}`;
		}

		const response = await fetch(url);
		if (!response.ok) {
			return { error: `Failed to search locations: ${response.statusText}` };
		}

		const data = await response.json();
		const features = data.features || [];

		return {
			results: features.map((f: any) => ({
				name: f.properties?.name || "Unknown",
				fullAddress: f.properties?.full_address || f.properties?.place_formatted || "",
				coordinates: f.geometry?.coordinates || [],
				type: f.properties?.feature_type || "place",
			})),
		};
	},
});

export const eventSearchTool = tool({
	description:
		"Search for events, activities, concerts, festivals, shows, or time-bound happenings. Use this for queries like 'concerts tonight', 'festivals this weekend', 'what events are happening', etc.",
	inputSchema: z.object({
		query: z.string().describe("The search query for events"),
		limit: z.number().optional().default(10).describe("Maximum number of results to return"),
	}),
	execute: async ({ query, limit }, { abortSignal }) => {
		try {
			const baseUrl = getServicesUrl();
			const url = `${baseUrl}/search/api/events?query=${encodeURIComponent(query)}&limit=${limit}`;

			const response = await fetch(url, { signal: abortSignal });
			if (!response.ok) {
				const errorBody = await response.text().catch(() => "");
				return {
					error: `Failed to search events (${response.status}): ${errorBody || response.statusText}`,
				};
			}

			const events = await response.json();

			if (!Array.isArray(events)) {
				return { results: [], message: "No events found" };
			}

			return {
				results: events.map((e: any) => ({
					id: e.id,
					name: e.name,
					description: e.description,
					author: e.author?.name || "Unknown",
					community: e.community?.name || null,
					visibility: e.visibility,
					timing: e.timing,
					tags: e.tags || [],
					windows:
						e.windows?.map((w: any) => ({
							id: w.id,
							title: w.title,
							startTime: w.startTime,
							endTime: w.endTime,
							location: w.location
								? {
										address: w.location.address,
										coordinates: w.location.coordinates,
									}
								: null,
						})) || [],
				})),
			};
		} catch (err) {
			const message = err instanceof Error ? err.message : "Unknown error";
			return { error: `Event search failed: ${message}` };
		}
	},
});

export const getEventDetailsTool = tool({
	description:
		"Get detailed information about a specific event by its ID. Use this when the user asks for specific details about a named event or provides an event ID.",
	inputSchema: z.object({
		eventId: z.string().describe("The unique ID of the event to retrieve details for"),
	}),
	execute: async ({ eventId }, { abortSignal }) => {
		const url = `${getServicesUrl()}/event/api/${eventId}`;

		const response = await fetch(url, { signal: abortSignal });
		if (!response.ok) {
			if (response.status === 404) {
				return { error: "Event not found" };
			}
			return { error: `Failed to get event details: ${response.statusText}` };
		}

		const event = await response.json();

		return {
			id: event.id,
			name: event.name,
			description: event.description,
			author: {
				id: event.author?.id,
				name: event.author?.name,
			},
			community: event.community
				? {
						id: event.community.id,
						name: event.community.name,
					}
				: null,
			visibility: event.visibility,
			timing: event.timing,
			tags: event.tags || [],
			windows:
				event.windows?.map((w: any) => ({
					id: w.id,
					title: w.title,
					description: w.description,
					startTime: w.startTime,
					endTime: w.endTime,
					location: w.location
						? {
								address: w.location.address,
								coordinates: w.location.coordinates,
							}
						: null,
				})) || [],
		};
	},
});

export const searchTools = {
	mapboxsearch: mapboxSearchTool,
	eventsearch: eventSearchTool,
	geteventdetails: getEventDetailsTool,
};
