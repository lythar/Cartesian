<script lang="ts">
	import { cn, type LatLng } from "$lib/utils";
	import { animate, stagger } from "motion";
	import { newEventOverlayState } from "./map-state.svelte";
	import Button from "../ui/button/button.svelte";
	import { Badge } from "$lib/components/ui/badge";
	import { Label } from "$lib/components/ui/label";
	import * as Form from "$lib/components/ui/form";
  import * as AlertDialog from "$lib/components/ui/alert-dialog";
  import { Input } from "$lib/components/ui/input";
	import { Textarea } from "$lib/components/ui/textarea";
	import { superForm, defaults } from "sveltekit-superforms";
	import { zod4 } from "sveltekit-superforms/adapters";
	import { z } from "zod";
	import { X, MapPin, LocateFixed, Map, Search, Loader2 } from "@lucide/svelte";
	import { createReverseGeocodeQuery } from "$lib/api/queries/reverse-geocode.query";
	import { createForwardGeocodeQuery } from "$lib/api/queries/forward-geocode.query";
	import {
		createPostEventApiCreate,
		EventTag,
		type CreateEventBody
	} from "$lib/api/cartesian-client";
  import mapboxgl from "mapbox-gl";
	import { Debounced } from "runed";

	interface Props {
		map: mapboxgl.Map;
	}

	let { map }: Props = $props();

	let overlayContainer = $state<HTMLDivElement | null>(null);

	let open = $state<boolean>(newEventOverlayState.open);
	let draftLocation: LatLng | null = $state<LatLng | null>(null);
	let searchQuery = $state("");
	let isSearching = $state<boolean>(false);
	let cancelDialogOpen = $state(false);
	const debouncedSearch = new Debounced<string>(() => searchQuery, 300);

	const locationQuery = $derived(
		$formData.latitude && $formData.longitude
			? createReverseGeocodeQuery($formData.longitude, $formData.latitude, {
					query: { enabled: true }
				})
			: null
	);

	const searchResults = createForwardGeocodeQuery(() => debouncedSearch.current);

	$effect(() => {
		open = newEventOverlayState.open;
	});

	$effect(() => {
		if (open) {
			if (newEventOverlayState.location) {
				$formData.latitude = newEventOverlayState.location.lat;
				$formData.longitude = newEventOverlayState.location.lng;
			} else {
				$formData.latitude = null;
				$formData.longitude = null;
			}
		}
	});

	$effect(() => {
		if (open) {
			if (overlayContainer) {
				animate(
					overlayContainer,
					{
						opacity: [0, 1],
						y: [-10, 0],
						scale: [0.98, 1],
						filter: ["blur(8px)", "blur(0px)"]
					},
					{
						duration: 0.4,
						ease: [0.2, 0.8, 0.2, 1]
					}
				);

				const items = overlayContainer.querySelectorAll(".animate-item");
				if (items.length) {
					animate(
						items,
						{ opacity: [0, 1], y: [10, 0], filter: ["blur(5px)", "blur(0px)"] },
						{ delay: stagger(0.05, { startDelay: 0.1 }), duration: 0.3 }
					);
				}
			}
		} else {
			if (overlayContainer) {
				animate(
					overlayContainer,
					{
						opacity: [1, 0],
						scale: [1, 0.98],
						filter: ["blur(0px)", "blur(8px)"]
					},
					{
						duration: 0.2
					}
				);
			}
		}
	});

	// Update marker when location changes
	let marker: mapboxgl.Marker | null = null;
	let markerElement: HTMLDivElement | null = null;

	$effect(() => {
		if ($formData.latitude && $formData.longitude && open) {
			if (!marker) {
				const el = document.createElement("div");
				el.className = "flex flex-col items-center gap-1";
				el.innerHTML = `
					<div class="bg-background/90 backdrop-blur-md px-2 py-1 rounded-md shadow-sm border border-border/50 text-xs font-medium whitespace-nowrap">
						Selected Location
					</div>
					<img src="/lythar.svg" alt="Selected Location" class="w-8 h-8 drop-shadow-md" />
				`;
				markerElement = el;
				marker = new mapboxgl.Marker({ element: el, anchor: "bottom" })
					.setLngLat([$formData.longitude, $formData.latitude])
					.addTo(map);
			} else {
				marker.setLngLat([$formData.longitude, $formData.latitude]);
			}
		} else {
			if (marker) {
				marker.remove();
				marker = null;
				markerElement = null;
			}
		}
	});

	// Cleanup marker on destroy
	$effect(() => {
		return () => {
			if (marker) {
				marker.remove();
			}
		};
	});

	// Form Schema
	const tagValues = Object.values(EventTag) as [string, ...string[]];
	const formSchema = z.object({
		name: z.string().min(1, "Name is required"),
		description: z.string().min(1, "Description is required"),
		communityId: z.string().nullable().default(null),
		tags: z.array(z.enum(tagValues)).min(1, "At least one tag is required"),
		latitude: z.number().nullable(),
		longitude: z.number().nullable()
	});

	// Mutation
	const createEventMutation = createPostEventApiCreate();

	// Form initialization (SPA mode)
	const initialData: z.infer<typeof formSchema> = {
		name: "",
		description: "",
		communityId: null,
		tags: [],
		latitude: null,
		longitude: null
	};

	const form = superForm(defaults(initialData as any, zod4(formSchema as any)), {
		SPA: true,
		validators: zod4(formSchema as any),
		onUpdate: async ({ form: f }) => {
			if (f.valid) {
				try {
					await createEventMutation.mutateAsync({ data: f.data as CreateEventBody });
					newEventOverlayState.open = false;
					f.data = initialData as any;
                    form.reset();
				} catch (e) {
					console.error("Failed to create event", e);
				}
			}
		}
	});

	const { form: formData, enhance } = form;
