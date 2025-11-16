<script lang="ts">
	import { createIpGeoQuery } from "$lib/api";
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
	import { mode } from "mode-watcher";
	import { onMount } from "svelte";
	import FabMenu from "./fab-menu.svelte";
	import GeolocateControl from "./geolocate-control.svelte";
	import MapControls from "./map-controls.svelte";
	import { getLightingPreset } from "./map-utils";
	import SearchBar from "./search-bar.svelte";
	import UserMenu from "./user-menu.svelte";
	import { mapState } from "./map-state.svelte";

	interface Props {
		ipGeo: IpGeo | null;
	}

	let { ipGeo }: Props = $props();
  const layout = getLayoutContext();

	mapboxgl.accessToken = import.meta.env.VITE_MAPBOX_ACCESS_TOKEN;

	const mapStyle = "mapbox://styles/mapbox/standard";
	const runtime = Runtime.defaultRuntime;

	const approximateLocation = createIpGeoQuery({
		query: {
			initialData: ipGeo ?? undefined,
		},
	});

	let selectedLocation: { lng: number; lat: number } | null = $state(null);

	let markers = $state<mapboxgl.Marker[]>([]);

	const handleMapClick = (e: mapboxgl.MapMouseEvent) => {
		// Remove previous marker
		markers.forEach((m) => m.remove());
		markers = [];

		const markerElement = document.createElement("img");
		markerElement.src = "/lythar.svg";
		markerElement.className = "size-12 transform -translate-y-1/2";
		const marker = new mapboxgl.Marker({
			color: "#BB0000",
			element: markerElement,
		})
			.setLngLat(e.lngLat)
			.addTo(mapState.instance!);

		selectedLocation = { lng: e.lngLat.lng, lat: e.lngLat.lat };

		markers = [marker];
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

		mapState.instance.on("click", handleMapClick);

		mapState.instance.on("load", () => {
			mapState.instance!.addSource("events", {
				type: "geojson",
				generateId: true,
				data: "https://docs.mapbox.com/mapbox-gl-js/assets/earthquakes.geojson", // CHANGE THIS LATER TO ACTUAL EVENTS DATA
				cluster: true,
				clusterMaxZoom: 14,
				clusterRadius: 50,
			});

			mapState.instance!.addLayer({
				id: "clusters",
				type: "circle",
				source: "events",
				filter: ["has", "point_count"],
				paint: {
					"circle-color": mode.current == "light" ? "#FFFFFF" : "#101010",
					"circle-radius": ["step", ["get", "point_count"], 20, 100, 30, 750, 40],
					"circle-emissive-strength": 1,
				},
			});

			mapState.instance!.addLayer({
				id: "cluster-count",
				type: "symbol",
				source: "events",
				filter: ["has", "point_count"],
				layout: {
					"text-field": ["get", "point_count_abbreviated"],
					"text-font": ["DIN Offc Pro Medium", "Arial Unicode MS Bold"],
					"text-size": 12,
				},
				paint: {
					"text-color": mode.current == "light" ? "#101010" : "#FFFFFF",
				},
			});

      mapState.instance!.addInteraction("click-clusters", {
        type: "click",
        target: { layerId: "clusters" },
        handler: e => {
          const features = mapState.instance.queryRenderedFeatures(e.point, { layers: ["clusters"] });
          const clusterId = features[0].properties.cluster_id;
          mapState.instance!.getSource("events").getClusterExpansionZoom(
            clusterId,
            (err, zoom) => {
              if (err) return;

              mapState.instance!.easeTo({
                center: features[0].geometry.coordinates,
                zoom: zoom,
              })
            }
          )
        }
      })

      mapState.instance!.addInteraction("clusters-mouseenter", {
        type: "mouseenter",
        target: { layerId: "clusters" },
        handler: () => {
          mapState.instance!.getCanvas().style.cursor = "pointer";
        }
      });

      mapState.instance!.addInteraction("clusters-mouseleave", {
        type: "mouseleave",
        target: { layerId: 'clusters' },
        handler: () => {
          mapState.instance!.getCanvas().style.cursor = "";
        }
      });
		});
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
