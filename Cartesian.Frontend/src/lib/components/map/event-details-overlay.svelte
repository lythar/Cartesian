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
		"pointer-events-none absolute left-4 top-4 bottom-4 z-50 flex w-full max-w-lg flex-col transition-all duration-300 ease-in-out",
		open ? "translate-x-0 opacity-100" : "-translate-x-full opacity-0"
	)}
>
	<div
		class={cn(
			"flex h-full flex-col overflow-hidden rounded-3xl border border-border/40 bg-background shadow-2xl transition-all",
			open ? "pointer-events-auto" : "pointer-events-none"
		)}
	>
    {#if event}
      {#key event.eventId}
        <EventDetailsContent {event} onClose={close} />
      {/key}
    {/if}
	</div>
</div>