</script>

<div
	bind:this={overlayContainer}
	class={cn(
		"pointer-events-none absolute right-4 top-4 z-50 flex max-h-[calc(100%-2rem)] w-full max-w-md origin-top-right flex-col",
		open ? "" : ""
	)}
>
	<div
		class={cn(
			"flex flex-col overflow-hidden rounded-xl border border-border/50 bg-background/95 shadow-2xl backdrop-blur-xl transition-all",
			open ? "pointer-events-auto" : "pointer-events-none"
		)}
	>
		<div
			class="flex items-center justify-between border-b border-border/50 bg-muted/30 p-4 backdrop-blur-sm"
		>
			<h2 class="text-lg font-semibold tracking-tight">New Event</h2>
			<Button
				variant="ghost"
				size="icon"
				class="h-8 w-8"
				onclick={() => {
					newEventOverlayState.open = false;
				}}
			>
				<X class="h-4 w-4" />
				<span class="sr-only">Close</span>
			</Button>
		</div>

		<div
			class="flex-1 overflow-y-auto p-4 scrollbar-thin scrollbar-thumb-border scrollbar-track-transparent"
		>
			<form method="POST" use:enhance class="space-y-4" id="create-event-form">
				<div class="animate-item">
					<Form.Field {form} name="name">
						<Form.Control>
							{#snippet children({ props })}
								<Form.Label>Name</Form.Label>
								<Input {...props} bind:value={$formData.name} placeholder="Event name" />
							{/snippet}
						</Form.Control>
						<Form.FieldErrors />
					</Form.Field>
				</div>

				<div class="animate-item">
					<Form.Field {form} name="description">
						<Form.Control>
							{#snippet children({ props })}
								<Form.Label>Description</Form.Label>
								<Textarea
									{...props}
									bind:value={$formData.description}
									placeholder="Describe your event..."
									rows={4}
								/>
							{/snippet}
						</Form.Control>
						<Form.FieldErrors />
					</Form.Field>
				</div>

				<div class="animate-item relative z-10">
					<div class="space-y-2">
						<Label>Location</Label>
						{#if $formData.latitude && $formData.longitude}
							<div class="flex items-start gap-3 rounded-md border bg-muted/50 p-3">
								<MapPin class="mt-0.5 h-4 w-4 shrink-0 text-muted-foreground" />
								<div class="flex-1 space-y-1">
									<p class="text-sm font-medium leading-none">
										{#if locationQuery?.isPending}
											<span class="animate-pulse">Loading address...</span>
										{:else if locationQuery?.data?.features?.[0]?.properties}
											{locationQuery.data.features[0].properties.name ??
												locationQuery.data.features[0].properties.place_formatted}
										{:else}
											Selected Location
										{/if}
									</p>
									<p class="text-xs text-muted-foreground">
										{$formData.latitude.toFixed(6)}, {$formData.longitude.toFixed(6)}
									</p>
								</div>
								<Button
									variant="ghost"
									size="icon"
									class="h-6 w-6"
									onclick={() => {
										$formData.latitude = null;
										$formData.longitude = null;
										newEventOverlayState.location = null;
									}}
								>
									<X class="h-4 w-4" />
								</Button>
							</div>
						{:else}
							<div class="space-y-2">
								<div class="relative z-20">
									<Search class="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
									<Input
										placeholder="Search for a location..."
										class="pl-8"
										bind:value={searchQuery}
										onfocus={() => (isSearching = true)}
										onblur={() => setTimeout(() => (isSearching = false), 200)}
									/>
									{#if searchResults.isPending && debouncedSearch.current.length >= 3}
										<div class="absolute right-2 top-2.5">
											<Loader2 class="h-4 w-4 animate-spin text-muted-foreground" />
										</div>
									{/if}
									{#if isSearching && searchResults.data?.features?.length}
										<div
											class="absolute top-full left-0 right-0 z-60 mt-1 max-h-60 w-full overflow-auto rounded-md border bg-background shadow-lg"
										>
											{#each searchResults.data.features as feature}
												<button
													class="flex w-full flex-col items-start rounded-sm px-2 py-1.5 text-sm hover:bg-accent hover:text-accent-foreground"
													onclick={() => {
														const [lng, lat] = feature.geometry.coordinates;
														$formData.latitude = lat;
														$formData.longitude = lng;
														newEventOverlayState.location = { lat, lng };
														map.flyTo({ center: [lng, lat], zoom: 14 });
														searchQuery = "";
														isSearching = false;
													}}
												>
													<span class="font-medium"
														>{feature.properties.name ??
															feature.properties.place_formatted?.split(",")[0]}</span
													>
													<span class="text-xs text-muted-foreground line-clamp-1"
														>{feature.properties.full_address ??
															feature.properties.place_formatted}</span
													>
												</button>
											{/each}
										</div>
									{/if}
								</div>
								<div class="grid grid-cols-2 gap-2">
									<Button
										variant="outline"
										class="w-full"
										onclick={() => {
											if (navigator.geolocation) {
												navigator.geolocation.getCurrentPosition(
													(position) => {
														const { latitude, longitude } = position.coords;
														$formData.latitude = latitude;
														$formData.longitude = longitude;
														newEventOverlayState.location = { lat: latitude, lng: longitude };
														map.flyTo({ center: [longitude, latitude], zoom: 14 });
													},
													(error) => {
														console.error("Error getting location", error);
													}
												);
											}
										}}
									>
										<LocateFixed class="mr-2 h-4 w-4" />
										Current Location
									</Button>
									<Button
										variant="outline"
										class="w-full"
										onclick={() => {
											newEventOverlayState.open = false;
										}}
									>
										<Map class="mr-2 h-4 w-4" />
										Select on Map
									</Button>
								</div>
							</div>
						{/if}
						<input type="hidden" name="latitude" bind:value={$formData.latitude} />
						<input type="hidden" name="longitude" bind:value={$formData.longitude} />
					</div>
				</div>

				<div class="animate-item">
					<Form.Field {form} name="tags">
						<Form.Control>
							{#snippet children({ props })}
								<Form.Label>Tags</Form.Label>
								<div class="flex flex-wrap gap-2 pt-2">
									{#each tagValues as tag}
										{@const isSelected = $formData.tags.includes(tag)}
										<Badge
											variant={isSelected ? "default" : "outline"}
											class={cn(
												"cursor-pointer transition-all hover:bg-primary/90 active:scale-95",
												!isSelected && "hover:bg-accent hover:text-accent-foreground"
											)}
											onclick={() => {
												if (isSelected) {
													$formData.tags = $formData.tags.filter((t: string) => t !== tag);
												} else {
													$formData.tags = [...$formData.tags, tag];
												}
											}}
										>
											{tag}
										</Badge>
									{/each}
								</div>
							{/snippet}
						</Form.Control>
						<Form.Description>Select at least one category.</Form.Description>
						<Form.FieldErrors />
					</Form.Field>
				</div>
			</form>
		</div>
		<div
			class="flex items-center justify-end gap-2 border-t border-border/50 bg-muted/30 p-4 backdrop-blur-sm"
		>
			<Button
				variant="outline"
				onclick={() => {
					cancelDialogOpen = true;
				}}
			>Cancel</Button>
			<Button
				type="submit"
				form="create-event-form"
				disabled={createEventMutation.isPending}
			>
				{#if createEventMutation.isPending}
					Creating...
				{:else}
					Create Event
				{/if}
			</Button>
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
			<AlertDialog.Cancel>Cancel</AlertDialog.Cancel>
			<AlertDialog.Action
				onclick={() => {
					form.reset();
					newEventOverlayState.open = false;
					cancelDialogOpen = false;
				}}>Discard</AlertDialog.Action
			>
		</AlertDialog.Footer>
	</AlertDialog.Content>
</AlertDialog.Root>



