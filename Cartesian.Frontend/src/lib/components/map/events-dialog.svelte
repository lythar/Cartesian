<script lang="ts">
	import * as Dialog from "$lib/components/ui/dialog";
	import * as Tabs from "$lib/components/ui/tabs";
	import * as Avatar from "$lib/components/ui/avatar";
	import * as Empty from "$lib/components/ui/empty";
	import { Badge } from "$lib/components/ui/badge";
	import { Button } from "$lib/components/ui/button";
	import { Input } from "$lib/components/ui/input";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import {
		Cancel01Icon,
		Calendar03Icon,
		Bookmark02Icon,
		UserIcon,
		CheckmarkCircle02Icon,
		Search01Icon,
	} from "@hugeicons/core-free-icons";
	import {
		createGetEventApiFavorites,
		createGetEventApiMy,
		createGetEventApiParticipating,
	} from "$lib/api";
	import type { EventDto, EventWindowDto } from "$lib/api";
	import { mapInteractionState } from "./map-state.svelte";
	import { format, formatDistanceToNow, isBefore, addWeeks } from "date-fns";
	import { getInitials, getAvatarUrl } from "$lib/utils";

	let { open = $bindable(false) } = $props();

	const favoritesQuery = createGetEventApiFavorites();
	const myEventsQuery = createGetEventApiMy();
	const participatingQuery = createGetEventApiParticipating();

	let searchQuery = $state("");

	function getNextWindow(event: EventDto): EventWindowDto | undefined {
		if (!event.windows || event.windows.length === 0) return undefined;

		const now = new Date();
		const sortedWindows = [...event.windows].sort((a, b) => {
			const dateA = new Date(a.startTime as string);
			const dateB = new Date(b.startTime as string);
			return dateA.getTime() - dateB.getTime();
		});

		const nextWindow = sortedWindows.find((w) => new Date(w.startTime as string) > now);
		return nextWindow || sortedWindows[sortedWindows.length - 1];
	}

	function processEvents(events: EventDto[] | undefined, query: string): EventDto[] {
		if (!events) return [];

		let processed = [...events];

		if (query) {
			const lowerQuery = query.toLowerCase();
			processed = processed.filter(
				(e) =>
					e.name.toLowerCase().includes(lowerQuery) ||
					e.description.toLowerCase().includes(lowerQuery),
			);
		}

		processed.sort((a, b) => {
			const windowA = getNextWindow(a);
			const windowB = getNextWindow(b);

			if (!windowA && !windowB) return 0;
			if (!windowA) return 1;
			if (!windowB) return -1;

			const dateA = new Date(windowA.startTime as string);
			const dateB = new Date(windowB.startTime as string);

			return dateA.getTime() - dateB.getTime();
		});

		return processed;
	}

	function getWindowDate(event: EventDto): Date | null {
		const window = getNextWindow(event);
		return window ? new Date(window.startTime as string) : null;
	}

	function isUpcomingSoon(event: EventDto): boolean {
		const date = getWindowDate(event);
		if (!date) return false;
		const now = new Date();
		const oneWeekFromNow = addWeeks(now, 1);
		return isBefore(date, oneWeekFromNow) && !isBefore(date, now);
	}

	function formatEventTimeRelative(event: EventDto): string {
		const date = getWindowDate(event);
		if (!date) return "";
		// formatDistanceToNow returns "3 days", addSuffix adds "in" or "ago"
		return formatDistanceToNow(date, { addSuffix: true });
	}

	function formatEventTimeAbsolute(event: EventDto): string {
		const date = getWindowDate(event);
		if (!date) return "No upcoming dates";
		return format(date, "MMM d, h:mm a");
	}

	const processedFavorites = $derived(processEvents(favoritesQuery.data, searchQuery));
	const processedMyEvents = $derived(processEvents(myEventsQuery.data, searchQuery));
	const processedParticipating = $derived(processEvents(participatingQuery.data, searchQuery));

	function handleEventClick(event: EventDto) {
		const window = getNextWindow(event) ?? event.windows?.[0];

		mapInteractionState.selectedEvent = {
			eventId: event.id,
			eventName: event.name,
			eventDescription: event.description,
			windowId: window?.id ?? "",
			windowTitle: window?.title ?? "",
			windowDescription: window?.description ?? "",
			authorId: event.author.id,
			authorName: event.author.name,
			authorAvatar: event.author.avatar?.id ?? "",
			communityId: event.community?.id ?? null,
			communityName: event.community?.name ?? null,
			communityAvatar: event.community?.avatar?.id ?? null,
			visibility: event.visibility,
			timing: event.timing,
			tags: event.tags,
			startTime: (window?.startTime as string) ?? "",
			endTime: (window?.endTime as string) ?? "",
			createdAt: "",
		};

		mapInteractionState.eventDetailsOpen = true;
		open = false;
	}
