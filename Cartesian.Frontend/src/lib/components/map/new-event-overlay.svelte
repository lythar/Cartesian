<script lang="ts">
	import { cn } from "$lib/utils";
	import { toast } from "svelte-sonner";
	import { animate } from "motion";
	import { newEventOverlayState } from "./map-state.svelte";
	import { Effect } from "effect";
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
		ArrowRight01Icon,
		Image01Icon,
		ImageAdd01Icon,
	} from "@hugeicons/core-free-icons";
	import { createForwardGeocodeQuery } from "$lib/api/queries/forward-geocode.query";
	import {
		createPostEventApiCreate,
		createPostEventApiEventIdWindowCreate,
		createPostEventApiEventIdImages,
		createPostEventApiWindowWindowIdImages,
		EventTag,
		type CreateEventBody,
		type CreateEventWindowBody,
	} from "$lib/api/cartesian-client";
	import { fetchReverseGeocode } from "$lib/api/queries/reverse-geocode.query";
	import { createQuery, useQueryClient } from "@tanstack/svelte-query";
	import mapboxgl from "mapbox-gl";
	import { Debounced } from "runed";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { AiEditingIcon } from "@hugeicons/core-free-icons";
	import { EVENT_TAG_CONFIG } from "$lib/constants/event-tags";
