<script lang="ts">
	import { createIpGeoQuery } from "$lib/api";
	import type { IpGeo } from "$lib/effects/schemas/ip-geo.schema";
	import {
		GeolocationService,
		GeolocationServiceLive,
	} from "$lib/effects/services/geolocation.service";
	import { Effect, Runtime } from "effect";
	import mapboxgl from "mapbox-gl";
	import { onMount } from "svelte";
	import GeolocateControl from "./geolocate-control.svelte";

	interface Props {
		ipGeo: IpGeo | null;
	}

	let { ipGeo }: Props = $props();

	mapboxgl.accessToken = import.meta.env.VITE_MAPBOX_ACCESS_TOKEN;

	let map: mapboxgl.Map | undefined = $state();
	const mapStyle = "mapbox://styles/mufarodev/cmhty7aqj001h01s9cqw2ao4g";
	const runtime = Runtime.defaultRuntime;

	const approximateLocation = createIpGeoQuery({
		query: {
			initialData: ipGeo ?? undefined,
		},
	});

	onMount(async () => {
		let center: [number, number];
		let zoom: number;

		const program = Effect.gen(function* () {
			const geoService = yield* GeolocationService;
			const position = yield* geoService.getCurrentPosition({
				enableHighAccuracy: true,
				timeout: 6000,
				maximumAge: 0,
			});
			return position;
		});

		const runnable = Effect.provide(program, GeolocationServiceLive);

		try {
			const position = await Runtime.runPromise(runtime)(runnable);
			center = [position.longitude, position.latitude];
			zoom = 15;
		} catch {
			center = approximateLocation.data
				? [approximateLocation.data.lon, approximateLocation.data.lat]
				: [-74.5, 40];
			zoom = 9;
		}

		map = new mapboxgl.Map({
			container: "lythar-map",
			style: mapStyle,
			center,
			zoom,
		});
	});
</script>

<div class="relative flex min-h-svh w-svw flex-col overflow-hidden">
	<div id="lythar-map" class="w-full flex-1 rounded-4xl"></div>
	{#if map}
		<GeolocateControl {map} />
	{/if}
</div>
