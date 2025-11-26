<script lang="ts">
	import { cn } from "$lib/utils";
	import { toast } from "svelte-sonner";
	import { animate } from "motion";
	import { editEventOverlayState } from "./map-state.svelte";
	import Button from "../ui/button/button.svelte";
	import { Label } from "$lib/components/ui/label";
	import * as Form from "$lib/components/ui/form";
	import * as AlertDialog from "$lib/components/ui/alert-dialog";
	import { Input } from "$lib/components/ui/input";
	import { Textarea } from "$lib/components/ui/textarea";
	import { superForm, defaults } from "sveltekit-superforms";
	import { zod4 } from "sveltekit-superforms/adapters";
	import { z } from "zod";
	import {
		Cancel01Icon,
		Location01Icon,
		Gps01Icon,
		MapsLocation01Icon,
		Search01Icon,
		Loading03Icon,
		Calendar03Icon,
		Clock01Icon,
		Add01Icon,
		Delete02Icon,
		ImageAdd01Icon,
		AiEditingIcon,
	} from "@hugeicons/core-free-icons";
	import { createForwardGeocodeQuery } from "$lib/api/queries/forward-geocode.query";
	import {
		getEventApiEventId,
		createPutEventApiEventIdEdit,
		createPostEventApiEventIdWindowCreate,
		createPutEventApiEventIdWindowWindowIdEdit,
		createDeleteEventApiEventIdWindowWindowId,
		EventTag,
		type PutEditEventBody,
	} from "$lib/api/cartesian-client";
	import { fetchReverseGeocode } from "$lib/api/queries/reverse-geocode.query";
	import { createQuery, useQueryClient } from "@tanstack/svelte-query";
	import mapboxgl from "mapbox-gl";
	import { Debounced } from "runed";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { EVENT_TAG_CONFIG } from "$lib/constants/event-tags";
	import { m } from "$lib/paraglide/messages";
	import { fly, slide, fade } from "svelte/transition";
	import DateTimePicker from "./date-time-picker.svelte";
	import { Effect } from "effect";
	import { Completion } from "@ai-sdk/svelte";

	interface Props {
		map: mapboxgl.Map;
	}

	let { map }: Props = $props();

	let overlayContainer = $state<HTMLDivElement | null>(null);

	let open = $state<boolean>(editEventOverlayState.open);
	let searchQuery = $state("");
	let isSearching = $state<boolean>(false);
	let cancelDialogOpen = $state(false);
	let isGlowing = $state(false);
	let submitError = $state<string | null>(null);
	const debouncedSearch = new Debounced<string>(() => searchQuery, 300);

	type EventWindowInput = {
		id: string;
		title: string;
		description: string;
		startTime: string;
		endTime: string;
		isNew?: boolean;
	};

	let eventWindows = $state<EventWindowInput[]>([]);
	let deletedWindowIds = $state<string[]>([]);
	let tagSearchQuery = $state("");
	let isInitialized = $state(false);
	let currentEventId = $state<string | null>(null);

	const queryClient = useQueryClient();
	const searchResults = createForwardGeocodeQuery(() => debouncedSearch.current);

	const eventQuery = createQuery(() => ({
		queryKey: ["getEventApiEventId", editEventOverlayState.eventId],
		queryFn: ({ signal }) => getEventApiEventId(editEventOverlayState.eventId!, signal),
		enabled: !!editEventOverlayState.eventId && editEventOverlayState.open,
	}));

	const locationQuery = createQuery(() => ({
		queryKey: [
			"reverse-geocode",
			editEventOverlayState.location?.lng,
			editEventOverlayState.location?.lat,
		],
		queryFn: () => {
			if (!editEventOverlayState.location) throw new Error("No location");
			return Effect.runPromise(
				fetchReverseGeocode(
					editEventOverlayState.location.lng,
					editEventOverlayState.location.lat,
				),
			);
		},
		enabled: !!editEventOverlayState.location,
		staleTime: 1000 * 60 * 60,
	}));

	$effect(() => {
		open = editEventOverlayState.open;
		if (open) {
			submitError = null;
			if (editEventOverlayState.eventId !== currentEventId) {
				isInitialized = false;
				currentEventId = editEventOverlayState.eventId;
				eventWindows = [];
				deletedWindowIds = [];
				form.reset();
			}
		} else {
			currentEventId = null;
			isInitialized = false;
			deletedWindowIds = [];
		}
	});

	$effect(() => {
		const data = eventQuery.data;
		if (data && !isInitialized && open && editEventOverlayState.eventId) {
			$formData.name = data.name;
			$formData.description = data.description;
			$formData.tags = data.tags as EventTag[];

			if (data.windows) {
				eventWindows = data.windows.map((w) => ({
					id: w.id,
					title: w.title ?? "",
					description: w.description ?? "",
					startTime: w.startTime
						? new Date(w.startTime as string).toISOString().slice(0, 16)
						: "",
					endTime: w.endTime
						? new Date(w.endTime as string).toISOString().slice(0, 16)
						: "",
					isNew: false,
				}));
			}

			const location = data.location as unknown as
				| { coordinates: [number, number] }
				| undefined;
			if (location?.coordinates) {
				editEventOverlayState.location = {
					lng: location.coordinates[0],
					lat: location.coordinates[1],
				};
			}

			isInitialized = true;
		}
	});

	$effect(() => {
		if (open) {
			if (overlayContainer) {
				animate(
					overlayContainer,
					{
						opacity: [0, 1],
						x: [20, 0],
						scale: [0.95, 1],
						filter: ["blur(8px)", "blur(0px)"],
					},
					{
						duration: 0.5,
						ease: [0.16, 1, 0.3, 1],
					},
				);
			}
		} else {
			if (overlayContainer) {
				animate(
					overlayContainer,
					{
						opacity: [1, 0],
						scale: [1, 0.95],
						filter: ["blur(0px)", "blur(8px)"],
					},
					{
						duration: 0.3,
						ease: [0.16, 1, 0.3, 1],
					},
				);
			}
		}
	});

	let marker: mapboxgl.Marker | null = null;

	$effect(() => {
		if (editEventOverlayState.location && open) {
			if (!marker) {
				const el = document.createElement("div");
				el.className = "flex flex-col items-center gap-2 transition-transform";
				el.innerHTML = `
					<div class="relative flex h-10 w-10 items-center justify-center rounded-full bg-primary shadow-lg shadow-primary/30 ring-4 ring-background">
						<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-primary-foreground"><path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z"/><circle cx="12" cy="10" r="3"/></svg>
					</div>
				`;
				marker = new mapboxgl.Marker({ element: el, anchor: "bottom" })
					.setLngLat([
						editEventOverlayState.location.lng,
						editEventOverlayState.location.lat,
					])
					.addTo(map);
			} else {
				marker.setLngLat([
					editEventOverlayState.location.lng,
					editEventOverlayState.location.lat,
				]);
			}
		} else {
			if (marker) {
				marker.remove();
				marker = null;
			}
		}
	});

	$effect(() => {
		return () => {
			if (marker) {
				marker.remove();
			}
		};
	});

	const tagValues = Object.values(EventTag) as EventTag[];

	const filteredTags = $derived(
		tagValues.filter((tag) => {
			const config = EVENT_TAG_CONFIG[tag];
			const name = config ? m[config.translationKey as keyof typeof m]() : tag;
			return name.toLowerCase().includes(tagSearchQuery.toLowerCase());
		}),
	);

	const formSchema = z.object({
		name: z.string().min(1, "Name is required"),
		description: z.string().min(1, "Description is required"),
		tags: z.array(z.enum(tagValues)).min(1, "At least one tag is required"),
	});

	const editEventMutation = createPutEventApiEventIdEdit();
	const createWindowMutation = createPostEventApiEventIdWindowCreate();
	const editWindowMutation = createPutEventApiEventIdWindowWindowIdEdit();
	const deleteWindowMutation = createDeleteEventApiEventIdWindowWindowId();

	const initialData: z.infer<typeof formSchema> = {
		name: "",
		description: "",
		tags: [],
	};

	const form = superForm(defaults(initialData, zod4(formSchema)), {
		SPA: true,
		resetForm: false,
		validators: zod4(formSchema),
		onUpdate: async ({ form: f }) => {
			if (f.valid && editEventOverlayState.eventId) {
				submitError = null;
				try {
					if (!editEventOverlayState.location) {
						throw new Error("Location is required");
					}

					const editBody: PutEditEventBody = {
						name: f.data.name,
						description: f.data.description,
						tags: f.data.tags as EventTag[],
						timing: eventQuery.data?.timing ?? null,
						visibility: eventQuery.data?.visibility ?? null,
						location: {
							type: "Point",
							coordinates: [
								editEventOverlayState.location.lng,
								editEventOverlayState.location.lat,
							],
						} as any,
					};

					await editEventMutation.mutateAsync({
						eventId: editEventOverlayState.eventId,
						data: editBody,
					});

					for (const windowId of deletedWindowIds) {
						await deleteWindowMutation.mutateAsync({
							eventId: editEventOverlayState.eventId,
							windowId,
						});
					}

					for (const window of eventWindows) {
						if (!window.startTime || !window.endTime) {
							toast.error("All event windows must have a start and end time.");
							return;
						}

						if (window.isNew) {
							await createWindowMutation.mutateAsync({
								eventId: editEventOverlayState.eventId,
								data: {
									title: window.title,
									description: window.description,
									startTime: new Date(window.startTime).toISOString(),
									endTime: new Date(window.endTime).toISOString(),
								},
							});
						} else {
							await editWindowMutation.mutateAsync({
								eventId: editEventOverlayState.eventId,
								windowId: window.id,
								data: {
									title: window.title,
									description: window.description,
									startTime: new Date(window.startTime).toISOString(),
									endTime: new Date(window.endTime).toISOString(),
								},
							});
						}
					}

					toast.success("Event updated successfully");
					deletedWindowIds = [];
					await queryClient.invalidateQueries({
						queryKey: ["getEventApiEventId", editEventOverlayState.eventId],
					});
					await queryClient.invalidateQueries({ queryKey: ["getEventApiMy"] });
					await queryClient.invalidateQueries({ queryKey: ["getEventApiList"] });
					await queryClient.invalidateQueries({ queryKey: ["/event/api/geojson"] });
					editEventOverlayState.open = false;
					editEventOverlayState.eventId = null;
					editEventOverlayState.location = null;
					isInitialized = false;
				} catch (e) {
					console.error("Failed to update event", e);
					if (e instanceof Error) {
						submitError = e.message
							.replace(/^\(FiberFailure\) /, "")
							.replace(/^FetchError: /, "")
							.trim();
					} else {
						submitError = "An unknown error occurred";
					}
				}
			}
		},
	});

	const { form: formData, enhance } = form;

	const aiEnhancer = new Completion({
		api: "/api/ai/enhance-description",
		streamProtocol: "text",
	});

	$effect(() => {
		if (aiEnhancer.loading) {
			$formData.description = aiEnhancer.completion;
		}
	});

	function addEventWindow() {
		eventWindows = [
			...eventWindows,
			{
				id: crypto.randomUUID(),
				title: "",
				description: "",
				startTime: "",
				endTime: "",
				isNew: true,
			},
		];
	}

	function removeEventWindow(id: string, isNew?: boolean) {
		if (!isNew) {
			deletedWindowIds = [...deletedWindowIds, id];
		}
		eventWindows = eventWindows.filter((w) => w.id !== id);
	}

	function closeOverlay() {
		editEventOverlayState.open = false;
		editEventOverlayState.eventId = null;
		editEventOverlayState.location = null;
		isInitialized = false;
	}
