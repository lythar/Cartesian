<script lang="ts">
	import { Input } from "$lib/components/ui/input";
	import { Button } from "$lib/components/ui/button";
	import * as Tooltip from "$lib/components/ui/tooltip";
	import * as Avatar from "$lib/components/ui/avatar";
	import { Badge } from "$lib/components/ui/badge";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import {
		AiMagicIcon,
		Search01Icon,
		Location01Icon,
		Calendar01Icon,
		Calendar03Icon,
		UserIcon,
		UserGroupIcon,
		RefreshIcon,
		SentIcon,
		Cancel01Icon,
		Loading03Icon,
		ArrowDown01Icon,
	} from "@hugeicons/core-free-icons";
	import { getLayoutContext } from "$lib/context/layout.svelte";
	import UserMenu from "./user-menu.svelte";
	import { createForwardGeocodeQuery } from "$lib/api/queries/forward-geocode.query";
	import { createSearchAllQuery } from "$lib/api/queries/search.query";
	import { mapState } from "$lib/components/map/map-state.svelte";
	import { cn, getAvatarUrl, getInitials } from "$lib/utils";
	import { animate, stagger } from "motion";
	import { Debounced } from "runed";
	import mapboxgl from "mapbox-gl";
	import { useQueryClient } from "@tanstack/svelte-query";
	import { toast } from "svelte-sonner";
	import { openUserProfile } from "$lib/components/profile/profile-state.svelte";
	import { Chat } from "@ai-sdk/svelte";
	import { DefaultChatTransport } from "ai";
	import { format, formatDistanceToNow, isBefore, addWeeks } from "date-fns";

	interface MapboxSearchResult {
		name: string;
		fullAddress: string;
		coordinates: number[];
		type: string;
	}

	interface EventSearchResult {
		id: string;
		name: string;
		description: string;
		author: string;
		community: string | null;
		visibility: string;
		timing: string;
		tags: string[];
		coordinates?: number[];
		startTime?: string;
		endTime?: string;
		windows: Array<{
			id: string;
			title: string;
			startTime: string;
			endTime: string;
			location: {
				address: string;
				coordinates: number[];
			} | null;
		}>;
	}

	interface DiscoverEventResult {
		id: string;
		name: string;
		description: string;
		author: string;
		tags: string[];
		timing: string;
		coordinates: number[];
		startTime: string | null;
		endTime: string | null;
	}

	const layout = getLayoutContext();
	const queryClient = useQueryClient();

	let searchValue = $state("");
	const debouncedSearch = new Debounced<string>(() => searchValue, 300);
	let isAIMode = $state(false);
	let isFocused = $state(false);
	let isRefreshing = $state(false);
	let expandedTools = $state<Set<string>>(new Set());

	let searchBarRef: HTMLDivElement | undefined = $state();
	let resultsContainerRef: HTMLDivElement | undefined = $state();
	const geoQuery = createForwardGeocodeQuery(() => debouncedSearch.current);
	const searchQuery = createSearchAllQuery(() => ({ query: debouncedSearch.current }));

	const chat = new Chat({
		transport: new DefaultChatTransport({ api: "/api/ai/chat" }),
		onError: (error) => {
			toast.error("AI search failed: " + error.message);
		},
	});

	const showExpanded = $derived(
		(isFocused && searchValue.length > 0) || (isAIMode && chat.messages.length > 0),
	);

	function toggleToolExpanded(toolId: string) {
		if (expandedTools.has(toolId)) {
			expandedTools.delete(toolId);
		} else {
			expandedTools.add(toolId);
		}
		expandedTools = new Set(expandedTools);
	}

	function formatEventDate(startTime: string | null | undefined): string {
		if (!startTime) return "No date set";
		const date = new Date(startTime);
		return format(date, "MMM d, h:mm a");
	}

	function formatEventDateRelative(startTime: string | null | undefined): string {
		if (!startTime) return "";
		const date = new Date(startTime);
		return formatDistanceToNow(date, { addSuffix: true });
	}

	function isEventUpcomingSoon(startTime: string | null | undefined): boolean {
		if (!startTime) return false;
		const date = new Date(startTime);
		const now = new Date();
		const oneWeekFromNow = addWeeks(now, 1);
		return isBefore(date, oneWeekFromNow) && !isBefore(date, now);
	}

	function renderMarkdown(text: string): string {
		return text
			.replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
			.replace(/\*(.+?)\*/g, '<em>$1</em>')
			.replace(/`(.+?)`/g, '<code class="rounded bg-muted px-1 py-0.5 text-xs">$1</code>')
			.replace(/\n/g, '<br />');
	}

	async function handleRefreshEvents() {
		if (isRefreshing) return;
		isRefreshing = true;
		try {
			await queryClient.refetchQueries({ queryKey: ["/event/api/geojson"] });
			toast.success("Events refreshed");
		} catch (e) {
			toast.error("Failed to refresh events");
		} finally {
			isRefreshing = false;
		}
	}

	$effect(() => {
		if (!searchBarRef || !resultsContainerRef) return;

		if (showExpanded) {
			animate(
				searchBarRef,
				{ borderRadius: "24px" },
				{ duration: 0.4, ease: [0.2, 0.8, 0.2, 1] },
			);

			resultsContainerRef.style.display = "block";

			animate(
				resultsContainerRef,
				{ height: "auto", opacity: 1 },
				{ duration: 0.4, ease: [0.2, 0.8, 0.2, 1] },
			);

			// Stagger children
			// Todo: make a single stagger group
			const items = resultsContainerRef.querySelectorAll(".animate-item");
			if (items.length) {
				animate(
					items,
					{ opacity: [0, 1], y: [10, 0], filter: ["blur(4px)", "blur(0px)"] },
					{ delay: stagger(0.05, { startDelay: 0.1 }), duration: 0.3 },
				);
			}
		} else {
			// search bar back to pill
			animate(
				searchBarRef,
				{ borderRadius: "9999px" },
				{ duration: 0.3, ease: [0.2, 0.8, 0.2, 1] },
			);

			// Collapse results container
			animate(
				resultsContainerRef,
				{ height: 0, opacity: 0 },
				{ duration: 0.3, ease: [0.2, 0.8, 0.2, 1] },
			).finished.then(() => {
				if (resultsContainerRef && !showExpanded) {
					resultsContainerRef.style.display = "none";
				}
			});
		}
	});

	function handleLocationSelect(feature: any) {
		const [lng, lat] = feature.geometry.coordinates;

		mapState.instance?.flyTo({
			center: [lng, lat],
			zoom: 14,
			essential: true,
			duration: 2000,
		});

		isFocused = false;
	}

	function handleEventSelect(event: any) {
		if (!event.windows || event.windows.length === 0) {
			console.warn("Event has no windows with location data");
			isFocused = false;
			return;
		}

		const window = event.windows[0];
		const location = window.location;

		if (!location || !location.coordinates) {
			console.warn("Event window has no location coordinates");
			isFocused = false;
			return;
		}

		const [lng, lat] = location.coordinates;

		mapState.instance?.flyTo({
			center: [lng, lat],
			zoom: 16,
			essential: true,
			duration: 2000,
		});

		setTimeout(() => {
			new mapboxgl.Popup()
				.setLngLat([lng, lat])
				.setHTML(
					`
					<div class="p-2">
						<h3 class="font-semibold text-sm mb-1">${event.name}</h3>
						<p class="text-xs text-muted-foreground">${event.description}</p>
					</div>
				`,
				)
				.addTo(mapState.instance!);
		}, 2100);

		isFocused = false;
	}

	function handleCommunitySelect(community: any) {
		isFocused = false;
		window.location.href = `/community/${community.id}`;
	}

	function handleUserSelect(user: any) {
		isFocused = false;
		openUserProfile(user.id);
	}

	function handleAISubmit(e: SubmitEvent) {
		e.preventDefault();
		if (!searchValue.trim()) return;
		chat.sendMessage({ text: searchValue });
		searchValue = "";
	}

	function clearAIChat() {
		chat.messages = [];
	}

	function handleAIModeToggle() {
		isAIMode = !isAIMode;
		if (!isAIMode) {
			clearAIChat();
		}
	}

	function handleAILocationClick(coordinates: number[], name: string) {
		if (coordinates.length < 2) return;
		const [lng, lat] = coordinates;
		mapState.instance?.flyTo({
			center: [lng, lat],
			zoom: 14,
			essential: true,
			duration: 2000,
		});
	}

	function getToolInput<T>(part: any): T | undefined {
		return part?.input as T | undefined;
	}

	function getToolOutput<T>(part: any): T | undefined {
		return part?.output as T | undefined;
	}

	function handleAIEventClick(eventData: DiscoverEventResult | EventSearchResult) {
		let lng: number, lat: number;

		if ('coordinates' in eventData && eventData.coordinates && eventData.coordinates.length >= 2) {
			[lng, lat] = eventData.coordinates;
		} else if ('windows' in eventData && eventData.windows && eventData.windows.length > 0) {
			const eventWindow = eventData.windows[0];
			if (!eventWindow.location?.coordinates) return;
			[lng, lat] = eventWindow.location.coordinates;
		} else {
			return;
		}

		mapState.instance?.flyTo({
			center: [lng, lat],
			zoom: 16,
			essential: true,
			duration: 2000,
		});

		setTimeout(() => {
			new mapboxgl.Popup()
				.setLngLat([lng, lat])
				.setHTML(
					`<div class="p-2">
						<h3 class="font-semibold text-sm mb-1">${eventData.name}</h3>
						<p class="text-xs text-muted-foreground">${eventData.description?.substring(0, 100) || ""}</p>
					</div>`,
				)
				.addTo(mapState.instance!);
		}, 2100);
	}

	function getToolLabel(toolName: string, input: any): string {
		switch (toolName) {
			case "mapboxsearch":
				return `Searching locations: "${input?.query || ''}"`;
			case "eventsearch":
				return `Searching events: "${input?.query || ''}"`;
			case "discoverevents":
				const parts = [];
				if (input?.tags?.length) parts.push(input.tags.join(", "));
				if (input?.boundingBox) parts.push("in area");
				return parts.length ? `Discovering events: ${parts.join(" ")}` : "Discovering events";
			case "geteventdetails":
				return "Getting event details";
			default:
				return `Using ${toolName}`;
		}
	}

	function getToolIcon(toolName: string) {
		switch (toolName) {
			case "mapboxsearch":
				return Location01Icon;
			case "eventsearch":
			case "discoverevents":
			case "geteventdetails":
				return Calendar03Icon;
			default:
				return Search01Icon;
		}
	}

	function getToolColor(toolName: string): string {
		switch (toolName) {
			case "mapboxsearch":
				return "text-emerald-500";
			case "eventsearch":
				return "text-blue-500";
			case "discoverevents":
				return "text-violet-500";
			case "geteventdetails":
				return "text-purple-500";
			default:
				return "text-muted-foreground";
		}
	}

	function filterEventListFromText(text: string, hasEventResults: boolean): string {
		if (!hasEventResults) return text;
		const lines = text.split('\n');
		const filtered = lines.filter(line => {
			const trimmed = line.trim();
			if (trimmed.startsWith('* **') || trimmed.startsWith('- **')) return false;
			if (trimmed.match(/^\* (Opis|Kiedy|Description|When|Date|Location|Tags):/i)) return false;
			if (trimmed.match(/^- (Opis|Kiedy|Description|When|Date|Location|Tags):/i)) return false;
			return true;
		});
		return filtered.join('\n').trim();
	}

	function messageHasEventResults(message: any): boolean {
		if (!message.parts) return false;
		return message.parts.some((part: any) => {
			if (part.type === "tool-eventsearch" && part.state === "output-available") {
				const output = getToolOutput<{ results?: EventSearchResult[] }>(part);
				return output?.results && output.results.length > 0;
			}
			if (part.type === "tool-discoverevents" && part.state === "output-available") {
				const output = getToolOutput<{ results?: DiscoverEventResult[] }>(part);
				return output?.results && output.results.length > 0;
			}
			if (part.type === "tool-geteventdetails" && part.state === "output-available") {
				const output = getToolOutput<EventSearchResult & { error?: string }>(part);
				return output && !output.error;
			}
			return false;
		});
	}
