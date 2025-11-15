<script lang="ts">
	import { Button } from "$lib/components/ui/button";
	import { animate } from "motion";
	import { getLightingPreset } from "./map-utils";
	import { mode } from "mode-watcher";
	import * as ButtonGroup from "$lib/components/ui/button-group/index";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { Add01Icon, Gps02Icon, LayerIcon, Remove01Icon } from "@hugeicons/core-free-icons";
	import { getLayoutContext } from "$lib/context/layout.svelte";
	import { geolocateControl } from "./map-state";

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
		const styles: Array<"streets" | "satellite"> = ["streets",  "satellite"];
		const currentIndex = styles.indexOf(mapStyle);
		mapStyle = styles[(currentIndex + 1) % styles.length];
		map.setStyle(styleMap[mapStyle], {
			config: {
				basemap: {
					lightPreset: getLightingPreset(mode.current || "light"),
				}
			},
			localFontFamily: undefined,
			localIdeographFontFamily: undefined
		});
	}

	function recenterMap() {
    geolocateControl.trigger();
	}

	function handleStyleChange() {
		cycleMapStyle();
	}
</script>

<ButtonGroup.Root orientation="vertical" class="absolute left-4 bottom-4 z-20 [&_button]:dark:bg-card [&_button]:dark:hover:bg-secondary">
  {#if layout.isDesktop}
    <ButtonGroup.Root orientation="vertical">
      <Button
        variant="outline"
        size="icon-lg"
        onclick={() => map.zoomIn()}
      >
        <HugeiconsIcon icon={Add01Icon} strokeWidth={2} />
      </Button>
      <Button
        variant="outline"
        size="icon-lg"
        onclick={() => map.zoomOut()}
      >
        <HugeiconsIcon icon={Remove01Icon} strokeWidth={2} />
      </Button>
    </ButtonGroup.Root>
  {/if}
  <ButtonGroup.Root>
    <Button
      variant="outline"
      size="icon-lg"
      onclick={recenterMap}
    >
      <HugeiconsIcon icon={Gps02Icon} strokeWidth={2} />
    </Button>
  </ButtonGroup.Root>
    <ButtonGroup.Root>
    <Button
      variant="outline"
      size="icon-lg"
      onclick={handleStyleChange}
    >
      <HugeiconsIcon icon={LayerIcon} strokeWidth={2} />
    </Button>
  </ButtonGroup.Root>
</ButtonGroup.Root>
