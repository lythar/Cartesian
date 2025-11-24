<script lang="ts">
	import { Input } from "$lib/components/ui/input";
	import { Button } from "$lib/components/ui/button";
	import * as Tooltip from "$lib/components/ui/tooltip";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import {
		AiMagicIcon,
		Search01Icon,
		Location01Icon,
		Calendar01Icon,
		UserIcon,
		UserGroupIcon,
		RefreshIcon,
	} from "@hugeicons/core-free-icons";
	import { getLayoutContext } from "$lib/context/layout.svelte";
	import UserMenu from "./user-menu.svelte";
	import { createForwardGeocodeQuery } from "$lib/api/queries/forward-geocode.query";
	import { createSearchAllQuery } from "$lib/api/queries/search.query";
	import { mapState } from "$lib/components/map/map-state.svelte";
	import { cn } from "$lib/utils";
	import { animate, stagger } from "motion";
	import { Debounced } from "runed";
	import mapboxgl from "mapbox-gl";
	import { useQueryClient } from "@tanstack/svelte-query";
	import { toast } from "svelte-sonner";

	const layout = getLayoutContext();
	const queryClient = useQueryClient();

	let searchValue = $state("");
	const debouncedSearch = new Debounced<string>(() => searchValue, 300);
	let isAIMode = $state(false);
	let isFocused = $state(false);
	let isRefreshing = $state(false);

	let searchBarRef: HTMLDivElement | undefined = $state();
	let resultsContainerRef: HTMLDivElement | undefined = $state();
	const geoQuery = createForwardGeocodeQuery(() => debouncedSearch.current);
	const searchQuery = createSearchAllQuery(() => ({ query: debouncedSearch.current }));

	const showExpanded = $derived(isFocused && searchValue.length > 0);

	async function handleRefreshEvents() {
		if (isRefreshing) return;
		isRefreshing = true;
		try {
			await queryClient.invalidateQueries({ queryKey: ["/event/api/geojson"] });
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

		// Get the first window's location
		const window = event.windows[0];
		const location = window.location;

		if (!location || !location.coordinates) {
			console.warn("Event window has no location coordinates");
			isFocused = false;
			return;
		}

		const [lng, lat] = location.coordinates;

		// Zoom to the event location
		mapState.instance?.flyTo({
			center: [lng, lat],
			zoom: 16,
			essential: true,
			duration: 2000,
		});

		// Show popup with event details
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
		}, 2100); // Show popup after fly animation completes

		isFocused = false;
	}

	function handleCommunitySelect(community: any) {
		console.log("Selected community:", community);
		// TODO: Navigate to community page
		isFocused = false;
	}

	function handleUserSelect(user: any) {
		console.log("Selected user:", user);
		// TODO: Navigate to user profile
		isFocused = false;
	}
</script>

<div
	class="absolute top-4 right-0 left-0 z-20 flex flex-col items-center px-4 lg:right-auto lg:left-4 lg:w-[400px] lg:items-start lg:px-0"
>
	<div
		bind:this={searchBarRef}
		class="relative w-full rounded-full bg-card/90 shadow-neu-highlight backdrop-blur-md"
	>
		<div class="flex h-16 w-full items-center gap-2 px-2 lg:h-14">
			<div
				class="flex h-12 flex-1 items-center gap-2 rounded-full bg-secondary/50 px-3 transition-colors focus-within:bg-secondary lg:h-10"
			>
				<HugeiconsIcon icon={Search01Icon} className="size-5 text-muted-foreground" />
				<div class="flex-1">
					<Input
						bind:value={searchValue}
						placeholder={isAIMode
							? "Ask AI to find events..."
							: "Search events, locations..."}
						class="h-10 border-0 bg-transparent px-0 text-base shadow-none placeholder:text-muted-foreground/70 focus-visible:ring-0 dark:bg-transparent"
						onfocus={() => (isFocused = true)}
						onblur={() => {
							// Small timeout to allow clicks on results to register
							setTimeout(() => (isFocused = false), 200);
						}}
					/>
				</div>
				{#if searchValue}
					<button
						onclick={() => (searchValue = "")}
						class="flex h-6 w-6 animate-in items-center justify-center rounded-full text-muted-foreground duration-200 fade-in zoom-in hover:bg-muted hover:text-foreground"
					>
						<span class="text-xs">✕</span>
					</button>
				{/if}
			</div>

			<Tooltip.Root>
				<Tooltip.Trigger>
					<Button
						variant={isAIMode ? "default" : "ghost"}
						size="icon"
						class={cn(
							"h-12 w-12 shrink-0 rounded-full transition-all duration-200 lg:h-10 lg:w-10",
							isAIMode
								? "bg-primary text-primary-foreground shadow-lg"
								: "bg-secondary/50 text-foreground",
						)}
						onclick={() => (isAIMode = !isAIMode)}
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

			<Tooltip.Root>
				<Tooltip.Trigger>
					<Button
						variant="ghost"
						size="icon"
						class={cn(
							"h-12 w-12 shrink-0 rounded-full bg-secondary/50 text-foreground transition-all duration-200 lg:h-10 lg:w-10",
							isRefreshing && "animate-spin",
						)}
						onclick={handleRefreshEvents}
						disabled={isRefreshing}
					>
						<HugeiconsIcon icon={RefreshIcon} size={20} className="duotone-fill" />
						<span class="sr-only">Refresh events</span>
					</Button>
				</Tooltip.Trigger>
				<Tooltip.Content>Refresh events</Tooltip.Content>
			</Tooltip.Root>

			{#if layout.isMobile && !showExpanded}
				<div class="animate-in duration-300 fade-in slide-in-from-right-4">
					<UserMenu class="flex h-12 w-12 items-center justify-center" />
				</div>
			{/if}
		</div>
	</div>

	<div
		bind:this={resultsContainerRef}
		class="mt-2 w-full overflow-hidden rounded-3xl bg-card/90 shadow-neu-highlight backdrop-blur-md"
		style="display: none; height: 0; opacity: 0;"
	>
		<div class="flex flex-col px-4 py-4">
			<div class="scrollbar-thin max-h-[60vh] overflow-y-auto">
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
									class="group flex w-full items-center gap-3 rounded-xl p-2 text-left transition-colors hover:bg-secondary/50"
									onclick={() => handleUserSelect(user)}
								>
									<div
										class="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-amber-500/10 text-amber-500 group-hover:bg-amber-500/20"
									>
										<HugeiconsIcon icon={UserIcon} className="size-5" />
									</div>
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
			</div>
		</div>
	</div>
</div>
