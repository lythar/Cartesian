<script lang="ts">
	import {
		createGetEventApiEventIdImages,
		createGetEventApiEventId,
		createGetEventApiEventIdFavorite,
		createPostEventApiEventIdFavorite,
		createDeleteEventApiEventIdFavorite,
		createGetAccountApiMe,
		createPostEventApiEventIdParticipate,
		createDeleteEventApiEventIdParticipate,
		createDeleteEventApiEventId,
	} from "$lib/api/cartesian-client";
	import { createReverseGeocodeQuery } from "$lib/api/queries/forward-geocode.query";
	import type { MapEventProperties } from "./map-state.svelte";
	import { editEventOverlayState } from "./map-state.svelte";
	import * as Carousel from "$lib/components/ui/carousel";
	import * as AlertDialog from "$lib/components/ui/alert-dialog";
	import LoginAlertDialog from "$lib/components/auth/login-alert-dialog.svelte";
	import * as Tooltip from "$lib/components/ui/tooltip";
	import { Button } from "$lib/components/ui/button";
	import {
		Cancel01Icon,
		Calendar03Icon,
		Clock01Icon,
		Location01Icon,
		Share01Icon,
		FavouriteIcon,
		PencilEdit01Icon,
		Delete02Icon,
		UserMultiple02Icon,
	} from "@hugeicons/core-free-icons";
	import { toast } from "svelte-sonner";
	import { page } from "$app/stores";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { Avatar, AvatarFallback, AvatarImage } from "$lib/components/ui/avatar";
	import { format } from "date-fns";
	import { getAvatarUrl, getMediaUrl, cn, getInitials } from "$lib/utils";
	import { openUserProfile } from "$lib/components/profile/profile-state.svelte";
	import { EVENT_TAG_CONFIG } from "$lib/constants/event-tags";
	import { m } from "$lib/paraglide/messages";
	import { invalidateAll } from "$app/navigation";
	import { useQueryClient } from "@tanstack/svelte-query";

	interface Props {
		event: MapEventProperties;
		onClose: () => void;
	}

	let { event, onClose }: Props = $props();

	const queryClient = useQueryClient();
	const imagesQuery = createGetEventApiEventIdImages(event.eventId);
	let images = $derived(imagesQuery.data || []);

	const eventDetailsQuery = createGetEventApiEventId(event.eventId);
	let eventDetails = $derived(eventDetailsQuery.data);
	let hasMultipleSchedules = $derived((eventDetails?.windows?.length ?? 0) > 1);

	let currentWindow = $derived(eventDetails?.windows?.find((w) => w.id === event.windowId));
	let eventLocation = $derived(
		eventDetails?.location as unknown as { coordinates: [number, number] } | undefined,
	);
	let longitude = $derived(eventLocation?.coordinates?.[0]);
	let latitude = $derived(eventLocation?.coordinates?.[1]);

	const reverseGeocodeQuery = createReverseGeocodeQuery(() =>
		longitude !== undefined && latitude !== undefined
			? { longitude: longitude!, latitude: latitude! }
			: undefined,
	);

	let address = $derived(
		reverseGeocodeQuery.data?.features?.[0]?.properties?.place_formatted ||
			reverseGeocodeQuery.data?.features?.[0]?.properties?.full_address ||
			"Unknown location",
	);

	let tags = $derived(typeof event.tags === "string" ? JSON.parse(event.tags) : event.tags || []);
	let displayTags = $derived(tags.slice(0, 6));
	let remainingTags = $derived(tags.length - 6);
	let showAllTags = $state(false);

	const meQuery = createGetAccountApiMe();
	let isParticipating = $derived(
		eventDetails?.participants?.some((p) => p.id === meQuery.data?.id) ?? false,
	);
	let isOwner = $derived(eventDetails?.author?.id === meQuery.data?.id);

	let participants = $derived(eventDetails?.participants ?? []);
	let displayParticipants = $derived(participants.slice(0, 5));
	let remainingParticipantCount = $derived(Math.max(0, participants.length - 5));

	const participateMutation = createPostEventApiEventIdParticipate();
	const unparticipateMutation = createDeleteEventApiEventIdParticipate();

	async function toggleParticipation() {
		if (!meQuery.data) {
			loginAlertDescription = "You need to sign up or log in to participate in an event.";
			loginAlertOpen = true;
			return;
		}
		if (isParticipating) {
			await unparticipateMutation.mutateAsync({ eventId: event.eventId });
		} else {
			await participateMutation.mutateAsync({ eventId: event.eventId });
		}
		await eventDetailsQuery.refetch();
	}

	function shareEvent() {
		const url = new URL($page.url);
		url.searchParams.set("event", event.eventId.toString());
		navigator.clipboard.writeText(url.toString());
		toast.success("Link copied to clipboard");
	}

	const favoriteQuery = createGetEventApiEventIdFavorite(event.eventId);
	let isFavorited = $derived(favoriteQuery.data ?? false);

	const favoriteMutation = createPostEventApiEventIdFavorite();
	const unfavoriteMutation = createDeleteEventApiEventIdFavorite();

	async function toggleFavorite() {
		if (!meQuery.data) {
			loginAlertDescription = "You need to sign up or log in to favorite an event.";
			loginAlertOpen = true;
			return;
		}
		if (isFavorited) {
			await unfavoriteMutation.mutateAsync({ eventId: event.eventId });
		} else {
			await favoriteMutation.mutateAsync({ eventId: event.eventId });
		}
		await favoriteQuery.refetch();
	}

	let deleteDialogOpen = $state(false);
	let loginAlertOpen = $state(false);
	let loginAlertDescription = $state("");
	const deleteEventMutation = createDeleteEventApiEventId();

	async function handleDeleteEvent() {
		try {
			await deleteEventMutation.mutateAsync({ eventId: event.eventId });
			toast.success("Event deleted successfully");
			await queryClient.invalidateQueries({ queryKey: ["getEventApiMy"] });
			await queryClient.invalidateQueries({ queryKey: ["getEventApiFavorites"] });
			await queryClient.invalidateQueries({ queryKey: ["getEventApiParticipating"] });
			await queryClient.invalidateQueries({ queryKey: ["getEventApiList"] });
			await queryClient.invalidateQueries({ queryKey: ["/event/api/geojson"] });
			deleteDialogOpen = false;
			onClose();
		} catch (e) {
			console.error("Failed to delete event", e);
			toast.error("Failed to delete event");
		}
	}

	function handleEditEvent() {
		if (!eventDetails) return;
		editEventOverlayState.open = true;
		editEventOverlayState.eventId = event.eventId;
		if (eventLocation) {
			editEventOverlayState.location = {
				lng: longitude!,
				lat: latitude!,
			};
		}
		onClose();
	}
