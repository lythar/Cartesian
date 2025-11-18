<script lang="ts">
	import type { LatLng } from "$lib/utils";
	import { animate } from "motion";
	import { newEventOverlayState } from "./map-state.svelte";
	import Button from "../ui/button/button.svelte";

    interface Props {
        map: mapboxgl.Map;
    }

    let { map }: Props = $props();

    let overlayContainer = $state<HTMLDivElement | null>(null);

    let open = $state<boolean>(false);
    let draftLocation: LatLng | null = $state<LatLng | null>(null);

    $effect(() => {
       console.log("hi");
        open = newEventOverlayState.open;
        // if (open) {
        //     draftLocation = newEventOverlayState.location;
        //     if (overlayContainer)
        //       animate(overlayContainer,
        //         { scale: [0.85, 1], opacity: [0, 1], filter: ["blur(6px)", "blur(0px)"] },
        //         { duration: 0.35, ease: [0.32, 0.72, 0, 1] }
        //       );
        // } else {
        //     if (overlayContainer) animate(overlayContainer, { scale: [1, 0.30], opacity: 0, x: 50 }, { duration: 0.2 }).then(() => {
        //         draftLocation = null;
        //     });
        // }
    });
</script>

<div
  class:open={open}
  bind:this={overlayContainer} class="overlay-container absolute top-0 right-4 pb-4 pt-20 z-50 origin-bottom-right w-2/7 h-full">
    {#if open}
        <div class="h-full w-full rounded-2xl bg-background shadow-lg p-6 flex flex-col">
            <h2 class="text-2xl font-semibold mb-4">New Event</h2>
            <Button
              class="self-end mb-4"
              onclick={() => {
                  newEventOverlayState.open = false;
              }}
            >Close</Button>
      </div>
    {/if}
</div>


<style>
    .overlay-container {
        opacity: 0;
        transform: translateY(10%) scale(0.25) scaleY(0.1);
        transition: transform 0.35s cubic-bezier(.45, -.05, .15, 1.05),
                    opacity 0.35s cubic-bezier(.45, -.05, .15, 1.05),
                    filter 0.35s cubic-bezier(.45, -.05, .15, 1.05);
        filter: blur(8px);
    }

    .overlay-container.open {
        opacity: 1;
        transform: translateY(0) scale(1) scaleY(1);
        filter: blur(0);
    }
</style>
