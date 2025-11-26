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
	import * as m from "$lib/paraglide/messages";

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

			mapInteractionState.previewEventOpen = false;
			mapInteractionState.previewEvent = null;

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
					mapInteractionState.previewEventOpen = false;
					mapInteractionState.previewEvent = null;
				} else {
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
				class="absolute top-3 left-1/2 z-10 h-1.5 w-[100px] -translate-x-1/2 rounded-full bg-white/50 backdrop-blur-sm"
			></div>
			{#if mapInteractionState.previewEvent}
				<EventPreview
					event={mapInteractionState.previewEvent}
					hideCloseButton
					hideViewButton
					class="w-full rounded-t-lg rounded-b-none shadow-none"
				/>
			{/if}
			<div class="px-0 lg:px-4">
				<Drawer.Footer class="pt-0">
					<Button onclick={openDetails} class="h-10 w-full">{m.view_details()}</Button>
					<Drawer.Close class={cn(buttonVariants({ variant: "outline" }), "h-10")}
						>{m.close()}</Drawer.Close
					>
				</Drawer.Footer>
			</div>
		</div>
	</Drawer.Content>
</Drawer.Root>
