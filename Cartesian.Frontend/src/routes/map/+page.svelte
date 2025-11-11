<script lang="ts">
	import mapboxgl from "mapbox-gl";
	import { onMount } from "svelte";

	mapboxgl.accessToken = import.meta.env.VITE_MAPBOX_ACCESS_TOKEN;

	let map: mapboxgl.Map;

	onMount(() => {
		map = new mapboxgl.Map({
			container: "lythar-map",
			style: "mapbox://styles/mufarodev/cmhty7aqj001h01s9cqw2ao4g",
			center: [-74.5, 40], // starting position [lng, lat]
			zoom: 9, // starting zoom,
		});
	});
</script>

<div id="lythar-map" class="test-class h-screen w-screen"></div>
