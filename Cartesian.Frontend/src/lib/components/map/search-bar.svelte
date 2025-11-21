<script lang="ts">
	import { Input } from "$lib/components/ui/input";
	import { Button } from "$lib/components/ui/button";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import {
		AiMagicIcon,
		Search01Icon,
		Location01Icon,
		Calendar01Icon,
	} from "@hugeicons/core-free-icons";
	import { getLayoutContext } from "$lib/context/layout.svelte";
	import UserMenu from "./user-menu.svelte";
	import { createForwardGeocodeQuery } from "$lib/api/queries/forward-geocode.query";
	import { mapState } from "$lib/components/map/map-state.svelte";
	import { cn } from "$lib/utils";
	import { animate, stagger } from "motion";
	import { Debounced } from "runed";
	import * as Sidebar from "$lib/components/ui/sidebar";

	const layout = getLayoutContext();

	let searchValue = $state("");
	const debouncedSearch = new Debounced<string>(() => searchValue, 300);
	let isAIMode = $state(false);
	let isFocused = $state(false);

	let searchBarRef: HTMLDivElement | undefined = $state();
	let resultsContainerRef: HTMLDivElement | undefined = $state();
	const geoQuery = createForwardGeocodeQuery(() => debouncedSearch.current);

	const showExpanded = $derived(isFocused && searchValue.length > 0);

	const placeholderEvents = [
		{ id: 1, name: "Jazz Night at The Blue Note", date: "Tonight, 8:00 PM" },
		{ id: 2, name: "Tech Meetup 2024", date: "Tomorrow, 6:00 PM" },
		{ id: 3, name: "Farmers Market", date: "Sat, 9:00 AM" },
	];

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
		console.log("Selected event:", event);
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
						class="h-10 border-0 bg-transparent dark:bg-transparent px-0 text-base shadow-none placeholder:text-muted-foreground/70 focus-visible:ring-0"
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
				<div class="animate-item mb-4">
					<h3
						class="mb-2 px-2 text-xs font-medium tracking-wider text-muted-foreground uppercase"
					>
						Events
					</h3>
					<div class="flex flex-col gap-1">
						{#if isAIMode}
							<div
								class="flex items-center gap-3 rounded-xl p-3 text-sm text-muted-foreground"
							>
								<HugeiconsIcon
									icon={AiMagicIcon}
									className="size-5 animate-pulse"
								/>
								<span>AI is analyzing your request...</span>
							</div>
						{:else}
							{#each placeholderEvents as event}
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
										<span class="truncate text-xs text-muted-foreground"
											>{event.date}</span
										>
									</div>
								</button>
							{/each}
						{/if}
					</div>
				</div>

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