</script>

{#snippet eventCard(event: EventDto)}
	<button
		class="group flex w-full items-start gap-4 rounded-2xl border border-border/40 bg-muted/20 p-4 text-left transition-all hover:border-primary/20 hover:bg-muted/40"
		onclick={() => handleEventClick(event)}
	>
		<!-- Avatar Image -->
		<Avatar.Root class="size-12 shrink-0 rounded-xl border border-border/50 shadow-sm">
			<Avatar.Image
				src={getAvatarUrl(event.community?.avatar ?? event.author.avatar)}
				alt={event.community?.name ?? event.author.name}
				class="object-cover"
			/>
			<Avatar.Fallback class="rounded-xl bg-background text-sm font-semibold text-primary/80">
				{getInitials(event.community?.name ?? event.author.name)}
			</Avatar.Fallback>
		</Avatar.Root>

		<div class="flex min-w-0 flex-1 flex-col gap-1">
			<!-- Title & Time Badge -->
			<div class="flex items-start justify-between gap-2">
				<h4 class="truncate pr-2 text-base leading-tight font-semibold">{event.name}</h4>
				{#if isUpcomingSoon(event)}
					<Badge
						variant="secondary"
						class="h-5 shrink-0 border-primary/20 bg-primary/10 px-1.5 text-[10px] font-medium whitespace-nowrap text-primary hover:bg-primary/20"
					>
						{formatEventTimeRelative(event)}
					</Badge>
				{/if}
			</div>

			<!-- Meta Info -->
			<div
				class="mt-0.5 flex flex-wrap items-center gap-x-3 gap-y-1 text-xs text-muted-foreground/80"
			>
				<!-- Date -->
				<div class="flex items-center gap-1">
					<HugeiconsIcon icon={Calendar03Icon} className="size-3.5" />
					<span>{formatEventTimeAbsolute(event)}</span>
				</div>
				<!-- Host -->
				<div class="flex max-w-[150px] items-center gap-1 truncate">
					<HugeiconsIcon
						icon={event.community ? UserIcon : UserIcon}
						className="size-3.5"
					/>
					<span class="truncate">
						{event.community ? event.community.name : event.author.name}
					</span>
				</div>
			</div>

			<!-- Description -->
			{#if event.description}
				<p class="mt-1.5 line-clamp-2 text-sm leading-relaxed text-muted-foreground">
					{event.description}
				</p>
			{/if}

			<!-- Tags -->
			{#if event.tags && event.tags.length > 0}
				<div class="mt-2 flex flex-wrap gap-1.5">
					{#each event.tags.slice(0, 3) as tag}
						<span
							class="inline-flex items-center rounded-md bg-muted/50 px-1.5 py-0.5 text-[10px] font-medium text-muted-foreground ring-1 ring-border/50 ring-inset"
						>
							{tag}
						</span>
					{/each}
					{#if event.tags.length > 3}
						<span
							class="inline-flex items-center rounded-md bg-muted/50 px-1.5 py-0.5 text-[10px] font-medium text-muted-foreground ring-1 ring-border/50 ring-inset"
						>
							+{event.tags.length - 3}
						</span>
					{/if}
				</div>
			{/if}
		</div>
	</button>
{/snippet}

<Dialog.Root bind:open>
	<Dialog.Content
		class="flex h-[600px] max-w-[500px] flex-col gap-0 overflow-hidden rounded-3xl border border-border/40 bg-background/95 p-0 shadow-2xl backdrop-blur-xl md:max-w-[600px]"
		showCloseButton={false}
	>
		<div class="flex flex-none flex-col gap-4 border-b border-border/10 px-6 py-4">
			<div class="flex items-center justify-between">
				<div class="flex items-center gap-3">
					<div class="rounded-full bg-primary/10 p-2 text-primary">
						<HugeiconsIcon icon={Calendar03Icon} className="size-5" />
					</div>
					<div>
						<Dialog.Title class="text-xl font-semibold tracking-tight"
							>Events</Dialog.Title
						>
						<Dialog.Description class="text-xs font-medium text-muted-foreground">
							Manage your events and schedule
						</Dialog.Description>
					</div>
				</div>
				<Button
					variant="ghost"
					size="icon"
					class="h-8 w-8 rounded-full transition-colors hover:bg-muted"
					onclick={() => (open = false)}
				>
					<HugeiconsIcon icon={Cancel01Icon} className="size-4" strokeWidth={2} />
				</Button>
			</div>

			<div class="relative">
				<HugeiconsIcon
					icon={Search01Icon}
					className="absolute left-3 top-2.5 size-4 text-muted-foreground"
				/>
				<Input
					placeholder="Search events..."
					class="border-border/40 bg-muted/20 pl-9"
					bind:value={searchQuery}
				/>
			</div>
		</div>

		<Tabs.Root value="favorites" class="flex flex-1 flex-col overflow-hidden">
			<div class="flex-none px-6 pt-4">
				<Tabs.List class="flex w-full items-center rounded-xl bg-muted/30 p-1">
					<Tabs.Trigger
						value="favorites"
						class="flex-1 gap-2 rounded-lg data-[state=active]:bg-background data-[state=active]:text-foreground data-[state=active]:shadow-sm"
					>
						<HugeiconsIcon icon={Bookmark02Icon} className="size-4" />
						Favorited
					</Tabs.Trigger>
					<Tabs.Trigger
						value="created"
						class="flex-1 gap-2 rounded-lg data-[state=active]:bg-background data-[state=active]:text-foreground data-[state=active]:shadow-sm"
					>
						<HugeiconsIcon icon={UserIcon} className="size-4" />
						Created
					</Tabs.Trigger>
					<Tabs.Trigger
						value="attending"
						class="flex-1 gap-2 rounded-lg data-[state=active]:bg-background data-[state=active]:text-foreground data-[state=active]:shadow-sm"
					>
						<HugeiconsIcon icon={CheckmarkCircle02Icon} className="size-4" />
						Attending
					</Tabs.Trigger>
				</Tabs.List>
			</div>

			<div class="flex-1 overflow-y-auto p-6">
				<Tabs.Content value="favorites" class="mt-0 h-full">
					{#if favoritesQuery.isLoading}
						<div class="flex h-full items-center justify-center">
							<div
								class="h-8 w-8 animate-spin rounded-full border-b-2 border-primary"
							></div>
						</div>
					{:else if processedFavorites.length === 0}
						<Empty.Root class="h-full border-0">
							<Empty.Media>
								<HugeiconsIcon
									icon={Bookmark02Icon}
									className="size-10 text-muted-foreground/50"
								/>
							</Empty.Media>
							<div class="space-y-1">
								<Empty.Title>No favorited events</Empty.Title>
								<Empty.Description>
									{searchQuery
										? "No events match your search query"
										: "Events you favorite will appear here"}
								</Empty.Description>
							</div>
						</Empty.Root>
					{:else}
						<div class="space-y-3">
							{#each processedFavorites as event}
								{@render eventCard(event)}
							{/each}
						</div>
					{/if}
				</Tabs.Content>

				<Tabs.Content value="created" class="mt-0 h-full">
					{#if myEventsQuery.isLoading}
						<div class="flex h-full items-center justify-center">
							<div
								class="h-8 w-8 animate-spin rounded-full border-b-2 border-primary"
							></div>
						</div>
					{:else if processedMyEvents.length === 0}
						<Empty.Root class="h-full border-0">
							<Empty.Media>
								<HugeiconsIcon
									icon={UserIcon}
									className="size-10 text-muted-foreground/50"
								/>
							</Empty.Media>
							<div class="space-y-1">
								<Empty.Title>No created events</Empty.Title>
								<Empty.Description>
									{searchQuery
										? "No events match your search query"
										: "Events you create will appear here"}
								</Empty.Description>
							</div>
						</Empty.Root>
					{:else}
						<div class="space-y-3">
							{#each processedMyEvents as event}
								{@render eventCard(event)}
							{/each}
						</div>
					{/if}
				</Tabs.Content>

				<Tabs.Content value="attending" class="mt-0 h-full">
					{#if participatingQuery.isLoading}
						<div class="flex h-full items-center justify-center">
							<div
								class="h-8 w-8 animate-spin rounded-full border-b-2 border-primary"
							></div>
						</div>
					{:else if processedParticipating.length === 0}
						<Empty.Root class="h-full border-0">
							<Empty.Media>
								<HugeiconsIcon
									icon={CheckmarkCircle02Icon}
									className="size-10 text-muted-foreground/50"
								/>
							</Empty.Media>
							<div class="space-y-1">
								<Empty.Title>No attending events</Empty.Title>
								<Empty.Description>
									{searchQuery
										? "No events match your search query"
										: "Events you attend will appear here"}
								</Empty.Description>
							</div>
						</Empty.Root>
					{:else}
						<div class="space-y-3">
							{#each processedParticipating as event}
								{@render eventCard(event)}
							{/each}
						</div>
					{/if}
				</Tabs.Content>
			</div>
		</Tabs.Root>
	</Dialog.Content>
</Dialog.Root>
