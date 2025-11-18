<script lang="ts">
	import type { LatLng } from "$lib/utils";
	import { animate } from "motion";
	import { newEventOverlayState } from "./map-state.svelte";

    interface Props {
        map: mapboxgl.Map;
    }

    let { map }: Props = $props();

    let overlayContainer = $state<HTMLDivElement | null>(null);

    let open = $state<boolean>(false);
    let draftLocation: LatLng | null = $state<LatLng | null>(null);

    $effect(() => {
        open = newEventOverlayState.open;
        if (open) {
            draftLocation = newEventOverlayState.location;
            if (overlayContainer) animate(overlayContainer, { scale: 1, opacity: 1 }, { duration: 0.3, type: "spring", stiffness: 200 });
        } else {
            if (overlayContainer) animate(overlayContainer, { scale: 0.75, opacity: 0 }, { duration: 0.2 }).then(() => {
                draftLocation = null;
            });
        }
    });
</script>

<div bind:this={overlayContainer} class="absolute top-0 right-0 p-4 z-50 origin-right w-96 h-full scale-75 opacity-0">
    {#if open}
        <div class="h-full w-full rounded-2xl bg-background shadow-lg p-6 flex flex-col">
            <h2 class="text-2xl font-semibold mb-4">New Event</h2>
    </div>
    {/if}
</div>