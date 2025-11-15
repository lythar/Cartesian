import mapboxgl from "mapbox-gl";

export const geolocateControl = new mapboxgl.GeolocateControl({
	positionOptions: {
		enableHighAccuracy: true,
	},
	trackUserLocation: true,
	showUserHeading: true,
});
