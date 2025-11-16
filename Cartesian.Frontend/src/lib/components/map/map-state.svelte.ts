import { browser } from "$app/environment";
import mapboxgl from "mapbox-gl";

export let geolocateState = $state({ state: "disabled" });
export let navigationMode = $state({ enabled: false });
export let mapState = $state<{ instance: mapboxgl.Map | null }>({ instance: null });

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
