<script lang="ts">
	import { createIpGeoQuery } from "$lib/api";
	import PaneRenderer from "$lib/components/layout/pane-renderer.svelte";
	import Button from "$lib/components/ui/button/button.svelte";
	import Input from "$lib/components/ui/input/input.svelte";
	import Label from "$lib/components/ui/label/label.svelte";
	import { useSidebar } from "$lib/components/ui/sidebar";
	import Textarea from "$lib/components/ui/textarea/textarea.svelte";
	import { getLayoutContext } from "$lib/context/layout.svelte";
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
	const sidebar = useSidebar();

	const approximateLocation = createIpGeoQuery({
		query: {
			initialData: ipGeo ?? undefined,
		},
	});

	let selectedLocation: { lng: number; lat: number } | null = $state(null);

	let curMarker = $state<mapboxgl.Marker | null>(null);

	const handleMapClick = (e: mapboxgl.MapMouseEvent) => {
		// Remove previous marker
		curMarker?.remove();

		const markerElement = document.createElement("img");
		markerElement.src = "/lythar.svg";
		markerElement.className = "size-12 transform -translate-y-1/2";
		const marker = new mapboxgl.Marker({
			color: "#BB0000",
			element: markerElement,
		})
			.setLngLat(e.lngLat)
			.addTo(map!);

		selectedLocation = { lng: e.lngLat.lng, lat: e.lngLat.lat };

		curMarker = marker;
	};

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

		map.on("click", handleMapClick);
	});

	$effect(() => {
		if (!map) return;

		const container = document.getElementById("lythar-map");
		if (!container) return;

		let resizeTimeout: NodeJS.Timeout;

		const resizeObserver = new ResizeObserver(() => {
			clearTimeout(resizeTimeout);
			// This timeout causes the resize to not flush the canvas data for a split second?
			resizeTimeout = setTimeout(() => {
				map?.resize();
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

	<PaneRenderer />

	{#if map}
		<GeolocateControl {map} />
	{/if}

	{#if selectedLocation && getLayoutContext().isDesktop}
		<!-- prolly should be in a seperate component -->
		<div class="absolute top-10 right-2 bottom-10 rounded-2xl bg-background p-10">
			<Button
				class="absolute top-10 right-10 mb-4 h-9 w-9"
				onclick={() => (selectedLocation = null)}>X</Button
			>
			<h3 class="text-2xl font-bold">Add event</h3>
			<form action="#" class="">
				<Label for="event-title" class="mt-4 mb-2 block">Title</Label>
				<Input id="event-title" type="text" />

				<Label for="event-description" class="mt-4 mb-2 block">Description</Label>
				<Textarea id="event-description" />

				<Label for="start-time" class="mt-4 mb-2 block">Start Time</Label>
				<Input id="start-time" type="datetime-local" />

				<Label for="end-time" class="mt-4 mb-2 block">End Time</Label>
				<Input id="end-time" type="datetime-local" />

				<Button type="submit" class="mt-4 w-full">Create Event</Button>
			</form>
		</div>
	{/if}
</div>
