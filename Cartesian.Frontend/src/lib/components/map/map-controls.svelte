<script lang="ts">
	import { Button } from "$lib/components/ui/button";
	import { animate } from "motion";
	import { getLightingPreset } from "./map-utils";
	import { mode } from "mode-watcher";
	import * as ButtonGroup from "$lib/components/ui/button-group/index";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import {
		Add01Icon,
		Gps02Icon,
		LayerIcon,
		Navigation04Icon,
		Remove01Icon,
	} from "@hugeicons/core-free-icons";
	import { getLayoutContext } from "$lib/context/layout.svelte";
	import {
		deviceHeading,
		geolocateControl,
		geolocateState,
		navigationMode,
	} from "./map-state.svelte";
	import { cn } from "$lib/utils";
	import * as Sidebar from "$lib/components/ui/sidebar";

	interface Props {
		map: mapboxgl.Map;
	}

	let { map }: Props = $props();
	let locateButtonRef: HTMLButtonElement | undefined = $state();
	let mapStyleButtonRef: HTMLButtonElement | undefined = $state();

	let mapStyle = $state<"streets" | "satellite">("streets");

	const layout = getLayoutContext();

	const styleMap = {
		streets: "mapbox://styles/mapbox/standard",
		satellite: "mapbox://styles/mapbox/standard-satellite",
	};

	function cycleMapStyle() {
		const styles: Array<"streets" | "satellite"> = ["streets", "satellite"];
		const currentIndex = styles.indexOf(mapStyle);
		mapStyle = styles[(currentIndex + 1) % styles.length];
		map.setStyle(styleMap[mapStyle], {
			config: {
				basemap: {
					lightPreset: getLightingPreset(mode.current || "light"),
				},
			},
			localFontFamily: undefined,
			localIdeographFontFamily: undefined,
		});
	}

	function locationButtonPress() {
		if (geolocateState.state === "passive") {
			recenterMap();
		} else if (geolocateState.state === "active") {
			let isEnabled = navigationMode.enabled;

			map.easeTo(
				{
					pitch: isEnabled ? 0 : 50,
					zoom: isEnabled ? 15 : 18,
					bearing: isEnabled ? 0 : (deviceHeading.heading ?? map.getBearing()),
					duration: 1000,
				},
				{
					geolocateSource: true,
				},
			);

			navigationMode.enabled = !navigationMode.enabled;
		}
	}

	// $effect(() => {
	//   if (geolocateState.state !== "active") {
	//     navigationMode.enabled = false;
	//   }
	// })

	function recenterMap() {
		geolocateControl.trigger();
	}

	function handleStyleChange() {
		cycleMapStyle();
	}
</script>

<ButtonGroup.Root
	orientation="vertical"
	class={cn(
		"absolute bottom-24 left-4 z-20 lg:bottom-4 [&_button]:dark:bg-card [&_button]:dark:hover:bg-secondary",
		layout.isMobile && "[&_button]:size-16! [&_svg]:size-7!",
	)}
>
	{#if layout.isDesktop}
		<ButtonGroup.Root orientation="vertical">
			<Button variant="outline" size="icon-lg" onclick={() => map.zoomIn()}>
				<HugeiconsIcon icon={Add01Icon} strokeWidth={2} />
			</Button>
			<Button variant="outline" size="icon-lg" onclick={() => map.zoomOut()}>
				<HugeiconsIcon icon={Remove01Icon} strokeWidth={2} className="duotone-fill" />
			</Button>
		</ButtonGroup.Root>
	{/if}
	<ButtonGroup.Root>
		<Button
			variant="outline"
			size="icon-lg"
			class={cn(
				"transition-all duration-100 active:scale-95",
				navigationMode.enabled ? "text-chart-2" : "",
			)}
			onclick={locationButtonPress}
		>
			{#if geolocateState.state == "passive"}
				<HugeiconsIcon icon={Gps02Icon} strokeWidth={2} className="duotone-fill" />
			{:else}
				<HugeiconsIcon icon={Navigation04Icon} strokeWidth={2} className="duotone-fill" />
			{/if}
		</Button>
	</ButtonGroup.Root>
	<ButtonGroup.Root>
		<Button
			variant="outline"
			size="icon-lg"
			class="transition-all duration-100 active:scale-95"
			onclick={handleStyleChange}
		>
			<HugeiconsIcon icon={LayerIcon} strokeWidth={2} className="duotone-fill" />
		</Button>
	</ButtonGroup.Root>
	<ButtonGroup.Root>
		<Sidebar.Trigger class="size-12" variant="outline" />
	</ButtonGroup.Root>
</ButtonGroup.Root>