</script>

{#snippet eventCard(event: EventSearchResult)}
	<button
		type="button"
		onclick={() => handleAIEventClick(event)}
		class="group relative flex w-full items-start gap-3 rounded-xl border border-border/40 bg-muted/20 p-3 text-left transition-all hover:border-primary/20 hover:bg-muted/40"
	>
		<div class="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
			<HugeiconsIcon icon={Calendar03Icon} className="size-5" />
		</div>
		<div class="flex min-w-0 flex-1 flex-col gap-1">
			<div class="flex items-start justify-between gap-2">
				<h4 class="truncate text-sm leading-tight font-semibold">{event.name}</h4>
				{#if event.windows?.[0]?.startTime && isEventUpcomingSoon(event.windows[0].startTime)}
					<Badge
						variant="secondary"
						class="h-5 shrink-0 border-primary/20 bg-primary/10 px-1.5 text-[10px] font-medium whitespace-nowrap text-primary"
					>
						{formatEventDateRelative(event.windows[0].startTime)}
					</Badge>
				{/if}
			</div>
			<div class="flex flex-wrap items-center gap-x-2 gap-y-0.5 text-[11px] text-muted-foreground/80">
				<div class="flex items-center gap-1">
					<HugeiconsIcon icon={Calendar01Icon} className="size-3" />
					<span>{event.windows?.[0]?.startTime ? formatEventDate(event.windows[0].startTime) : "No date"}</span>
				</div>
				<div class="flex items-center gap-1">
					<HugeiconsIcon icon={UserIcon} className="size-3" />
					<span class="truncate max-w-[100px]">{event.author}</span>
				</div>
			</div>
			{#if event.description}
				<p class="mt-0.5 line-clamp-2 text-xs leading-relaxed text-muted-foreground">
					{event.description}
				</p>
			{/if}
			{#if event.tags && event.tags.length > 0}
				<div class="mt-1.5 flex flex-wrap gap-1">
					{#each event.tags.slice(0, 3) as tag}
						<span class="inline-flex items-center rounded-md bg-muted/50 px-1.5 py-0.5 text-[10px] font-medium text-muted-foreground ring-1 ring-border/50 ring-inset">
							{tag}
						</span>
					{/each}
					{#if event.tags.length > 3}
						<span class="inline-flex items-center rounded-md bg-muted/50 px-1.5 py-0.5 text-[10px] font-medium text-muted-foreground ring-1 ring-border/50 ring-inset">
							+{event.tags.length - 3}
						</span>
					{/if}
				</div>
			{/if}
		</div>
	</button>
{/snippet}

{#snippet discoverEventCard(event: DiscoverEventResult)}
	<button
		type="button"
		onclick={() => handleAIEventClick(event)}
		class="group relative flex w-full items-start gap-3 rounded-xl border border-border/40 bg-muted/20 p-3 text-left transition-all hover:border-primary/20 hover:bg-muted/40"
	>
		<div class="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
			<HugeiconsIcon icon={Calendar03Icon} className="size-5" />
		</div>
		<div class="flex min-w-0 flex-1 flex-col gap-1">
			<div class="flex items-start justify-between gap-2">
				<h4 class="truncate text-sm leading-tight font-semibold">{event.name}</h4>
				{#if event.startTime && isEventUpcomingSoon(event.startTime)}
					<Badge
						variant="secondary"
						class="h-5 shrink-0 border-primary/20 bg-primary/10 px-1.5 text-[10px] font-medium whitespace-nowrap text-primary"
					>
						{formatEventDateRelative(event.startTime)}
					</Badge>
				{/if}
			</div>
			<div class="flex flex-wrap items-center gap-x-2 gap-y-0.5 text-[11px] text-muted-foreground/80">
				<div class="flex items-center gap-1">
					<HugeiconsIcon icon={Calendar01Icon} className="size-3" />
					<span>{formatEventDate(event.startTime)}</span>
				</div>
				<div class="flex items-center gap-1">
					<HugeiconsIcon icon={UserIcon} className="size-3" />
					<span class="truncate max-w-[100px]">{event.author}</span>
				</div>
				{#if event.timing}
					<Badge variant="outline" class="h-4 px-1 text-[9px]">{event.timing}</Badge>
				{/if}
			</div>
			{#if event.description}
				<p class="mt-0.5 line-clamp-2 text-xs leading-relaxed text-muted-foreground">
					{event.description}
				</p>
			{/if}
			{#if event.tags && event.tags.length > 0}
				<div class="mt-1.5 flex flex-wrap gap-1">
					{#each event.tags.slice(0, 3) as tag}
						<span class="inline-flex items-center rounded-md bg-muted/50 px-1.5 py-0.5 text-[10px] font-medium text-muted-foreground ring-1 ring-border/50 ring-inset">
							{tag}
						</span>
					{/each}
					{#if event.tags.length > 3}
						<span class="inline-flex items-center rounded-md bg-muted/50 px-1.5 py-0.5 text-[10px] font-medium text-muted-foreground ring-1 ring-border/50 ring-inset">
							+{event.tags.length - 3}
						</span>
					{/if}
				</div>
			{/if}
		</div>
	</button>
{/snippet}

<div
	class="absolute top-4 right-0 left-0 z-20 flex flex-col items-center px-4 lg:right-auto lg:left-4 lg:w-[400px] lg:items-start lg:px-0"
>
	<div
		bind:this={searchBarRef}
		class="relative w-full rounded-full bg-background/90 shadow-neu-highlight backdrop-blur-md"
	>
		<form
			class="flex h-16 w-full items-center gap-2 px-2 lg:h-14 group"
			onsubmit={isAIMode ? handleAISubmit : (e) => e.preventDefault()}
		>
			<div
				class="flex h-12 flex-1 items-center gap-2 rounded-full bg-secondary/30 px-3 transition-colors focus-within:bg-secondary/50 lg:h-10"
			>
				<HugeiconsIcon
					icon={isAIMode ? AiMagicIcon : Search01Icon}
					className={cn("size-5", isAIMode ? "text-primary" : "text-muted-foreground")}
				/>
				<div class="flex-1">
					<Input
						bind:value={searchValue}
						placeholder={isAIMode
							? "Ask AI to find events..."
							: "Search events, locations..."}
						class="h-10 border-0 bg-transparent px-0 text-base shadow-none placeholder:text-muted-foreground/70 focus-visible:ring-0 dark:bg-transparent"
						onfocus={() => (isFocused = true)}
						onblur={() => {
							setTimeout(() => (isFocused = false), 200);
						}}
						disabled={chat.status === "streaming" || chat.status === "submitted"}
					/>
				</div>
				{#if searchValue}
					<button
						type="button"
						onclick={() => (searchValue = "")}
						class="flex h-6 w-6 animate-in items-center justify-center rounded-full text-muted-foreground duration-200 fade-in zoom-in hover:bg-muted hover:text-foreground"
					>
						<span class="text-xs">✕</span>
					</button>
				{/if}
				{#if isAIMode && searchValue.trim()}
					<button
						type="submit"
						disabled={chat.status === "streaming" || chat.status === "submitted"}
						class="flex h-8 w-8 items-center justify-center rounded-full bg-primary text-primary-foreground transition-colors hover:bg-primary/90 disabled:opacity-50"
					>
						{#if chat.status === "streaming" || chat.status === "submitted"}
							<HugeiconsIcon icon={Loading03Icon} className="size-4 animate-spin" />
						{:else}
							<HugeiconsIcon icon={SentIcon} className="size-4" />
						{/if}
					</button>
				{/if}
			</div>

			<Tooltip.Root>
				<Tooltip.Trigger>
					<Button
						type="button"
						variant={isAIMode ? "default" : "ghost"}
						size="icon"
						class={cn(
							"h-12 w-12 shrink-0 rounded-full transition-all duration-200 lg:h-10 lg:w-10",
							isAIMode
								? "bg-primary text-primary-foreground shadow-lg"
								: "bg-secondary/30 text-foreground",
						)}
						onclick={handleAIModeToggle}
					>
						<HugeiconsIcon
							icon={AiMagicIcon}
							size={20}
							className={isAIMode ? "" : "duotone-fill"}
						/>
						<span class="sr-only">Toggle AI mode</span>
					</Button>
				</Tooltip.Trigger>
				<Tooltip.Content>{isAIMode ? "Disable AI mode" : "Enable AI mode"}</Tooltip.Content>
			</Tooltip.Root>

			{#if !isAIMode}
				<Tooltip.Root>
					<Tooltip.Trigger>
            <!-- group-has-focus:hidden -->
						<Button
							type="button"
							variant="ghost"
							size="icon"
							class={cn(
								"h-12 w-12 shrink-0 rounded-full bg-secondary/30 text-foreground transition-all duration-200 lg:h-10 lg:w-10 ",
								isRefreshing && "animate-spin",
							)}
							onclick={handleRefreshEvents}
							disabled={isRefreshing}
						>
							<HugeiconsIcon icon={RefreshIcon} size={20} />
							<span class="sr-only">Refresh events</span>
						</Button>
					</Tooltip.Trigger>
					<Tooltip.Content>Refresh events</Tooltip.Content>
				</Tooltip.Root>
			{/if}

			{#if layout.isMobile && !showExpanded}
				<div class="animate-in duration-300 fade-in slide-in-from-right-4">
					<UserMenu class="flex h-12 w-12 items-center justify-center" />
				</div>
			{/if}
		</form>
	</div>

	<div
		bind:this={resultsContainerRef}
		class="mt-2 w-full overflow-hidden rounded-3xl bg-card/90 shadow-neu-highlight backdrop-blur-md"
		style="display: none; height: 0; opacity: 0;"
	>
		<div class="flex flex-col px-4 py-4">
			<div class="scrollbar-thin max-h-[60vh] overflow-y-auto">
				{#if isAIMode}
					<!-- AI Chat Interface -->
					<div class="animate-item flex flex-col gap-3">
						{#if chat.messages.length === 0}
							<div class="rounded-xl p-4 text-center text-sm text-muted-foreground">
								<HugeiconsIcon
									icon={AiMagicIcon}
									className="mx-auto mb-2 size-8 text-primary/50"
								/>
								<p>Ask me to find events, locations, or get details about happenings.</p>
								<p class="mt-1 text-xs">Try: "Find concerts near me" or "What events are happening this weekend?"</p>
							</div>
						{:else}
							{#each chat.messages as message (message.id)}
								{@const hasEventResults = messageHasEventResults(message)}
								<div
									class={cn(
										"rounded-xl p-3",
										message.role === "user"
											? "ml-8 bg-primary text-primary-foreground"
											: "mr-4 bg-transparent",
									)}
								>
									{#each message.parts as part, partIndex (partIndex)}
										{@const toolId = `${message.id}-${partIndex}`}
										{#if part.type === "text"}
											{@const filteredText = filterEventListFromText(part.text, hasEventResults)}
											{#if filteredText}
												<div class="prose prose-sm dark:prose-invert max-w-none text-sm">
													{@html renderMarkdown(filteredText)}
												</div>
											{/if}
										{:else if part.type === "tool-mapboxsearch"}
											{@const input = getToolInput<{ query: string }>(part)}
											{@const output = getToolOutput<{ results: MapboxSearchResult[] }>(part)}
											{@const isExpanded = expandedTools.has(toolId)}
											<div class="mt-2">
												<button
													type="button"
													onclick={() => toggleToolExpanded(toolId)}
													class="flex w-full items-center gap-2 rounded-lg bg-background/50 p-2 text-left transition-colors hover:bg-background/70"
												>
													{#if part.state === "input-available" || part.state === "input-streaming"}
														<HugeiconsIcon icon={Loading03Icon} className="size-4 text-emerald-500 animate-spin" />
													{:else}
														<HugeiconsIcon icon={Location01Icon} className="size-4 text-emerald-500" />
													{/if}
													<span class="flex-1 text-xs font-medium text-emerald-600 dark:text-emerald-400">
														{getToolLabel("mapboxsearch", input)}
													</span>
													{#if part.state === "output-available"}
														<HugeiconsIcon
															icon={ArrowDown01Icon}
															className={cn("size-4 text-muted-foreground transition-transform", isExpanded && "rotate-180")}
														/>
													{/if}
												</button>
												{#if isExpanded && part.state === "output-available" && output?.results}
													<div class="mt-1 flex flex-col gap-1 rounded-lg bg-background/30 p-2">
														{#each output.results.slice(0, 5) as result}
															<button
																type="button"
																onclick={() => handleAILocationClick(result.coordinates, result.name)}
																class="flex items-center gap-2 rounded-lg p-2 text-left transition-colors hover:bg-secondary/50"
															>
																<HugeiconsIcon
																	icon={Location01Icon}
																	className="size-4 text-emerald-500"
																/>
																<div class="flex-1 overflow-hidden">
																	<p class="truncate text-xs font-medium">{result.name}</p>
																	<p class="truncate text-xs text-muted-foreground">
																		{result.fullAddress}
																	</p>
																</div>
															</button>
														{/each}
													</div>
												{/if}
											</div>
										{:else if part.type === "tool-eventsearch"}
											{@const input = getToolInput<{ query: string }>(part)}
											{@const output = getToolOutput<{ results?: EventSearchResult[]; error?: string }>(part)}
											<div class="mt-2">
												{#if part.state === "input-available" || part.state === "input-streaming"}
													<div class="flex items-center gap-2 rounded-lg bg-background/50 p-2">
														<HugeiconsIcon icon={Loading03Icon} className="size-4 text-blue-500 animate-spin" />
														<span class="text-xs font-medium text-blue-600 dark:text-blue-400">
															{getToolLabel("eventsearch", input)}
														</span>
													</div>
												{:else if part.state === "output-available" && output?.results}
													<div class="flex flex-col gap-2">
														{#each output.results.slice(0, 5) as event}
															{@render eventCard(event)}
														{/each}
													</div>
												{:else if output?.error}
													<p class="p-2 text-xs text-destructive">{output.error}</p>
												{/if}
											</div>
										{:else if part.type === "tool-discoverevents"}
											{@const input = getToolInput<{ tags?: string[]; boundingBox?: any; limit?: number }>(part)}
											{@const output = getToolOutput<{ results?: DiscoverEventResult[]; error?: string; message?: string }>(part)}
											<div class="mt-2">
												{#if part.state === "input-available" || part.state === "input-streaming"}
													<div class="flex items-center gap-2 rounded-lg bg-background/50 p-2">
														<HugeiconsIcon icon={Loading03Icon} className="size-4 text-violet-500 animate-spin" />
														<span class="text-xs font-medium text-violet-600 dark:text-violet-400">
															{getToolLabel("discoverevents", input)}
														</span>
													</div>
												{:else if part.state === "output-available" && output?.results}
													<div class="flex flex-col gap-2">
														{#each output.results.slice(0, 5) as event}
															{@render discoverEventCard(event)}
														{/each}
													</div>
												{:else if output?.error}
													<p class="p-2 text-xs text-destructive">{output.error}</p>
												{:else if output?.message}
													<p class="p-2 text-xs text-muted-foreground">{output.message}</p>
												{/if}
											</div>
										{:else if part.type === "tool-geteventdetails"}
											{@const output = getToolOutput<EventSearchResult & { error?: string }>(part)}
											<div class="mt-2">
												{#if part.state === "input-available" || part.state === "input-streaming"}
													<div class="flex items-center gap-2 rounded-lg bg-background/50 p-2">
														<HugeiconsIcon icon={Loading03Icon} className="size-4 text-purple-500 animate-spin" />
														<span class="text-xs font-medium text-purple-600 dark:text-purple-400">
															{getToolLabel("geteventdetails", null)}
														</span>
													</div>
												{:else if part.state === "output-available" && output && !output.error}
													{@render eventCard(output)}
												{:else if output?.error}
													<p class="p-2 text-xs text-destructive">{output.error}</p>
												{/if}
											</div>
										{/if}
									{/each}
								</div>
							{/each}

							{#if chat.status === "streaming" || chat.status === "submitted"}
								<div class="mr-8 flex items-center gap-2 rounded-xl bg-secondary/50 p-3">
									<HugeiconsIcon icon={Loading03Icon} className="size-4 animate-spin text-primary" />
									<span class="text-sm text-muted-foreground">
										{chat.status === "submitted" ? "Thinking..." : "Responding..."}
									</span>
								</div>
							{/if}

							<div class="flex items-center justify-between pt-2">
								<Button
									type="button"
									variant="ghost"
									size="sm"
									onclick={clearAIChat}
									class="text-xs text-muted-foreground"
								>
									<HugeiconsIcon icon={Cancel01Icon} className="mr-1 size-3" />
									Clear chat
								</Button>
								{#if chat.status === "streaming"}
									<Button
										type="button"
										variant="ghost"
										size="sm"
										onclick={() => chat.stop()}
										class="text-xs text-destructive"
									>
										Stop
									</Button>
								{/if}
							</div>
						{/if}
					</div>
				{:else}
					<!-- Standard Search Results -->
					<!-- Events Section -->
					{#if searchQuery.data?.events && searchQuery.data.events.length > 0}
					<div class="animate-item mb-4">
						<h3
							class="mb-2 px-2 text-xs font-medium tracking-wider text-muted-foreground uppercase"
						>
							Events
						</h3>
						<div class="flex flex-col gap-1">
							{#each searchQuery.data.events as event}
								<button
									type="button"
									class="group flex w-full items-center gap-3 rounded-xl p-2 text-left transition-colors hover:bg-secondary/50"
									onclick={() => handleEventSelect(event)}
								>
									<div
										class="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-blue-500/10 text-blue-500 group-hover:bg-blue-500/20"
									>
										<HugeiconsIcon icon={Calendar01Icon} className="size-5" />
									</div>
									<div class="flex flex-col overflow-hidden">
										<span class="truncate text-sm font-medium"
											>{event.name}</span
										>
										<span class="truncate text-xs text-muted-foreground">
											{event.description.substring(0, 60)}{event.description
												.length > 60
												? "..."
												: ""}
										</span>
									</div>
								</button>
							{/each}
						</div>
					</div>
				{/if}

				<!-- Communities Section -->
				{#if searchQuery.data?.communities && searchQuery.data.communities.length > 0}
					<div class="animate-item mb-4">
						<h3
							class="mb-2 px-2 text-xs font-medium tracking-wider text-muted-foreground uppercase"
						>
							Communities
						</h3>
						<div class="flex flex-col gap-1">
							{#each searchQuery.data.communities as community}
								<button
									type="button"
									class="group flex w-full items-center gap-3 rounded-xl p-2 text-left transition-colors hover:bg-secondary/50"
									onclick={() => handleCommunitySelect(community)}
								>
									<div
										class="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-purple-500/10 text-purple-500 group-hover:bg-purple-500/20"
									>
										<HugeiconsIcon icon={UserGroupIcon} className="size-5" />
									</div>
									<div class="flex flex-col overflow-hidden">
										<span class="truncate text-sm font-medium"
											>{community.name}</span
										>
										<span class="truncate text-xs text-muted-foreground">
											{community.description.substring(0, 60)}{community
												.description.length > 60
												? "..."
												: ""}
										</span>
									</div>
								</button>
							{/each}
						</div>
					</div>
				{/if}

				<!-- Users Section -->
				{#if searchQuery.data?.users && searchQuery.data.users.length > 0}
					<div class="animate-item mb-4">
						<h3
							class="mb-2 px-2 text-xs font-medium tracking-wider text-muted-foreground uppercase"
						>
							Users
						</h3>
						<div class="flex flex-col gap-1">
							{#each searchQuery.data.users as user}
								<button
									type="button"
									class="group flex w-full items-center gap-3 rounded-xl p-2 text-left transition-colors hover:bg-secondary/50"
									onclick={() => handleUserSelect(user)}
								>
									{#if user.avatar}
										<Avatar.Root class="h-10 w-10 shrink-0 rounded-lg border border-border/40">
											<Avatar.Image
												src={getAvatarUrl(user.avatar)}
												alt={user.name}
												class="object-cover rounded-lg"
											/>
											<Avatar.Fallback class="rounded-lg bg-amber-500/10 text-amber-500 text-xs font-medium">
												{getInitials(user.name)}
											</Avatar.Fallback>
										</Avatar.Root>
									{:else}
										<div
											class="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-amber-500/10 text-amber-500 group-hover:bg-amber-500/20"
										>
											<HugeiconsIcon icon={UserIcon} className="size-5" />
										</div>
									{/if}
									<div class="flex flex-col overflow-hidden">
										<span class="truncate text-sm font-medium">{user.name}</span
										>
									</div>
								</button>
							{/each}
						</div>
					</div>
				{/if}

				<!-- Loading State -->
				{#if searchQuery.isLoading}
					<div class="animate-item mb-4">
						<div
							class="flex items-center gap-3 rounded-xl p-3 text-sm text-muted-foreground"
						>
							<div
								class="size-4 animate-spin rounded-full border-2 border-primary border-t-transparent"
							></div>
							<span>Searching...</span>
						</div>
					</div>
				{/if}

				<!-- Locations Section -->
				<div class="animate-item">
					<h3
						class="mb-2 px-2 text-xs font-medium tracking-wider text-muted-foreground uppercase"
					>
						Locations
					</h3>
					<div class="flex flex-col gap-1">
						{#if geoQuery.isLoading}
							<div
								class="flex items-center gap-3 rounded-xl p-3 text-sm text-muted-foreground"
							>
								<div
									class="size-4 animate-spin rounded-full border-2 border-primary border-t-transparent"
								></div>
								<span>Searching locations...</span>
							</div>
						{:else if geoQuery.data?.features?.length === 0}
							<div class="rounded-xl p-3 text-sm text-muted-foreground">
								No locations found
							</div>
						{:else if geoQuery.data?.features}
							{#each geoQuery.data.features as feature}
								<button
									type="button"
									class="group flex w-full items-center gap-3 rounded-xl p-2 text-left transition-colors hover:bg-secondary/50"
									onclick={() => handleLocationSelect(feature)}
								>
									<div
										class="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-emerald-500/10 text-emerald-500 group-hover:bg-emerald-500/20"
									>
										<HugeiconsIcon icon={Location01Icon} className="size-5" />
									</div>
									<div class="flex flex-col overflow-hidden">
										<span class="truncate text-sm font-medium"
											>{feature.properties.name}</span
										>
										<span class="truncate text-xs text-muted-foreground">
											{feature.properties.full_address}
										</span>
									</div>
								</button>
							{/each}
						{/if}
					</div>
				</div>
			{/if}
			</div>
		</div>
	</div>
</div>
