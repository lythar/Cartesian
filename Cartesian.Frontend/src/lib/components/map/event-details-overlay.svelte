<script lang="ts">
	import { mapInteractionState } from "./map-state.svelte";
	import { cn } from "$lib/utils";
	import EventDetailsContent from "./event-details-content.svelte";

	let open = $derived(mapInteractionState.eventDetailsOpen);
	let event = $derived(mapInteractionState.selectedEvent);

	function close() {
		mapInteractionState.eventDetailsOpen = false;
	}
</script>

<div
	class={cn(
		"pointer-events-none absolute inset-0 z-50 flex w-full max-w-full flex-col overflow-hidden transition-all duration-300 ease-in-out lg:inset-auto lg:top-4 lg:bottom-4 lg:left-4 lg:max-w-lg",
		open ? "translate-x-0 opacity-100" : "-translate-x-full opacity-0",
	)}
>
	<div
		class={cn(
			"flex h-full flex-col overflow-hidden border border-border/40 bg-background shadow-2xl transition-all lg:rounded-3xl",
			open ? "pointer-events-auto" : "pointer-events-none",
		)}
	>
		{#if event}
			{#key event.eventId}
				<EventDetailsContent {event} onClose={close} />
			{/key}
		{/if}
	</div>
</div>
