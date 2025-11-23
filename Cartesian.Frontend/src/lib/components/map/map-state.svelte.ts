import { browser } from "$app/environment";
import type { LatLng } from "$lib/utils";
import mapboxgl from "mapbox-gl";

export let geolocateState = $state({ state: "disabled" });
export let navigationMode = $state({ enabled: false });
export let mapState = $state<{ instance: mapboxgl.Map | null }>({ instance: null });
export let mapMarkers = $state<mapboxgl.Marker[]>([]);
export let newEventMarkerLocation = $state<{ data: LatLng } | null>(null);

export interface MapEventProperties {
	eventId: string;
	eventName: string;
	eventDescription: string;
	windowId: string;
	windowTitle: string;
	windowDescription: string;
	authorId: string;
	authorName: string;
	authorAvatar: string;
	communityId: string | null;
	communityName: string | null;
	communityAvatar: string | null;
	visibility: string;
	timing: string;
	tags: string[];
	startTime: string;
	endTime: string;
	createdAt: string;
}

export const mapInteractionState = $state({
	selectedEvent: null as MapEventProperties | null,
	eventDetailsOpen: false,
});

export let newEventOverlayState = $state<{
	open: boolean;
	/**
	 * create - user clicked on add event button
	 * picker - user clicked on map > "add event here"
	 * progammatic - restore draft / deep-link?
	 */
	source: "create" | "toolbar" | "programmatic";
	location: LatLng | null;
}>({
	open: false,
	source: "create",
	location: null,
});

export let deviceHeading = $state<{ heading: number | null }>({ heading: null });

if (browser) {
	if ("ondeviceorientationabsolute" in window) {
		(window as Window).addEventListener(
			"deviceorientationabsolute",
			(event: DeviceOrientationEvent) => {
				if (event.absolute && event.alpha !== null) {
					deviceHeading.heading = 360 - event.alpha;
				}
			},
		);
	} else if ("ondeviceorientation" in window) {
		(window as Window).addEventListener(
			"deviceorientation",
			(event: DeviceOrientationEvent) => {
				const webkitEvent = event as DeviceOrientationEvent & {
					webkitCompassHeading?: number;
				};
				if (event.alpha !== null && webkitEvent.webkitCompassHeading !== undefined) {
					deviceHeading.heading = webkitEvent.webkitCompassHeading;
				} else if (event.alpha !== null) {
					deviceHeading.heading = 360 - event.alpha;
				}
			},
		);
	}
}

export const geolocateControl = new mapboxgl.GeolocateControl({
	positionOptions: {
		enableHighAccuracy: true,
	},
	trackUserLocation: true,
	showUserHeading: true,
});

const originalUpdateCamera = geolocateControl._updateCamera.bind(geolocateControl);

geolocateControl._updateCamera = function (position: GeolocationPosition) {
	if (navigationMode.enabled && mapState.instance) {
		console.log("Navigation mode update");

		this._lastKnownPosition = position;

		mapState.instance.easeTo(
			{
				center: [position.coords.longitude, position.coords.latitude],
				pitch: 50,
				zoom: 18,
				bearing:
					deviceHeading.heading !== null
						? deviceHeading.heading
						: position.coords.heading !== null
							? position.coords.heading
							: mapState.instance.getBearing(),
				duration: 1000,
				essential: true,
			},
			{
				geolocateSource: true,
			},
		);
	} else {
		originalUpdateCamera(position);
	}
};
