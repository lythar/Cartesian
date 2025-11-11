<script lang="ts">
	import mapboxgl from "mapbox-gl";
	import { onMount } from "svelte";
	import { Navigation06Icon } from "@hugeicons/core-free-icons";
	import { HugeiconsIcon } from "@hugeicons/svelte";

	interface Props {
		map: mapboxgl.Map;
	}

	let { map }: Props = $props();

	let geolocateState = $state("disabled");
	let geolocateControl = $state<mapboxgl.GeolocateControl>();

	onMount(() => {
		geolocateControl = new mapboxgl.GeolocateControl({
			positionOptions: {
				enableHighAccuracy: true,
			},
			trackUserLocation: true,
			showUserHeading: true,
		});

		geolocateControl.on("geolocate", () => {
			geolocateState = "active";
		});

		geolocateControl.on("trackuserlocationstart", () => {
			geolocateState = "active";
		});

		geolocateControl.on("trackuserlocationend", () => {
			geolocateState = "passive";
		});

		geolocateControl.on("error", () => {
			geolocateState = "disabled";
		});

		map.addControl(geolocateControl);

		map.on("load", () => {
			geolocateControl!.trigger();
		});

		return () => {
			if (geolocateControl) {
				map.removeControl(geolocateControl);
			}
		};
	});

	function handleRecenter() {
		if (geolocateControl) {
			geolocateControl.trigger();
		}
	}
</script>

{#if geolocateState === "passive"}
	<button
		type="button"
		class="absolute top-4 right-4 z-10 flex h-[29px] w-[29px] items-center justify-center rounded bg-white shadow-md transition-opacity hover:bg-gray-50"
		onclick={handleRecenter}
		aria-label="Recenter map on my location"
	>
		<HugeiconsIcon icon={Navigation06Icon} />
	</button>
{/if}

<style>
	:global(.mapboxgl-ctrl-geolocate) {
		display: none !important;
	}
</style>
