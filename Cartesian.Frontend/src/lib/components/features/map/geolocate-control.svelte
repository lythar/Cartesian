<script lang="ts">
	import mapboxgl from "mapbox-gl";
	import { onMount } from "svelte";
	import { Navigation03Icon, Navigation04Icon, Navigation06Icon } from "@hugeicons/core-free-icons";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { Button } from "$lib/components/ui/button";
	import { geolocateControl } from "./map-state";

	interface Props {
		map: mapboxgl.Map;
	}

	let { map }: Props = $props();

	let geolocateState = $state("disabled");

	onMount(() => {
		geolocateControl.on("geolocate", () => {
			// geolocateState = "active";
      console.log('Geolocation event triggered');
		});

		geolocateControl.on("trackuserlocationstart", () => {
			geolocateState = "active";
      console.log('User location tracking started');
		});

		geolocateControl.on("trackuserlocationend", () => {
			geolocateState = "passive";
      console.log('User location tracking ended');
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
      geolocateState = "active";
    }
  }
</script>

{#if geolocateState === "passive"}
	<Button
		type="button"
		class="absolute top-4 right-4 z-10 w-28 h-12 flex items-center justify-center"
		onclick={handleRecenter}
    size="lg"
		aria-label="Recenter map on my location"
	>
		<HugeiconsIcon size={24} strokeWidth={2} className="fill-current/50 size-6" icon={Navigation03Icon} /> Recenter
	</Button>
{/if}

<style>
	:global(.mapboxgl-ctrl-geolocate) {
		display: none !important;
	}
</style>
