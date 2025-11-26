import { tool } from "ai";
import { z } from "zod";
import { env as publicEnv } from "$env/dynamic/public";
import { env as privateEnv } from "$env/dynamic/private";

const getMapboxToken = () => publicEnv.PUBLIC_MAPBOX_ACCESS_TOKEN || "";
const getServicesUrl = () => privateEnv.INTERNAL_SERVICES_URL || "http://localhost:5164";

const EVENT_TAGS = [
	"Sport",
	"Music",
	"Art",
	"Food",
	"Technology",
	"Business",
	"Education",
	"Health",
	"Travel",
	"Fashion",
	"Gaming",
	"Film",
	"Literature",
	"Science",
	"Politics",
	"Religion",
	"Charity",
	"Family",
	"Networking",
	"Workshop",
	"Conference",
	"Festival",
	"Party",
	"Fitness",
	"Outdoor",
	"Indoor",
	"Virtual",
	"Free",
	"Paid",
	"Kids",
	"Adults",
	"Seniors",
	"LGBTQ",
	"Cultural",
	"Religious",
	"Community",
	"Professional",
	"Casual",
	"Formal",
	"Parenting",
	"Running",
] as const;

export const mapboxSearchTool = tool({
	description:
		"Search for physical locations, places, businesses, addresses, or points of interest using Mapbox. Use this for queries like 'pizza near me', 'museums in Paris', 'where is Central Park', etc. Also use this to find coordinates for a city or place when the user wants events near a location.",
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
		"Search for events using text search. Best for finding events by name or description keywords. For browsing/discovering events, use discoverEvents instead.",
	inputSchema: z.object({
		query: z.string().describe("The text search query for events (min 2 characters)"),
		limit: z.number().optional().default(10).describe("Maximum number of results to return"),
	}),
	execute: async ({ query, limit }, { abortSignal }) => {
		try {
			if (query.length < 2) {
				return { error: "Search query must be at least 2 characters", results: [] };
			}

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
				results: events.map((e: any) => formatEventResult(e)),
			};
		} catch (err) {
			const message = err instanceof Error ? err.message : "Unknown error";
			return { error: `Event search failed: ${message}` };
		}
	},
});

export const discoverEventsTool = tool({
	description: `Browse and discover events. Use this for:
- Finding random/any events ("show me events", "what's happening")
- Filtering by category/tag ("sport events", "music festivals")
- Finding events near a location ("events in Warsaw", "things to do near me")
- Date-based queries ("events this weekend", "concerts tonight")
Available tags: ${EVENT_TAGS.join(", ")}`,
	inputSchema: z.object({
		tags: z
			.array(z.string())
			.optional()
			.describe(
				`Filter by event categories. Available: ${EVENT_TAGS.join(", ")}. Match user intent to closest tag(s).`,
			),
		boundingBox: z
			.object({
				minLon: z.number(),
				minLat: z.number(),
				maxLon: z.number(),
				maxLat: z.number(),
			})
			.optional()
			.describe(
				"Geographic bounding box to filter events. Get coordinates from mapboxsearch first if user mentions a city/location.",
			),
		startDate: z
			.string()
			.optional()
			.describe("Filter events starting after this ISO date (e.g., 2025-11-26)"),
		endDate: z.string().optional().describe("Filter events ending before this ISO date"),
		limit: z.number().optional().default(10).describe("Maximum number of results"),
	}),
	execute: async ({ tags, boundingBox, startDate, endDate, limit }, { abortSignal }) => {
		try {
			const baseUrl = getServicesUrl();
			const params = new URLSearchParams();

			params.set("limit", String(limit || 10));

			if (tags && tags.length > 0) {
				tags.forEach((tag) => params.append("tags", tag));
			}

			if (boundingBox) {
				params.set("minLon", String(boundingBox.minLon));
				params.set("minLat", String(boundingBox.minLat));
				params.set("maxLon", String(boundingBox.maxLon));
				params.set("maxLat", String(boundingBox.maxLat));
			}

			if (startDate) {
				params.set("startDate", startDate);
			}

			if (endDate) {
				params.set("endDate", endDate);
			}

			const url = `${baseUrl}/event/api/geojson?${params.toString()}`;

			const response = await fetch(url, { signal: abortSignal });
			if (!response.ok) {
				const errorBody = await response.text().catch(() => "");
				return {
					error: `Failed to discover events (${response.status}): ${errorBody || response.statusText}`,
				};
			}

			const geojson = await response.json();
			const features = geojson.features || [];

			if (features.length === 0) {
				return {
					results: [],
					message: "No events found matching your criteria. Try broadening your search.",
				};
			}

			return {
				results: features.map((f: any) => ({
					id: f.properties?.eventId,
					name: f.properties?.eventName,
					description: f.properties?.eventDescription,
					author: f.properties?.authorName || "Unknown",
					tags: f.properties?.tags || [],
					timing: f.properties?.timing,
					coordinates: f.geometry?.coordinates,
					startTime: f.properties?.startTime,
					endTime: f.properties?.endTime,
				})),
				totalFound: features.length,
			};
		} catch (err) {
			const message = err instanceof Error ? err.message : "Unknown error";
			console.error("Discover Events Tool Error:", err);
			return { error: `Event discovery failed: ${message}` };
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

function formatEventResult(e: any) {
	return {
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
	};
}

export const searchTools = {
	mapboxsearch: mapboxSearchTool,
	eventsearch: eventSearchTool,
	discoverevents: discoverEventsTool,
	geteventdetails: getEventDetailsTool,
};
