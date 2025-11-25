<script lang="ts">
	import * as Drawer from "$lib/components/ui/drawer";
	import { mapInteractionState } from "./map-state.svelte";
	import EventPreview from "./event-preview.svelte";
	import { page } from "$app/stores";
	import { pushState } from "$app/navigation";
	import { browser } from "$app/environment";
	import { buttonVariants } from "$lib/components/ui/button";
	import { Button } from "$lib/components/ui/button";
	import { newEventOverlayState } from "./map-state.svelte";
	import { cn } from "$lib/utils";

	let open = $state(false);
	let previousDrawerState = $state(!!$page.state.previewDrawer);

	$effect(() => {
		open = mapInteractionState.previewEventOpen;
	});

	function onOpenChange(isOpen: boolean) {
		if (!isOpen && mapInteractionState.previewEventOpen) {
			mapInteractionState.previewEventOpen = false;
			mapInteractionState.previewEvent = null;

			if (browser && $page.state.previewDrawer) {
				history.back();
			}
		}
	}

	function openDetails() {
		if (mapInteractionState.previewEvent) {
			mapInteractionState.selectedEvent = mapInteractionState.previewEvent;
			mapInteractionState.eventDetailsOpen = true;
			newEventOverlayState.open = false;

			// Close the drawer immediately
			mapInteractionState.previewEventOpen = false;
			mapInteractionState.previewEvent = null;

			// Pop history state if it exists to keep history clean
			if (browser && $page.state.previewDrawer) {
				history.back();
			}
		}
	}

	$effect(() => {
		const currentDrawerState = !!$page.state.previewDrawer;

		if (mapInteractionState.previewEventOpen) {
			if (!currentDrawerState) {
				if (previousDrawerState) {
					// State was present, now gone -> Back button pressed
					mapInteractionState.previewEventOpen = false;
					mapInteractionState.previewEvent = null;
				} else {
					// State wasn't present, still isn't -> Initial open
					if (browser) {
						pushState("", { previewDrawer: true });
					}
				}
			}
		}

		previousDrawerState = currentDrawerState;
	});
</script>

<Drawer.Root bind:open {onOpenChange}>
	<Drawer.Content hideHandle>
		<div class="relative mx-auto w-full pb-8">
			<div
				class="absolute left-1/2 top-3 z-10 h-1.5 w-[100px] -translate-x-1/2 rounded-full bg-white/50 backdrop-blur-sm"
			></div>
			{#if mapInteractionState.previewEvent}
				<EventPreview
					event={mapInteractionState.previewEvent}
					hideCloseButton
					hideViewButton
					class="w-full rounded-b-none rounded-t-lg shadow-none"
				/>
			{/if}
			<div class="px-0 lg:px-4">
				<Drawer.Footer class="pt-0">
					<Button onclick={openDetails} class="w-full h-10">View Details</Button>
					<Drawer.Close class={cn(buttonVariants({ variant: "outline" }), "h-10")}>Close</Drawer.Close>
				</Drawer.Footer>
			</div>
		</div>
	</Drawer.Content>
</Drawer.Root>
