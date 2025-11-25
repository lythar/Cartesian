<script lang="ts">
	import * as Drawer from "$lib/components/ui/drawer";
	import { mapInteractionState } from "./map-state.svelte";
	import EventPreview from "./event-preview.svelte";
	import { page } from "$app/stores";
	import { pushState } from "$app/navigation";
	import { browser } from "$app/environment";
	import { buttonVariants } from "$lib/components/ui/button";

	let open = $state(false);

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

	$effect(() => {
		if (mapInteractionState.previewEventOpen && !$page.state.previewDrawer) {
			if (browser) {
				pushState("", { previewDrawer: true });
			}
		}
	});

	$effect(() => {
		if (!$page.state.previewDrawer && mapInteractionState.previewEventOpen) {
			mapInteractionState.previewEventOpen = false;
			mapInteractionState.previewEvent = null;
		}
	});
</script>

<Drawer.Root bind:open {onOpenChange}>
	<Drawer.Content>
		<div class="mx-auto w-full max-w-sm p-4 pb-8">
			{#if mapInteractionState.previewEvent}
				<EventPreview
					event={mapInteractionState.previewEvent}
					hideCloseButton
					class="w-full shadow-none"
				/>
			{/if}
			<Drawer.Footer class="pt-2">
				<Drawer.Close class={buttonVariants({ variant: "outline" })}>Close</Drawer.Close>
			</Drawer.Footer>
		</div>
	</Drawer.Content>
</Drawer.Root>
