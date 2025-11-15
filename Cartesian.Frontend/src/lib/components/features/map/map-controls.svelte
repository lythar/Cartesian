<script lang="ts">
	import { Button } from "$lib/components/ui/button";
	import { Plus, Minus, Locate, Map } from "@lucide/svelte";
	import { animate } from "motion";
	import type mapboxgl from "mapbox-gl";

	interface Props {
		map: mapboxgl.Map;
	}

	let { map }: Props = $props();
	let locateButtonRef: HTMLButtonElement | undefined = $state();
	let mapStyleButtonRef: HTMLButtonElement | undefined = $state();

	let mapStyle = $state<"streets" | "dark" | "satellite">("streets");

	const styleMap = {
		streets: "mapbox://styles/mufarodev/cmhty7aqj001h01s9cqw2ao4g",
		dark: "mapbox://styles/mapbox/dark-v11",
		satellite: "mapbox://styles/mapbox/satellite-streets-v12",
	};

	function cycleMapStyle() {
		const styles: Array<"streets" | "dark" | "satellite"> = ["streets", "dark", "satellite"];
		const currentIndex = styles.indexOf(mapStyle);
		mapStyle = styles[(currentIndex + 1) % styles.length];
		map.setStyle(styleMap[mapStyle]);
	}

	function recenterMap() {
		if (locateButtonRef) {
			animate(
				locateButtonRef,
				{ transform: ["rotate(0deg)", "rotate(360deg)"] },
				{ duration: 0.6, ease: [0.32, 0.72, 0, 1] }
			);
		}

		navigator.geolocation.getCurrentPosition(
			(position) => {
				map.flyTo({
					center: [position.coords.longitude, position.coords.latitude],
					zoom: 15,
					essential: true,
				});
			},
			(error) => {
				console.error("Error getting location:", error);
			}
		);
	}

	function handleStyleChange() {
		cycleMapStyle();
		if (mapStyleButtonRef) {
			animate(
				mapStyleButtonRef,
				{ transform: ["scale(1)", "scale(1.15)", "scale(1)"] },
				{ duration: 0.3, ease: [0.32, 0.72, 0, 1] }
			);
		}
	}
</script>

<div class="absolute bottom-8 left-8 z-20 flex flex-col gap-3">
	<div class="flex flex-col gap-1.5 rounded-2xl bg-sidebar/95 backdrop-blur-sm shadow-xl border border-border/50 p-1.5">
		<Button
			variant="ghost"
			size="icon-lg"
			class="size-14 hover:scale-105 transition-all duration-200"
			onclick={() => map.zoomIn()}
		>
			<Plus class="size-5" />
		</Button>
		<div class="h-px bg-border mx-1"></div>
		<Button
			variant="ghost"
			size="icon-lg"
			class="size-14 hover:scale-105 transition-all duration-200"
			onclick={() => map.zoomOut()}
		>
			<Minus class="size-5" />
		</Button>
	</div>

	<Button
		bind:this={locateButtonRef}
		variant="ghost"
		size="icon-lg"
		class="size-14 rounded-2xl bg-sidebar/95 backdrop-blur-sm shadow-xl border border-border/50 hover:scale-105 transition-all duration-200"
		onclick={recenterMap}
	>
		<Locate class="size-5" />
	</Button>

	<Button
		bind:this={mapStyleButtonRef}
		variant="ghost"
		size="icon-lg"
		class="size-14 rounded-2xl bg-sidebar/95 backdrop-blur-sm shadow-xl border border-border/50 hover:scale-105 transition-all duration-200"
		onclick={handleStyleChange}
	>
		<Map class="size-5" />
	</Button>
</div>
