<script lang="ts">
	import { Avatar, AvatarFallback, AvatarImage } from "$lib/components/ui/avatar";
	import { Button } from "$lib/components/ui/button";
	import {
		mapInteractionState,
		type MapEventProperties,
		newEventOverlayState,
	} from "./map-state.svelte";
	import { cn, getAvatarUrl, getInitials, getMediaUrl } from "$lib/utils";
	import { Calendar03Icon, Cancel01Icon, Clock01Icon } from "@hugeicons/core-free-icons";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { format } from "date-fns";
	import { createGetEventApiEventIdImages } from "$lib/api";

	interface Props {
		event: MapEventProperties;
		onclose?: () => void;
		class?: string;
		hideCloseButton?: boolean;
		hideViewButton?: boolean;
	}

	let {
		event,
		onclose,
		class: className,
		hideCloseButton = false,
		hideViewButton = false,
	}: Props = $props();

	function openDetails() {
		mapInteractionState.selectedEvent = event;
		mapInteractionState.eventDetailsOpen = true;
		newEventOverlayState.open = false;
	}

	let tags = $derived(typeof event.tags === "string" ? JSON.parse(event.tags) : event.tags || []);

	const imagesQuery = createGetEventApiEventIdImages(event.eventId);
	let images = $derived(imagesQuery.data || []);

	let displayTags = $derived(tags.slice(0, 3));
	let remainingTags = $derived(tags.length - 3);
</script>

<div class={cn("relative overflow-hidden rounded-2xl bg-background", className)}>
	{#if !hideCloseButton}
		<Button
			variant="ghost"
			size="icon"
			class="absolute right-2 top-2 z-10 h-8 w-8 rounded-full bg-black/40 text-white hover:bg-black/60"
			onclick={() => {
				onclose?.();
			}}
		>
			<HugeiconsIcon icon={Cancel01Icon} size={16} />
		</Button>
	{/if}
	<div class="flex h-32 w-full items-center justify-center bg-muted">
		{#if images.length > 0}
			<div
				class="h-32 w-full bg-cover bg-center"
				style="background-image: url({getMediaUrl(images[0].id)})"
			></div>
		{:else}
			<HugeiconsIcon icon={Calendar03Icon} size={48} className="duotone-fill" />
		{/if}
	</div>
	<div class="p-4">
		<div class="mb-2 flex flex-wrap gap-1">
			{#each displayTags as tag}
				<span
					class="rounded-full bg-secondary px-2 py-0.5 text-[10px] font-medium text-secondary-foreground"
				>
					{tag}
				</span>
			{/each}
			{#if remainingTags > 0}
				<span
					class="rounded-full bg-muted px-2 py-0.5 text-[10px] font-medium text-muted-foreground"
				>
					+{remainingTags}
				</span>
			{/if}
		</div>

		<h3 class="mb-1 text-lg leading-tight font-semibold">{event.eventName}</h3>
		<p class="mb-3 line-clamp-2 text-xs text-muted-foreground">{event.eventDescription}</p>

		<div class="flex items-center justify-between">
			<div class="flex items-center gap-2">
				<Avatar class="h-6 w-6">
					<AvatarImage
						src={event.communityId
							? getAvatarUrl({ id: event.communityAvatar! })
							: getAvatarUrl({ id: event.authorAvatar })}
						alt={event.communityId ? event.communityName : event.authorName}
					/>
					<AvatarFallback
						>{getInitials(
							(event.communityId ? event.communityName : event.authorName) || "",
						)}</AvatarFallback
					>
				</Avatar>
				<span class="text-xs font-medium text-muted-foreground">
					{event.communityId ? event.communityName : event.authorName}
				</span>
			</div>
			{#if !hideViewButton}
				<Button variant="ghost" size="sm" class="h-7 text-xs" onclick={openDetails}>
					View
				</Button>
			{/if}
		</div>
	</div>
</div>
