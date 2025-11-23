<script lang="ts">
	import { createIpGeoQuery } from "$lib/api";
	import { getLayoutContext } from "$lib/context/layout.svelte";
	import type { IpGeo } from "$lib/effects/schemas/ip-geo.schema";
	import {
		GeolocationService,
		GeolocationServiceLive,
	} from "$lib/effects/services/geolocation.service";
	import { Effect, Runtime } from "effect";
	import mapboxgl from "mapbox-gl";
	import { mode } from "mode-watcher";
	import { onMount } from "svelte";
	import FabMenu from "./fab-menu.svelte";
	import GeolocateControl from "./geolocate-control.svelte";
	import MapControls from "./map-controls.svelte";
	import { getLightingPreset } from "./map-utils";
	import SearchBar from "./search-bar.svelte";
	import UserMenu from "./user-menu.svelte";
	import { mapState } from "./map-state.svelte";
	import MapPointCta from "./map-point-cta.svelte";
	import NewEventOverlay from "./new-event-overlay.svelte";
	import MapEventsLayer from "./map-events-layer.svelte";
	import EventDetailsOverlay from "./event-details-overlay.svelte";

	interface Props {
		ipGeo: IpGeo | null;
	}

	let { ipGeo }: Props = $props();
  	const layout = getLayoutContext();

	mapboxgl.accessToken = import.meta.env.VITE_MAPBOX_ACCESS_TOKEN;

	const mapStyle = "mapbox://styles/mapbox/standard";
	const runtime = Runtime.defaultRuntime;

  	let selectedLocation = $state<{ lng: number; lat: number } | null>(null);

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

		mapState.instance = new mapboxgl.Map({
			container: "lythar-map",
			style: mapStyle,
			center,
			zoom,
			config: {
				basemap: {
					lightPreset: getLightingPreset(mode.current || "light"),
				},
			},
		});

    const attrControl = new mapboxgl.AttributionControl({
      compact: true,
      customAttribution: '© <a href="https://lythar.com" target="_blank" rel="noopener noreferrer">Lythar</a>'
    });

    mapState.instance.addControl(attrControl, 'bottom-right');
	});

	$effect(() => {
		if (!mapState.instance) return;

		const currentMode = mode.current;
		if (!currentMode) return;

		const preset = getLightingPreset(currentMode);
		mapState.instance.setConfigProperty("basemap", "lightPreset", preset);
	});

	$effect(() => {
		if (!mapState.instance) return;

		const container = document.getElementById("lythar-map");
		if (!container) return;

		let resizeTimeout: NodeJS.Timeout;

		const resizeObserver = new ResizeObserver(() => {
			clearTimeout(resizeTimeout);
			// This timeout causes the resize to not flush the canvas data for a split second?
			resizeTimeout = setTimeout(() => {
				mapState.instance?.resize();
			}, 1);
		});

		resizeObserver.observe(container);

		return () => {
			clearTimeout(resizeTimeout);
			resizeObserver.disconnect();
		};
	});
</script>

<div class="relative flex h-full w-full flex-col overflow-hidden">
	<div id="lythar-map" class="flex-1 rounded-2xl"></div>

	{#if mapState.instance}
		<SearchBar />
    {#if layout.isDesktop}
		  <UserMenu class="absolute top-5 right-4" />
    {/if}
		<FabMenu />
		<MapControls map={mapState.instance} />
		<GeolocateControl map={mapState.instance} />
    <MapPointCta map={mapState.instance} bind:selectedLocation />
    <NewEventOverlay map={mapState.instance} />
	<EventDetailsOverlay />
	<MapEventsLayer map={mapState.instance} />
	{/if}
</div>

<style global>
  :global(#lythar-map > div.mapboxgl-control-container > div.mapboxgl-ctrl-bottom-right > div:nth-child(2)) {
    display: none;
  }
</style>
