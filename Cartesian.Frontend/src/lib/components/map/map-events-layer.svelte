<script lang="ts">
	import { createEventsGeojsonQuery } from "$lib/api/queries/events-geojson.query";
	import mapboxgl from "mapbox-gl";
	import { mode } from "mode-watcher";

	interface Props {
		map: mapboxgl.Map;
	}

	let { map }: Props = $props();

	const eventsQuery = createEventsGeojsonQuery(
		{},
		{
			query: {
				enabled: true,
				refetchInterval: 30000,
				staleTime: 15000
			}
		}
	);

	function addEventsLayer() {
		if (!map.isStyleLoaded()) return;
		if (map.getSource("events")) return;

		map.addSource("events", {
			type: "geojson",
			generateId: true,
			data: (eventsQuery.data ?? {
				type: "FeatureCollection",
				features: []
			}) as GeoJSON.FeatureCollection,
			cluster: true,
			clusterMaxZoom: 14,
			clusterRadius: 50
		});

		map.addLayer({
			id: "clusters",
			type: "circle",
			source: "events",
			filter: ["has", "point_count"],
			paint: {
				"circle-color": mode.current == "light" ? "#FFFFFF" : "#101010",
				"circle-radius": ["step", ["get", "point_count"], 20, 100, 30, 750, 40],
				"circle-emissive-strength": 1
			}
		});

		map.addLayer({
			id: "cluster-count",
			type: "symbol",
			source: "events",
			filter: ["has", "point_count"],
			layout: {
				"text-field": ["get", "point_count_abbreviated"],
				"text-font": ["DIN Offc Pro Medium", "Arial Unicode MS Bold"],
				"text-size": 12
			},
			paint: {
				"text-color": mode.current == "light" ? "#101010" : "#FFFFFF"
			}
		});

		map.addLayer({
			id: "unclustered-point",
			type: "circle",
			source: "events",
			filter: ["!", ["has", "point_count"]],
			paint: {
				"circle-color": mode.current == "light" ? "#FFFFFF" : "#101010",
				"circle-radius": 8,
				"circle-stroke-width": 2,
				"circle-stroke-color": mode.current == "light" ? "#101010" : "#FFFFFF",
				"circle-emissive-strength": 1
			}
		});

		// @ts-ignore - addInteraction seems to be present but types might vary or be custom
		map.addInteraction("click-clusters", {
			type: "click",
			target: { layerId: "clusters" },
			handler: (e: any) => {
				const features = map.queryRenderedFeatures(e.point, { layers: ["clusters"] });

				if (!features.length) return;

				const clusterId = features[0].properties!.cluster_id;

				const source = map.getSource("events") as mapboxgl.GeoJSONSource;
				source.getClusterExpansionZoom(clusterId, (err: any, zoom: any) => {
					if (err) return;

					map.easeTo({
						center: (features[0].geometry as GeoJSON.Point).coordinates as [number, number],
						zoom: zoom
					});
				});
			}
		});

		map.addInteraction("clusters-mouseenter", {
			type: "mouseenter",
			target: { layerId: "clusters" },
			handler: () => {
				map.getCanvas().style.cursor = "pointer";
			}
		});

		map.addInteraction("clusters-mouseleave", {
			type: "mouseleave",
			target: { layerId: "clusters" },
			handler: () => {
				map.getCanvas().style.cursor = "";
			}
		});

		map.addInteraction("click-unclustered-point", {
			type: "click",
			target: { layerId: "unclustered-point" },
			handler: (e: any) => {
				const features = map.queryRenderedFeatures(e.point, { layers: ["unclustered-point"] });
				if (!features.length) return;

				const coordinates = (features[0].geometry as GeoJSON.Point).coordinates.slice();
				const properties = features[0].properties;

				while (Math.abs(e.lngLat.lng - coordinates[0]) > 180) {
					coordinates[0] += e.lngLat.lng > coordinates[0] ? 360 : -360;
				}

				new mapboxgl.Popup()
					.setLngLat(coordinates as [number, number])
					.setHTML(
						`
              <div class="p-2">
                <h3 class="font-semibold text-sm mb-1">${properties?.name ?? "Event"}</h3>
                <p class="text-xs text-muted-foreground">${properties?.description ?? ""}</p>
              </div>
            `
					)
					.addTo(map);
			}
		});

		map.addInteraction("unclustered-point-mouseenter", {
			type: "mouseenter",
			target: { layerId: "unclustered-point" },
			handler: () => {
				map.getCanvas().style.cursor = "pointer";
			}
		});

		map.addInteraction("unclustered-point-mouseleave", {
			type: "mouseleave",
			target: { layerId: "unclustered-point" },
			handler: () => {
				map.getCanvas().style.cursor = "";
			}
		});
	}

	$effect(() => {
		if (map.isStyleLoaded()) {
			addEventsLayer();
		} else {
			map.once("load", addEventsLayer);
		}

		return () => {
			try {
				if (map.getLayer("clusters")) map.removeLayer("clusters");
				if (map.getLayer("cluster-count")) map.removeLayer("cluster-count");
				if (map.getLayer("unclustered-point")) map.removeLayer("unclustered-point");
				if (map.getSource("events")) map.removeSource("events");
			} catch (e) {
				console.warn("Error cleaning up map events layer", e);
			}
		};
	});

	$effect(() => {
		const currentMode = mode.current;
		if (!currentMode || !map.isStyleLoaded()) return;

		const textColor = currentMode === "light" ? "#101010" : "#FFFFFF";
		const circleColor = currentMode === "light" ? "#FFFFFF" : "#101010";
		const strokeColor = currentMode === "light" ? "#101010" : "#FFFFFF";

		if (map.getLayer("clusters")) {
			map.setPaintProperty("clusters", "circle-color", circleColor);
		}
		if (map.getLayer("cluster-count")) {
			map.setPaintProperty("cluster-count", "text-color", textColor);
		}
		if (map.getLayer("unclustered-point")) {
			map.setPaintProperty("unclustered-point", "circle-color", circleColor);
			map.setPaintProperty("unclustered-point", "circle-stroke-color", strokeColor);
		}
	});

	$effect(() => {
		if (!map.isStyleLoaded()) return;

		const source = map.getSource("events") as mapboxgl.GeoJSONSource;
		if (source && eventsQuery.data) {
			source.setData(eventsQuery.data as GeoJSON.FeatureCollection);
		}
	});
</script>