import { m } from "$lib/paraglide/messages";
import { fly, fade, slide } from "svelte/transition";
import DateTimePicker from "./date-time-picker.svelte";
import { Completion } from "@ai-sdk/svelte";	const queryClient = useQueryClient();

	interface Props {
		map: mapboxgl.Map;
	}

	let { map }: Props = $props();

	let overlayContainer = $state<HTMLDivElement | null>(null);

	let open = $state<boolean>(newEventOverlayState.open);
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
	};

	let eventWindows = $state<EventWindowInput[]>([]);
	let showSimpleMode = $state(true);
	let simpleStartTime = $state("");
	let simpleEndTime = $state("");
	let tagSearchQuery = $state("");

	let eventImages = $state<File[]>([]);
	let windowImages = $state<Record<string, File[]>>({});
	let simpleWindowImages = $state<File[]>([]);

	const searchResults = createForwardGeocodeQuery(() => debouncedSearch.current);

	const locationQuery = createQuery(() => ({
		queryKey: [
			"reverse-geocode",
			newEventOverlayState.location?.lng,
			newEventOverlayState.location?.lat,
		],
		queryFn: () => {
			if (!newEventOverlayState.location) throw new Error("No location");
			return Effect.runPromise(
				fetchReverseGeocode(
					newEventOverlayState.location.lng,
					newEventOverlayState.location.lat,
				),
			);
		},
		enabled: !!newEventOverlayState.location,
		staleTime: 1000 * 60 * 60,
	}));

	$effect(() => {
		open = newEventOverlayState.open;
		if (open) submitError = null;
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
						ease: [0.16, 1, 0.3, 1], // Spring-like ease
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

	// Update marker when location changes
	let marker: mapboxgl.Marker | null = null;

	$effect(() => {
		if (newEventOverlayState.location && open) {
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
						newEventOverlayState.location.lng,
						newEventOverlayState.location.lat,
					])
					.addTo(map);
			} else {
				marker.setLngLat([
					newEventOverlayState.location.lng,
					newEventOverlayState.location.lat,
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
		communityId: z.string().nullable().default(null),
		tags: z.array(z.enum(tagValues)).min(1, "At least one tag is required"),
	});

	const createEventMutation = createPostEventApiCreate();
	const createEventWindowMutation = createPostEventApiEventIdWindowCreate();
	const createEventImagesMutation = createPostEventApiEventIdImages();
	const createWindowImagesMutation = createPostEventApiWindowWindowIdImages();

	const initialData: z.infer<typeof formSchema> = {
		name: "",
		description: "",
		communityId: null,
		tags: [],
	};

	const form = superForm(defaults(initialData, zod4(formSchema)), {
		SPA: true,
		resetForm: false,
		validators: zod4(formSchema),
		onUpdate: async ({ form: f }) => {
			if (f.valid) {
				submitError = null;
				try {
					if (!newEventOverlayState.location) {
						throw new Error("Location is required");
					}

					let windowsToCreate: (CreateEventWindowBody & { tempId?: string })[] = [];

					if (eventWindows.length > 0) {
						for (const w of eventWindows) {
							if (!w.startTime || !w.endTime) {
								toast.error("All event windows must have a start and end time.");
								return;
							}
						}
						windowsToCreate = eventWindows.map((w) => ({
							tempId: w.id,
							title: w.title,
							description: w.description,
							startTime: new Date(w.startTime).toISOString(),
							endTime: new Date(w.endTime).toISOString(),
						}));
					} else if (showSimpleMode) {
						if (!simpleStartTime || !simpleEndTime) {
							toast.error("Please select a start and end time.");
							return;
						}
						windowsToCreate = [
							{
								title: f.data.name,
								description: f.data.description,
								startTime: new Date(simpleStartTime).toISOString(),
								endTime: new Date(simpleEndTime).toISOString(),
							},
						];
					} else {
						toast.error("Please add at least one event window.");
						return;
					}

					const eventBody: CreateEventBody & { windows: CreateEventWindowBody[] } = {
						name: f.data.name,
						description: f.data.description,
						communityId: f.data.communityId,
						tags: f.data.tags as EventTag[],
						location: {
							type: "Point",
							coordinates: [
								newEventOverlayState.location.lng,
								newEventOverlayState.location.lat,
							],
						} as any,
						windows: windowsToCreate.map(({ tempId, ...rest }) => rest),
					};

					const event = await createEventMutation.mutateAsync({ data: eventBody });

					if (eventImages.length > 0) {
						const uploadEffects = eventImages.map((file) =>
							Effect.tryPromise({
								try: () =>
									createEventImagesMutation.mutateAsync({
										eventId: event.id,
										data: { file },
									}),
								catch: (error) => error,
							}),
						);
						await Effect.runPromise(Effect.all(uploadEffects, { concurrency: 3 }));
					}

					if (event.windows && event.windows.length > 0) {
						const imageEffects: Effect.Effect<any, unknown, never>[] = [];

						event.windows.forEach((window, index) => {
							const originalWindow = windowsToCreate[index];
							const images = originalWindow.tempId
								? windowImages[originalWindow.tempId] || []
								: simpleWindowImages;

							if (images.length > 0) {
								images.forEach((file) => {
									imageEffects.push(
										Effect.tryPromise({
											try: () =>
												createWindowImagesMutation.mutateAsync({
													windowId: window.id,
													data: { file },
												}),
											catch: (error) => error,
										}),
									);
								});
							}
						});

						if (imageEffects.length > 0) {
							await Effect.runPromise(Effect.all(imageEffects, { concurrency: 3 }));
						}
					}

					await queryClient.invalidateQueries({ queryKey: ["/event/api/geojson"] });
					await queryClient.invalidateQueries({ queryKey: ["getEventApiMy"] });
					await queryClient.invalidateQueries({ queryKey: ["getEventApiList"] });

					toast.success("Event published successfully");
					newEventOverlayState.open = false;
					newEventOverlayState.location = null;
					f.data = initialData;
					eventWindows = [];
					eventImages = [];
					windowImages = {};
					simpleWindowImages = [];
					showSimpleMode = true;
					simpleStartTime = "";
					simpleEndTime = "";
					form.reset();
				} catch (e) {
					console.error("Failed to create event", e);
					if (e instanceof Error) {
						// Clean up Effect error prefixes to show the actual message
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
		if (showSimpleMode && simpleStartTime && simpleEndTime) {
			// Move simple mode data to first window
			eventWindows = [
				{
					id: crypto.randomUUID(),
					title: $formData.name || "Event Window 1",
					description: $formData.description || "",
					startTime: simpleStartTime,
					endTime: simpleEndTime,
				},
			];
			simpleStartTime = "";
			simpleEndTime = "";
			showSimpleMode = false;
		} else {
			eventWindows = [
				...eventWindows,
				{
					id: crypto.randomUUID(),
					title: "",
					description: "",
					startTime: "",
					endTime: "",
				},
			];
		}
	}

	function removeEventWindow(id: string) {
		eventWindows = eventWindows.filter((w) => w.id !== id);
		if (eventWindows.length === 0) {
			showSimpleMode = true;
		}
	}

	function validateAndAddImages(
		currentFiles: File[],
		newFiles: File[],
		onUpdate: (files: File[]) => void,
	) {
		const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
		const MAX_FILES = 10;

		const schema = z
			.array(
				z
					.instanceof(File)
					.refine((file) => file.size <= MAX_FILE_SIZE, "Max image size is 5MB.")
					.refine(
						(file) => file.type.startsWith("image/"),
						"Only image files are allowed.",
					),
			)
			.max(MAX_FILES, `You can only upload up to ${MAX_FILES} images.`);

		const combined = [...currentFiles, ...newFiles];
		const result = schema.safeParse(combined);

		if (result.success) {
			onUpdate(result.data);
		} else {
			const firstIssue = result.error.issues[0];
			toast.error(firstIssue.message);
		}
	}
</script>

{#snippet imagePicker(files: File[], onFilesChange: (files: File[]) => void, id: string)}
	<div class="space-y-3">
		<div class="flex flex-wrap gap-2">
			{#each files as file, i}
				<div
					class="group relative h-20 w-20 overflow-hidden rounded-lg border border-border/50 bg-muted/30"
				>
					<img
						src={URL.createObjectURL(file)}
						alt="Preview"
						class="h-full w-full object-cover transition-transform group-hover:scale-105"
						onload={(e) =>
							URL.revokeObjectURL((e.currentTarget as HTMLImageElement).src)}
					/>
					<button
						type="button"
						class="absolute top-1 right-1 rounded-full bg-background/80 p-1 text-muted-foreground opacity-0 backdrop-blur-sm transition-opacity group-hover:opacity-100 hover:bg-destructive hover:text-destructive-foreground"
						onclick={() => {
							const newFiles = [...files];
							newFiles.splice(i, 1);
							onFilesChange(newFiles);
						}}
					>
						<HugeiconsIcon icon={Delete02Icon} size={12} strokeWidth={2} />
					</button>
				</div>
			{/each}
			<label
				for={id}
				class="flex h-20 w-20 cursor-pointer flex-col items-center justify-center gap-1 rounded-lg border border-dashed border-border/50 bg-muted/10 text-muted-foreground transition-colors hover:border-primary/50 hover:bg-primary/5 hover:text-primary"
			>
				<HugeiconsIcon icon={ImageAdd01Icon} size={20} strokeWidth={2} />
				<span class="text-[10px] font-medium">Add</span>
				<input
					{id}
					type="file"
					accept="image/*"
					multiple
					class="hidden"
					onchange={(e) => {
						const input = e.currentTarget;
						if (input.files) {
							validateAndAddImages(files, Array.from(input.files), onFilesChange);
							input.value = "";
						}
					}}
				/>
			</label>
		</div>
	</div>
{/snippet}

<div
	bind:this={overlayContainer}
	class={cn(
		"pointer-events-none overflow-hidden absolute inset-0 lg:left-auto lg:top-4 lg:right-4 lg:bottom-4 z-50 flex w-full max-w-full lg:max-w-lg origin-top-right flex-col",
		open ? "" : "",
	)}
>
	<div
		class={cn(
			"flex h-full flex-col overflow-hidden lg:rounded-3xl border border-border/40 bg-background shadow-2xl transition-all",
			open ? "pointer-events-auto " : "pointer-events-none",
		)}
	>
		<!-- Header -->
		<div class="relative flex items-center justify-between px-6 py-5">
			<div class="space-y-0.5">
				<h2 class="text-xl font-semibold tracking-tight">New Event</h2>
				<p class="text-xs font-medium text-muted-foreground">Create a public gathering</p>
			</div>
			<Button
				variant="ghost"
				size="icon"
				class="h-8 w-8 rounded-full transition-colors hover:bg-muted"
				onclick={() => {
					newEventOverlayState.open = false;
				}}
			>
				<HugeiconsIcon icon={Cancel01Icon} size={16} strokeWidth={2} />
				<span class="sr-only">Close</span>
			</Button>
		</div>

		<!-- Content -->
		<div
			class="scrollbar-thin scrollbar-thumb-border scrollbar-track-transparent flex-1 overflow-y-auto px-6 pb-6"
		>
			<form method="POST" use:enhance class="space-y-8" id="create-event-form">
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

						<!-- AI Enhance Button overlaying bottom right of textarea context -->
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

					<div class="space-y-1.5">
						<Label class="ml-1 text-xs text-muted-foreground">Images</Label>
						{@render imagePicker(eventImages, (f) => (eventImages = f), "event-images")}
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
						{#if !newEventOverlayState.location}
							<span
								class="flex items-center gap-1 text-[10px] font-medium text-destructive"
							>
								<HugeiconsIcon icon={Location01Icon} size={12} strokeWidth={2} />
								Required
							</span>
						{/if}
					</div>

					{#if newEventOverlayState.location}
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
												{locationQuery.data.features[0].properties.name ??
													locationQuery.data.features[0].properties
														.place_formatted}
											{:else}
												Selected Coordinates
											{/if}
										</p>
										<p class="truncate text-xs text-muted-foreground">
											{newEventOverlayState.location.lat.toFixed(6)}, {newEventOverlayState.location.lng.toFixed(
												6,
											)}
										</p>
									</div>
									<Button
										variant="ghost"
										size="icon"
										class="h-8 w-8 rounded-full text-muted-foreground hover:bg-background hover:text-destructive hover:shadow-sm"
										onclick={() => {
											newEventOverlayState.location = null;
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
													const [lng, lat] = feature.geometry.coordinates;
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
														feature.properties.place_formatted}</span
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
													const { latitude, longitude } = position.coords;
													newEventOverlayState.location = {
														lat: latitude,
														lng: longitude,
													};
													map.flyTo({
														center: [longitude, latitude],
														zoom: 14,
													});
												},
												(error) => {
													console.error("Error getting location", error);
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
										newEventOverlayState.open = false;
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
						{#if eventWindows.length > 0}
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
						{/if}
					</div>

					{#if showSimpleMode && eventWindows.length === 0}
						<div
							class="rounded-2xl border border-border/50 bg-muted/20 p-1"
							in:slide={{ duration: 200 }}
						>
							<div class="grid grid-cols-2 gap-px overflow-hidden rounded-xl">
								<div
									class="group relative bg-background/50 p-3 transition-colors focus-within:bg-background hover:bg-background/80"
								>
									<Label
										class="mb-1.5 flex items-center gap-1.5 text-[10px] text-muted-foreground"
									>
										<HugeiconsIcon
											icon={Calendar03Icon}
											size={12}
											strokeWidth={2}
										/> Starts
									</Label>
									<DateTimePicker
										bind:value={simpleStartTime}
										placeholder="Pick start time"
										class="w-full"
									/>
								</div>
								<div
									class="group relative bg-background/50 p-3 transition-colors focus-within:bg-background hover:bg-background/80"
								>
									<Label
										class="mb-1.5 flex items-center gap-1.5 text-[10px] text-muted-foreground"
									>
										<HugeiconsIcon
											icon={Clock01Icon}
											size={12}
											strokeWidth={2}
										/> Ends
									</Label>
									<DateTimePicker
										bind:value={simpleEndTime}
										placeholder="Pick end time"
										class="w-full"
									/>
								</div>
							</div>
							{#if simpleStartTime || simpleEndTime}
								<button
									class="mt-1 flex w-full items-center justify-center gap-1 rounded-lg py-1.5 text-[10px] font-medium text-muted-foreground transition-colors hover:bg-muted/50 hover:text-foreground"
									onclick={addEventWindow}
								>
									<HugeiconsIcon icon={Add01Icon} size={12} strokeWidth={2} />
									Add multiple times
								</button>
							{/if}

							<div class="mt-3 border-t border-border/50 pt-3">
								<Label class="mb-2 block text-[10px] text-muted-foreground"
									>Window Images</Label
								>
								{@render imagePicker(
									simpleWindowImages,
									(f) => (simpleWindowImages = f),
									"simple-window-images",
								)}
							</div>
						</div>
					{:else}
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
										</span>
										<Button
											variant="ghost"
											size="icon"
											class="h-6 w-6 text-muted-foreground hover:text-destructive"
											onclick={() => removeEventWindow(window.id)}
										>
											<HugeiconsIcon
												icon={Delete02Icon}
												size={14}
												strokeWidth={2}
											/>
										</Button>
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

										<div>
											<Label
												class="mb-2 block text-[10px] text-muted-foreground"
												>Window Images</Label
											>
											{@render imagePicker(
												windowImages[window.id] || [],
												(f) => {
													windowImages[window.id] = f;
												},
												`window-images-${window.id}`,
											)}
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
									/> Create Schedule
								</Button>
							{/if}
						</div>
					{/if}
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
														$formData.tags = [...$formData.tags, tag];
													}
												}}
											>
												{#if config?.icon}
													<HugeiconsIcon
														icon={config.icon}
														size={14}
														strokeWidth={2}
														className={cn(
															"transition-colors",
															isSelected
																? "text-primary"
																: "text-muted-foreground/70",
														)}
													/>
												{/if}
												{config
													? m[config.translationKey as keyof typeof m]()
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
		</div>

		<!-- Footer -->
		<div class="border-t border-border/10 bg-background/40 p-6 backdrop-blur-sm">
			{#if submitError}
				<div
					class="mb-4 rounded-lg bg-destructive/10 p-3 text-sm font-medium text-destructive"
				>
					{submitError}
				</div>
			{/if}
			<div class="flex items-center gap-3">
				<Button
					variant="ghost"
					class="flex-1 text-muted-foreground hover:text-foreground"
					onclick={() => {
						cancelDialogOpen = true;
					}}
				>
					Cancel
				</Button>
				<Button
					type="submit"
					form="create-event-form"
					class="flex-2 rounded-xl bg-primary font-semibold shadow-lg shadow-primary/25 transition-all hover:shadow-primary/40 active:scale-[0.98]"
					disabled={createEventMutation.isPending ||
						createEventWindowMutation.isPending ||
						createEventImagesMutation.isPending ||
						createWindowImagesMutation.isPending}
				>
					{#if createEventMutation.isPending || createEventWindowMutation.isPending || createEventImagesMutation.isPending || createWindowImagesMutation.isPending}
						<HugeiconsIcon
							icon={Loading03Icon}
							size={16}
							strokeWidth={2}
							className="mr-2 animate-spin"
						/>
						Creating...
					{:else}
						<span class="mr-2">Publish Event</span>
						<HugeiconsIcon
							icon={ArrowRight01Icon}
							size={16}
							strokeWidth={2}
							className="opacity-50"
						/>
					{/if}
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
					form.reset();
					newEventOverlayState.open = false;
					newEventOverlayState.location = null;
					eventWindows = [];
					showSimpleMode = true;
					simpleStartTime = "";
					simpleEndTime = "";
					cancelDialogOpen = false;
				}}>Discard</AlertDialog.Action
			>
		</AlertDialog.Footer>
	</AlertDialog.Content>
</AlertDialog.Root>
