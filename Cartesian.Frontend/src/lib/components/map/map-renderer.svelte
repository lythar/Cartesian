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
	import { createEventsGeojsonQuery } from "$lib/api/queries/events-geojson.query";

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

	const eventsQuery = createEventsGeojsonQuery({}, {
		query: {
			enabled: true,
			refetchInterval: 30000,
			staleTime: 15000,
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

		mapState.instance.on("load", () => {
			mapState.instance!.addSource("events", {
				type: "geojson",
				generateId: true,
				data: (eventsQuery.data ?? { type: "FeatureCollection", features: [] }) as GeoJSON.FeatureCollection,
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

			mapState.instance!.addLayer({
				id: "unclustered-point",
				type: "circle",
				source: "events",
				filter: ["!", ["has", "point_count"]],
				paint: {
					"circle-color": mode.current == "light" ? "#FFFFFF" : "#101010",
					"circle-radius": 8,
					"circle-stroke-width": 2,
					"circle-stroke-color": mode.current == "light" ? "#101010" : "#FFFFFF",
					"circle-emissive-strength": 1,
				},
			});

      mapState.instance!.addInteraction("click-clusters", {
        type: "click",
        target: { layerId: "clusters" },
        handler: e => {
          const features = mapState.instance!.queryRenderedFeatures(e.point, { layers: ["clusters"] });

					if (!features.length) return;

          const clusterId = features[0].properties!.cluster_id;

					const source = mapState.instance!.getSource("events") as mapboxgl.GeoJSONSource;
					source.getClusterExpansionZoom(
            clusterId,
            (err: any, zoom: any ) => {
              if (err) return;

              mapState.instance!.easeTo({
                center: (features[0].geometry as GeoJSON.Point).coordinates as [number, number],
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

      mapState.instance!.addInteraction("click-unclustered-point", {
        type: "click",
        target: { layerId: "unclustered-point" },
        handler: e => {
          const features = mapState.instance!.queryRenderedFeatures(e.point, { layers: ["unclustered-point"] });
          if (!features.length) return;

          const coordinates = (features[0].geometry as GeoJSON.Point).coordinates.slice();
          const properties = features[0].properties;

          while (Math.abs(e.lngLat.lng - coordinates[0]) > 180) {
            coordinates[0] += e.lngLat.lng > coordinates[0] ? 360 : -360;
          }

          new mapboxgl.Popup()
            .setLngLat(coordinates as [number, number])
            .setHTML(`
              <div class="p-2">
                <h3 class="font-semibold text-sm mb-1">${properties?.name ?? "Event"}</h3>
                <p class="text-xs text-muted-foreground">${properties?.description ?? ""}</p>
              </div>
            `)
            .addTo(mapState.instance!);
        }
      });

      mapState.instance!.addInteraction("unclustered-point-mouseenter", {
        type: "mouseenter",
        target: { layerId: "unclustered-point" },
        handler: () => {
          mapState.instance!.getCanvas().style.cursor = "pointer";
        }
      });

      mapState.instance!.addInteraction("unclustered-point-mouseleave", {
        type: "mouseleave",
        target: { layerId: "unclustered-point" },
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

		if (mapState.instance.isStyleLoaded()) {
			const textColor = currentMode === "light" ? "#101010" : "#FFFFFF";
			const circleColor = currentMode === "light" ? "#FFFFFF" : "#101010";
			const strokeColor = currentMode === "light" ? "#101010" : "#FFFFFF";

			if (mapState.instance.getLayer("clusters")) {
				mapState.instance.setPaintProperty("clusters", "circle-color", circleColor);
			}
			if (mapState.instance.getLayer("cluster-count")) {
				mapState.instance.setPaintProperty("cluster-count", "text-color", textColor);
			}
			if (mapState.instance.getLayer("unclustered-point")) {
				mapState.instance.setPaintProperty("unclustered-point", "circle-color", circleColor);
				mapState.instance.setPaintProperty("unclustered-point", "circle-stroke-color", strokeColor);
			}
		}
	});

	$effect(() => {
		if (!mapState.instance || !mapState.instance.isStyleLoaded()) return;

		const source = mapState.instance.getSource("events") as mapboxgl.GeoJSONSource;
		if (source && eventsQuery.data) {
			source.setData(eventsQuery.data as GeoJSON.FeatureCollection);
		}
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
	{/if}
</div>

<style global>
  :global(#lythar-map > div.mapboxgl-control-container > div.mapboxgl-ctrl-bottom-right > div:nth-child(2)) {
    display: none;
  }
</style>
