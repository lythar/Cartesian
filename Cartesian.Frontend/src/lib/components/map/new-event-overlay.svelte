<script lang="ts">
	import { cn, type LatLng } from "$lib/utils";
	import { animate } from "motion";
	import { newEventOverlayState } from "./map-state.svelte";
	import Button from "../ui/button/button.svelte";
	import Label from "../ui/label/label.svelte";

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
            if (overlayContainer) {
                animate(
                    overlayContainer,
                    {
                        opacity: [0, 1],
                        transform: ["scale(0.90)", "scale(1)"]
                    },
                    {
                        duration: 0.25,
                        ease: [.45, -.05, .15, 1.05]
                    }
                );
            }
        } else {
            if (overlayContainer) {
                animate(
                    overlayContainer,
                    {
                        opacity: [1, 0],
                        transform: ["scale(1)", "scale(0.90)"]
                    },
                    {
                        duration: 0.25,
                        ease: [.45, -.05, .15, 1.05]
                    }
                );
            }
        }
    });
</script>

<div
  bind:this={overlayContainer} class={cn("absolute top-0 right-4 z-50 pt-20 pb-4 origin-right w-140 h-full flex flex-col pointer-events-none", open ? "" : "")}>
    <div class={cn("flex-1 flex flex-col", open ? "pointer-events-auto" : "pointer-events-none")}>
        <div class="rounded-lg bg-background shadow-lg flex flex-col justify-between h-full">
            <div class="flex-1 p-6">
                <h2 class="text-2xl font-bold mb-4 tracking-tight">New Event</h2>
                <div>
                </div>
            </div>
            <div class="flex gap-2 items-center justify-end p-4 bg-card rounded-b-lg shadow-neu-highlight">
                <Button
                    class="self-end"
                    onclick={() => {
                        newEventOverlayState.open = false;
                    }}
                >Add Event</Button>
                <Button
                    variant="secondary"
                    class="self-end"
                    onclick={() => {
                        newEventOverlayState.open = false;
                    }}
                >Cancel</Button>
            </div>
        </div>
    </div>
</div>

