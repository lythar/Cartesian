<script lang="ts">
	import mapboxgl from "mapbox-gl";
	import { onMount } from "svelte";
	import { Navigation03Icon, Navigation04Icon, Navigation06Icon } from "@hugeicons/core-free-icons";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { Button } from "$lib/components/ui/button";
	import { geolocateControl, geolocateState, navigationMode } from "./map-state.svelte";

	interface Props {
		map: mapboxgl.Map;
	}

	let { map }: Props = $props();


	onMount(() => {
		geolocateControl.on("geolocate", (e) => {
      if(navigationMode.enabled && geolocateState.state === "active") {

      }
      console.log('Geolocation event triggered');
		});

		geolocateControl.on("trackuserlocationstart", () => {
			geolocateState.state = "active";
      console.log('User location tracking started');
		});

		geolocateControl.on("trackuserlocationend", () => {
			geolocateState.state = "passive";
      console.log('User location tracking ended');
		});

		geolocateControl.on("error", () => {
			geolocateState.state = "disabled";
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

</script>

<!-- {#if geolocateState.state === "passive"}
	<Button
		type="button"
		class="absolute top-4 right-4 z-10 w-28 h-12 flex items-center justify-center"
		onclick={handleRecenter}
    size="lg"
		aria-label="Recenter map on my location"
	>
		<HugeiconsIcon size={24} strokeWidth={2} className="fill-current/50 size-6" icon={Navigation03Icon} /> Recenter
	</Button>
{/if} -->

<style>
	:global(.mapboxgl-ctrl-geolocate) {
		display: none !important;
	}
</style>