</script>

<div
	bind:this={overlayContainer}
	class={cn(
		"pointer-events-none absolute top-4 right-4 bottom-4 z-50 flex w-full max-w-lg origin-top-right flex-col",
		open ? "" : "",
	)}
>
	<div
		class={cn(
			"flex h-full flex-col overflow-hidden rounded-3xl border border-border/40 bg-background shadow-2xl transition-all",
			open ? "pointer-events-auto" : "pointer-events-none",
		)}
	>
		<!-- Header -->
		<div class="relative flex items-center justify-between px-6 py-5">
			<div class="space-y-0.5">
				<h2 class="text-xl font-semibold tracking-tight">Edit Event</h2>
				<p class="text-xs font-medium text-muted-foreground">Update your event details</p>
			</div>
			<Button
				variant="ghost"
				size="icon"
				class="h-8 w-8 rounded-full transition-colors hover:bg-muted"
				onclick={closeOverlay}
			>
				<HugeiconsIcon icon={Cancel01Icon} size={16} strokeWidth={2} />
				<span class="sr-only">Close</span>
			</Button>
		</div>

		<!-- Content -->
		<div
			class="scrollbar-thin scrollbar-thumb-border scrollbar-track-transparent flex-1 overflow-y-auto px-6 pb-6"
		>
			{#if eventQuery.isLoading}
				<div class="flex h-full items-center justify-center">
					<HugeiconsIcon
						icon={Loading03Icon}
						size={32}
						className="animate-spin text-muted-foreground"
					/>
				</div>
			{:else}
				<form method="POST" use:enhance class="space-y-8" id="edit-event-form">
					<!-- Basic Info -->
					<div class="space-y-3">
						<Label
							class="text-xs font-semibold tracking-wider text-muted-foreground uppercase"
						>
							Details
						</Label>

						<Form.Field {form} name="name" class="space-y-1.5">
							<Form.Control>
								{#snippet children({ props })}
									<Form.Label class="ml-1 text-xs text-muted-foreground"
										>Event Title</Form.Label
									>
									<div class="relative">
										<Input
											{...props}
											bind:value={$formData.name}
											placeholder="What are you planning?"
											class="h-10 border-border/50 bg-muted/30 px-3 font-medium shadow-none transition-all focus-visible:bg-background focus-visible:ring-1"
										/>
									</div>
								{/snippet}
							</Form.Control>
							<Form.FieldErrors />
						</Form.Field>

						<div class="relative">
							<Form.Field {form} name="description" class="space-y-1.5">
								<Form.Control>
									{#snippet children({ props })}
										<Form.Label class="ml-1 text-xs text-muted-foreground"
											>Description</Form.Label
										>
										<Textarea
											{...props}
											bind:value={$formData.description}
											placeholder="Add details about your event..."
											rows={4}
											disabled={aiEnhancer.loading}
											class="resize-none border-border/50 bg-muted/30 px-3 py-2 shadow-none transition-all focus-visible:bg-background focus-visible:ring-1"
										/>
									{/snippet}
								</Form.Control>
								<Form.FieldErrors />
							</Form.Field>

							<!-- AI Enhance Button -->
							<div class="mt-2 flex justify-end">
								<Button
									variant="outline"
									size="sm"
									disabled={aiEnhancer.loading || !$formData.description}
									onclick={() => aiEnhancer.complete($formData.description)}
									class={cn(
										"group relative overflow-hidden rounded-full border border-primary/20 bg-background/50 pr-4 pl-3 text-xs font-medium transition-all hover:border-primary/40 hover:bg-background hover:shadow-sm",
										(isGlowing || aiEnhancer.loading) &&
											"border-primary/50 shadow-[0_0_15px_rgba(59,130,246,0.3)]",
									)}
									onmouseenter={() => (isGlowing = true)}
									onmouseleave={() => (isGlowing = false)}
								>
									{#if isGlowing || aiEnhancer.loading}
										<div
											class="absolute inset-0 bg-gradient-to-r from-transparent via-primary/10 to-transparent"
											transition:fade
										></div>
									{/if}
									{#if aiEnhancer.loading}
										<HugeiconsIcon
											icon={Loading03Icon}
											size={14}
											strokeWidth={2}
											className="mr-2 animate-spin text-primary"
										/>
									{:else}
										<HugeiconsIcon
											icon={AiEditingIcon}
											size={14}
											strokeWidth={2}
											className="mr-2 text-primary"
										/>
									{/if}
									<span
										class="bg-gradient-to-r from-foreground to-foreground/70 bg-clip-text text-transparent transition-all group-hover:to-primary"
									>
										Enhance with AI
									</span>
								</Button>
							</div>
						</div>
					</div>

					<!-- Location Section -->
					<div class="space-y-3">
						<div class="flex items-center justify-between">
							<Label
								class="text-xs font-semibold tracking-wider text-muted-foreground uppercase"
							>
								Location
							</Label>
							{#if !editEventOverlayState.location}
								<span
									class="flex items-center gap-1 text-[10px] font-medium text-destructive"
								>
									<HugeiconsIcon
										icon={Location01Icon}
										size={12}
										strokeWidth={2}
									/>
									Required
								</span>
							{/if}
						</div>

						{#if editEventOverlayState.location}
							<div in:slide={{ duration: 200 }}>
								<div
									class="group relative overflow-hidden rounded-2xl border border-border/50 bg-gradient-to-b from-muted/50 to-muted/20 p-4 transition-all hover:border-border hover:from-muted/60 hover:to-muted/30"
								>
									<div class="flex items-start gap-4">
										<div
											class="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-background shadow-sm ring-1 ring-black/5"
										>
											<HugeiconsIcon
												icon={Location01Icon}
												size={20}
												strokeWidth={2}
												className="text-primary"
											/>
										</div>
										<div class="flex-1 space-y-1 overflow-hidden">
											<p
												class="truncate leading-none font-semibold tracking-tight"
											>
												{#if locationQuery?.isPending}
													<span class="animate-pulse">Locating...</span>
												{:else if locationQuery?.data?.features?.[0]?.properties}
													{locationQuery.data.features[0].properties
														.name ??
														locationQuery.data.features[0].properties
															.place_formatted}
												{:else}
													Selected Coordinates
												{/if}
											</p>
											<p class="truncate text-xs text-muted-foreground">
												{editEventOverlayState.location.lat.toFixed(6)}, {editEventOverlayState.location.lng.toFixed(
													6,
												)}
											</p>
										</div>
										<Button
											variant="ghost"
											size="icon"
											class="h-8 w-8 rounded-full text-muted-foreground hover:bg-background hover:text-destructive hover:shadow-sm"
											onclick={() => {
												editEventOverlayState.location = null;
											}}
										>
											<HugeiconsIcon
												icon={Cancel01Icon}
												size={16}
												strokeWidth={2}
											/>
										</Button>
									</div>
								</div>
							</div>
						{:else}
							<div class="space-y-3" in:slide={{ duration: 200 }}>
								<div class="relative">
									<HugeiconsIcon
										icon={Search01Icon}
										size={16}
										strokeWidth={2}
										className="absolute left-3 top-3 text-muted-foreground"
									/>
									<Input
										placeholder="Search location..."
										class="h-10 border-border/50 bg-muted/30 pl-9 shadow-none focus-visible:bg-background focus-visible:ring-1"
										bind:value={searchQuery}
										onfocus={() => (isSearching = true)}
										onblur={() => setTimeout(() => (isSearching = false), 200)}
									/>
									{#if searchResults.isPending && debouncedSearch.current.length >= 3}
										<div class="absolute top-3 right-3">
											<HugeiconsIcon
												icon={Loading03Icon}
												size={16}
												strokeWidth={2}
												className="animate-spin text-muted-foreground"
											/>
										</div>
									{/if}

									{#if isSearching && searchResults.data?.features?.length}
										<div
											class="absolute top-full right-0 left-0 z-20 mt-2 max-h-[240px] origin-top overflow-auto rounded-xl border border-border/50 bg-background/95 p-1 shadow-xl backdrop-blur-md"
											transition:fly={{ y: 10, duration: 200 }}
										>
											{#each searchResults.data.features as feature}
												<button
													class="flex w-full flex-col items-start rounded-lg px-3 py-2.5 text-left text-sm transition-colors hover:bg-muted"
													onclick={() => {
														const [lng, lat] =
															feature.geometry.coordinates;
														editEventOverlayState.location = {
															lat,
															lng,
														};
														map.flyTo({ center: [lng, lat], zoom: 14 });
														searchQuery = "";
														isSearching = false;
													}}
												>
													<span class="font-medium"
														>{feature.properties.name ??
															feature.properties.place_formatted?.split(
																",",
															)[0]}</span
													>
													<span
														class="line-clamp-1 text-xs text-muted-foreground"
														>{feature.properties.full_address ??
															feature.properties
																.place_formatted}</span
													>
												</button>
											{/each}
										</div>
									{/if}
								</div>

								<div class="grid grid-cols-2 gap-3">
									<Button
										variant="outline"
										class="h-9 border-border/50 bg-transparent hover:bg-muted/50"
										onclick={() => {
											if (navigator.geolocation) {
												navigator.geolocation.getCurrentPosition(
													(position) => {
														const { latitude, longitude } =
															position.coords;
														editEventOverlayState.location = {
															lat: latitude,
															lng: longitude,
														};
														map.flyTo({
															center: [longitude, latitude],
															zoom: 14,
														});
													},
													(error) => {
														console.error(
															"Error getting location",
															error,
														);
													},
												);
											}
										}}
									>
										<HugeiconsIcon
											icon={Gps01Icon}
											size={14}
											strokeWidth={2}
											className="mr-2"
										/>
										<span class="text-xs">Use Current</span>
									</Button>
									<Button
										variant="outline"
										class="h-9 border-border/50 bg-transparent hover:bg-muted/50"
										onclick={() => {
											editEventOverlayState.open = false;
										}}
									>
										<HugeiconsIcon
											icon={MapsLocation01Icon}
											size={14}
											strokeWidth={2}
											className="mr-2"
										/>
										<span class="text-xs">Pick on Map</span>
									</Button>
								</div>
							</div>
						{/if}
					</div>

					<!-- Schedule Section -->
					<div class="space-y-3">
						<div class="flex items-center justify-between">
							<Label
								class="text-xs font-semibold tracking-wider text-muted-foreground uppercase"
							>
								Schedule
							</Label>
							<Button
								variant="ghost"
								size="sm"
								class="h-6 px-2 text-[10px] text-primary hover:bg-primary/10 hover:text-primary"
								onclick={addEventWindow}
							>
								<HugeiconsIcon
									icon={Add01Icon}
									size={12}
									strokeWidth={2}
									className="mr-1"
								/>
								Add Window
							</Button>
						</div>

						<div class="space-y-3">
							{#each eventWindows as window, index (window.id)}
								<div
									class="relative overflow-hidden rounded-2xl border border-border/50 bg-card p-4 shadow-sm transition-all hover:border-border hover:shadow-md"
									transition:slide
								>
									<div class="mb-3 flex items-center justify-between">
										<span
											class="flex items-center gap-2 text-xs font-semibold tracking-wider text-muted-foreground uppercase"
										>
											<div class="h-1.5 w-1.5 rounded-full bg-primary"></div>
											Window {index + 1}
											{#if window.isNew}
												<span class="text-[10px] text-primary">(new)</span>
											{/if}
										</span>
										{#if eventWindows.length > 1}
											<Button
												variant="ghost"
												size="icon"
												class="h-6 w-6 text-muted-foreground hover:text-destructive"
												onclick={() =>
													removeEventWindow(window.id, window.isNew)}
												disabled={deleteWindowMutation.isPending}
											>
												<HugeiconsIcon
													icon={Delete02Icon}
													size={14}
													strokeWidth={2}
												/>
											</Button>
										{/if}
									</div>

									<div class="grid gap-4">
										<div class="space-y-3">
											<div class="relative">
												<Input
													bind:value={window.title}
													placeholder="Session Title"
													class="h-10 border-border/50 bg-muted/30 px-3 font-medium shadow-none focus-visible:bg-background focus-visible:ring-1"
												/>
											</div>
											<div class="relative">
												<Input
													bind:value={window.description}
													placeholder="Optional description"
													class="h-10 border-border/50 bg-muted/30 px-3 text-sm text-muted-foreground shadow-none focus-visible:bg-background focus-visible:ring-1"
												/>
											</div>
										</div>

										<div
											class="grid grid-cols-2 gap-4 rounded-lg bg-muted/30 p-3"
										>
											<div class="space-y-1">
												<Label class="text-[10px] text-muted-foreground"
													>Start Time</Label
												>
												<DateTimePicker
													bind:value={window.startTime}
													placeholder="Pick start time"
												/>
											</div>
											<div class="space-y-1">
												<Label class="text-[10px] text-muted-foreground"
													>End Time</Label
												>
												<DateTimePicker
													bind:value={window.endTime}
													placeholder="Pick end time"
												/>
											</div>
										</div>
									</div>
								</div>
							{/each}
							{#if eventWindows.length === 0}
								<Button
									variant="outline"
									class="w-full border-dashed"
									onclick={addEventWindow}
								>
									<HugeiconsIcon
										icon={Add01Icon}
										size={16}
										strokeWidth={2}
										className="mr-2"
									/> Add Schedule
								</Button>
							{/if}
						</div>
					</div>

					<!-- Tags Section -->
					<div class="space-y-3">
						<div class="flex items-center justify-between">
							<Label
								class="text-xs font-semibold tracking-wider text-muted-foreground uppercase"
							>
								Category
							</Label>
						</div>
						<Form.Field {form} name="tags">
							<Form.Control>
								{#snippet children({})}
									<div class="space-y-3">
										<div class="relative">
											<HugeiconsIcon
												icon={Search01Icon}
												size={14}
												strokeWidth={2}
												className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground"
											/>
											<Input
												placeholder="Filter categories..."
												bind:value={tagSearchQuery}
												class="h-10 border-border/50 bg-muted/30 pl-9 shadow-none focus-visible:bg-background focus-visible:ring-1"
											/>
										</div>
										<div class="flex flex-wrap gap-2">
											{#each filteredTags as tag}
												{@const isSelected = $formData.tags.includes(tag)}
												{@const config = EVENT_TAG_CONFIG[tag]}
												<button
													type="button"
													class={cn(
														"group flex items-center gap-1.5 rounded-full border px-3 py-1.5 text-xs font-medium transition-all hover:scale-105 active:scale-95",
														isSelected
															? "border-primary/20 bg-primary/10 text-primary hover:bg-primary/20"
															: "border-transparent bg-muted/50 text-muted-foreground hover:bg-muted hover:text-foreground",
													)}
													onclick={() => {
														if (isSelected) {
															$formData.tags = $formData.tags.filter(
																(t: string) => t !== tag,
															);
														} else {
															$formData.tags = [
																...$formData.tags,
																tag,
															];
														}
													}}
												>
													{#if config?.icon}
														<HugeiconsIcon
															icon={config.icon}
															size={14}
															strokeWidth={2}
															className={isSelected
																? "text-primary"
																: "text-muted-foreground/70"}
														/>
													{/if}
													{config
														? m[
																config.translationKey as keyof typeof m
															]()
														: tag}
												</button>
											{/each}
										</div>
									</div>
								{/snippet}
							</Form.Control>
							<Form.FieldErrors />
						</Form.Field>
					</div>
				</form>
			{/if}
		</div>

		<!-- Footer -->
		<div class="border-t border-border/30 bg-muted/10 p-4">
			{#if submitError}
				<div
					class="mb-3 rounded-lg bg-destructive/10 px-3 py-2 text-xs font-medium text-destructive"
				>
					{submitError}
				</div>
			{/if}
			<div class="flex gap-3">
				<Button variant="outline" class="flex-1" onclick={() => (cancelDialogOpen = true)}>
					Cancel
				</Button>
				<Button
					class="flex-1"
					type="submit"
					form="edit-event-form"
					disabled={editEventMutation.isPending}
				>
					{editEventMutation.isPending ? "Saving..." : "Save Changes"}
				</Button>
			</div>
		</div>
	</div>
</div>

<AlertDialog.Root bind:open={cancelDialogOpen}>
	<AlertDialog.Content>
		<AlertDialog.Header>
			<AlertDialog.Title>Discard changes?</AlertDialog.Title>
			<AlertDialog.Description>
				Are you sure you want to discard your changes? This action cannot be undone.
			</AlertDialog.Description>
		</AlertDialog.Header>
		<AlertDialog.Footer>
			<AlertDialog.Cancel>Keep Editing</AlertDialog.Cancel>
			<AlertDialog.Action
				class="bg-destructive text-destructive-foreground hover:bg-destructive/90"
				onclick={() => {
					cancelDialogOpen = false;
					closeOverlay();
				}}
			>
				Discard
			</AlertDialog.Action>
		</AlertDialog.Footer>
	</AlertDialog.Content>
</AlertDialog.Root>