</script>

<div class="relative h-48 w-full shrink-0 bg-muted">
	{#if images.length > 0}
		<Carousel.Root class="h-full w-full">
			<Carousel.Content>
				{#each images as image}
					<Carousel.Item>
						<div
							class="h-48 w-full bg-cover bg-center"
							style="background-image: url({getMediaUrl(image.id)})"
						></div>
					</Carousel.Item>
				{/each}
			</Carousel.Content>
			{#if images.length > 1}
				<Carousel.Previous class="left-2" />
				<Carousel.Next class="right-2" />
			{/if}
		</Carousel.Root>
	{:else}
		<div class="flex h-full w-full items-center justify-center text-muted-foreground">
			<HugeiconsIcon icon={Calendar03Icon} size={48} />
		</div>
	{/if}

	{#if isOwner}
		<div class="absolute top-4 left-4 flex gap-2">
			<Tooltip.Root>
				<Tooltip.Trigger>
					<Button
						variant="ghost"
						size="icon"
						class="h-8 w-8 rounded-full bg-background/50 backdrop-blur hover:bg-background/80"
						onclick={handleEditEvent}
					>
						<HugeiconsIcon icon={PencilEdit01Icon} size={16} strokeWidth={2} />
						<span class="sr-only">Edit event</span>
					</Button>
				</Tooltip.Trigger>
				<Tooltip.Content>Edit event</Tooltip.Content>
			</Tooltip.Root>

			<Tooltip.Root>
				<Tooltip.Trigger>
					<Button
						variant="ghost"
						size="icon"
						class="h-8 w-8 rounded-full bg-background/50 text-destructive backdrop-blur hover:bg-background/80 hover:text-destructive"
						onclick={() => (deleteDialogOpen = true)}
					>
						<HugeiconsIcon icon={Delete02Icon} size={16} strokeWidth={2} />
						<span class="sr-only">Delete event</span>
					</Button>
				</Tooltip.Trigger>
				<Tooltip.Content>Delete event</Tooltip.Content>
			</Tooltip.Root>
		</div>
	{/if}

	<Button
		variant="ghost"
		size="icon"
		class="absolute top-4 right-4 h-8 w-8 rounded-full bg-background/50 backdrop-blur hover:bg-background/80"
		onclick={onClose}
	>
		<HugeiconsIcon icon={Cancel01Icon} size={16} strokeWidth={2} />
		<span class="sr-only">Close</span>
	</Button>
</div>

<div
	class="scrollbar-thin scrollbar-thumb-border scrollbar-track-transparent flex-1 overflow-x-hidden overflow-y-auto p-4 sm:p-6"
>
	<div class="mb-6 space-y-4">
		<h2 class="text-xl font-bold tracking-tight break-words hyphens-auto sm:text-2xl">
			{event.eventName}
		</h2>

		{#if !hasMultipleSchedules}
			<div class="flex flex-col gap-2 rounded-lg bg-muted/50 p-3 text-sm">
				<div class="flex items-center gap-2.5 text-muted-foreground">
					<div class="shrink-0">
						<HugeiconsIcon icon={Calendar03Icon} size={18} />
					</div>
					<span class="font-medium text-foreground">
						{format(new Date(event.startTime), "EEEE, MMMM d, yyyy")}
					</span>
				</div>
				<div class="flex items-center gap-2.5 text-muted-foreground">
					<div class="shrink-0">
						<HugeiconsIcon icon={Clock01Icon} size={18} />
					</div>
					<span>
						{format(new Date(event.startTime), "h:mm a")} - {format(
							new Date(event.endTime),
							"h:mm a",
						)}
					</span>
				</div>
				{#if eventLocation}
					<div class="flex items-start gap-2.5 text-muted-foreground">
						<div class="mt-0.5 shrink-0">
							<HugeiconsIcon icon={Location01Icon} size={18} />
						</div>
						<div class="flex flex-col gap-0.5">
							<span class="leading-snug font-medium break-words text-foreground"
								>{address}</span
							>
							<span class="text-xs text-muted-foreground/80">
								{latitude?.toFixed(6)}, {longitude?.toFixed(6)}
							</span>
						</div>
					</div>
				{/if}
			</div>
		{/if}

		<div
			class="flex items-center justify-between gap-4 rounded-lg border bg-card p-3 shadow-sm"
		>
			<div class="flex items-center gap-3">
				<Avatar class="h-10 w-10 border">
					<AvatarImage
						src={event.communityId
							? getAvatarUrl({ id: event.communityAvatar! })
							: getAvatarUrl({ id: event.authorAvatar! })}
						alt={event.communityId ? event.communityName : event.authorName}
					/>
					<AvatarFallback>
						{(event.communityId ? event.communityName : event.authorName)
							?.substring(0, 2)
							.toUpperCase()}
					</AvatarFallback>
				</Avatar>
				<div class="flex flex-col">
					<span class="text-xs font-medium tracking-wider text-muted-foreground uppercase"
						>Hosted by</span
					>
					{#if event.communityId}
						<span class="font-semibold text-foreground">{event.communityName}</span>
					{:else}
						<button
							class="text-start font-semibold text-foreground hover:underline"
							onclick={() => openUserProfile(event.authorId)}
						>
							{event.authorName}
						</button>
					{/if}
				</div>
			</div>
		</div>

		<div class="flex flex-col gap-3 sm:flex-row sm:items-center">
			<Button
				variant={isParticipating ? "outline" : "default"}
				class="h-14 w-full flex-1 text-base font-semibold sm:h-10 sm:w-auto sm:text-sm"
				onclick={toggleParticipation}
				disabled={participateMutation.isPending || unparticipateMutation.isPending}
			>
				{isParticipating ? "Don't participate" : "Participate"}
			</Button>

			<div class="flex gap-2">
				<Tooltip.Root>
					<Tooltip.Trigger class="flex-1 sm:flex-none">
						<Button
							variant="outline"
							class="h-12 w-full px-4 text-base sm:h-10 sm:w-auto sm:px-3 sm:text-sm"
							onclick={shareEvent}
						>
							<HugeiconsIcon icon={Share01Icon} size={20} strokeWidth={1.5} />
							<span class="ml-2 sm:hidden">Share</span>
						</Button>
					</Tooltip.Trigger>
					<Tooltip.Content>Share event</Tooltip.Content>
				</Tooltip.Root>

				<Tooltip.Root>
					<Tooltip.Trigger class="flex-1 sm:flex-none">
						<Button
							variant="outline"
							class="h-12 w-full px-4 text-base sm:h-10 sm:w-auto sm:px-3 sm:text-sm"
							onclick={toggleFavorite}
							disabled={favoriteMutation.isPending || unfavoriteMutation.isPending}
						>
							<div class={isFavorited ? "fill-red-500 text-red-500" : ""}>
								<HugeiconsIcon icon={FavouriteIcon} size={20} strokeWidth={1.5} />
							</div>
							<span class="ml-2 sm:hidden"
								>{isFavorited ? "Unfavorite" : "Favorite"}</span
							>
						</Button>
					</Tooltip.Trigger>
					<Tooltip.Content>{isFavorited ? "Unfavorite" : "Favorite"}</Tooltip.Content>
				</Tooltip.Root>
			</div>
		</div>

		<!-- Participants -->
		{#if participants.length > 0}
			<div class="flex items-center justify-between rounded-lg border border-dashed p-3">
				<div class="flex items-center gap-3">
					<div class="flex -space-x-3">
						{#each displayParticipants as participant}
							<Avatar class="h-8 w-8 border-2 border-background ring-1 ring-muted">
								<AvatarImage
									src={getAvatarUrl(participant.avatar)}
									alt={participant.name}
								/>
								<AvatarFallback class="text-[10px]"
									>{getInitials(participant.name)}</AvatarFallback
								>
							</Avatar>
						{/each}
						{#if remainingParticipantCount > 0}
							<div
								class="flex h-8 w-8 items-center justify-center rounded-full border-2 border-background bg-muted text-[10px] font-medium text-muted-foreground ring-1 ring-muted"
							>
								+{remainingParticipantCount}
							</div>
						{/if}
					</div>
					<div class="flex flex-col">
						<span class="text-sm font-medium text-foreground">
							{participants.length}
							{participants.length === 1 ? "person" : "people"}
						</span>
						<span class="text-xs text-muted-foreground">going</span>
					</div>
				</div>
			</div>
		{/if}
	</div>

	<!-- Tags -->
	{#if tags.length > 0}
		<div class="mb-8 flex flex-wrap gap-2">
			{#each showAllTags ? tags : displayTags as tag}
				{@const config = EVENT_TAG_CONFIG[tag as keyof typeof EVENT_TAG_CONFIG]}
				<span
					class={cn(
						"flex items-center gap-1.5 rounded-full border px-3 py-1 text-xs font-medium transition-colors",
						"border-transparent bg-secondary text-secondary-foreground hover:bg-secondary/80",
					)}
				>
					{#if config?.icon}
						<span class="flex items-center opacity-70">
							<HugeiconsIcon icon={config.icon} size={14} strokeWidth={2} />
						</span>
					{/if}
					{config ? m[config.translationKey as keyof typeof m]() : tag}
				</span>
			{/each}
			{#if remainingTags > 0 && !showAllTags}
				<button
					class="rounded-full bg-muted px-3 py-1 text-xs font-medium text-muted-foreground transition-colors hover:bg-muted/80"
					onclick={() => (showAllTags = true)}
				>
					+{remainingTags} more
				</button>
			{/if}
		</div>
	{/if}

	<!-- Description -->
	{#if eventDetails?.description}
		<div class="mb-8">
			<h3 class="mb-3 text-sm font-semibold tracking-wider text-muted-foreground uppercase">
				About
			</h3>
			<div class="prose prose-sm prose-muted max-w-none text-foreground/90">
				<p class="leading-relaxed whitespace-pre-wrap">
					{eventDetails.description}
				</p>
			</div>
		</div>
	{/if}

	<!-- Timeline -->
	{#if hasMultipleSchedules && eventDetails?.windows}
		<div>
			<h3 class="mb-4 text-sm font-semibold tracking-wider text-muted-foreground uppercase">
				Schedule
			</h3>
			<div class="relative ml-2 space-y-6 pl-4">
				{#each eventDetails.windows as window, i}
					<div class="relative">
						<div
							class="absolute top-1 -left-[21px] h-2.5 w-2.5 rounded-full {window.id ===
							event.windowId
								? 'bg-primary'
								: 'bg-muted-foreground'} ring-4 ring-background"
						></div>
						{#if i !== eventDetails.windows.length - 1}
							<div
								class="absolute top-5 -left-[calc(var(--spacing)*4.3)] h-[calc(100%+1rem)] w-0.5 rounded-full bg-border/80"
							></div>
						{/if}
						<div
							class="rounded-lg border bg-card p-3 shadow-sm {window.id ===
							event.windowId
								? 'border-primary'
								: ''}"
						>
							<h4 class="font-medium">{window.title || "Event Occurrence"}</h4>
							{#if window.description}
								<p class="mt-1 text-xs text-muted-foreground">
									{window.description}
								</p>
							{/if}
							<div class="mt-2 flex items-center gap-3 text-xs text-muted-foreground">
								<div class="flex items-center gap-1">
									<HugeiconsIcon icon={Calendar03Icon} size={14} />
									<span
										>{format(
											new Date(window.startTime as string),
											"MMM d, yyyy",
										)}</span
									>
								</div>
								<div class="flex items-center gap-1">
									<HugeiconsIcon icon={Clock01Icon} size={14} />
									<span
										>{format(new Date(window.startTime as string), "h:mm a")} - {format(
											new Date(window.endTime as string),
											"h:mm a",
										)}</span
									>
								</div>
							</div>
						</div>
					</div>
				{/each}
			</div>
		</div>
	{/if}
</div>

<LoginAlertDialog bind:open={loginAlertOpen} description={loginAlertDescription} />

<AlertDialog.Root bind:open={deleteDialogOpen}>
	<AlertDialog.Content>
		<AlertDialog.Header>
			<AlertDialog.Title>Delete Event</AlertDialog.Title>
			<AlertDialog.Description>
				Are you sure you want to delete "{event.eventName}"? This action cannot be undone.
				All participants and favorites will be removed.
			</AlertDialog.Description>
		</AlertDialog.Header>
		<AlertDialog.Footer>
			<AlertDialog.Cancel>Cancel</AlertDialog.Cancel>
			<AlertDialog.Action
				onclick={handleDeleteEvent}
				class="bg-destructive text-destructive-foreground hover:bg-destructive/90"
			>
				{deleteEventMutation.isPending ? "Deleting..." : "Delete"}
			</AlertDialog.Action>
		</AlertDialog.Footer>
	</AlertDialog.Content>
</AlertDialog.Root>
