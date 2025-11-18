<script lang="ts">
	import { cn, type LatLng } from "$lib/utils";
	import { animate } from "motion";
	import { newEventOverlayState } from "./map-state.svelte";
	import Button from "../ui/button/button.svelte";
	import * as Form from "$lib/components/ui/form";
	import { Input } from "$lib/components/ui/input";
	import { Textarea } from "$lib/components/ui/textarea";
	import { superForm, defaults } from "sveltekit-superforms";
	import { zod } from "sveltekit-superforms/adapters";
	import { z } from "zod";
	import {
		createPostEventApiCreate,
		EventTag,
		type CreateEventBody
	} from "$lib/api/cartesian-client";

	interface Props {
		map: mapboxgl.Map;
	}

	let { map }: Props = $props();

	let overlayContainer = $state<HTMLDivElement | null>(null);

	let open = $state<boolean>(newEventOverlayState.open);
	let draftLocation: LatLng | null = $state<LatLng | null>(null);

	$effect(() => {
		open = newEventOverlayState.open;
		if (open) {
			if (overlayContainer) {
				animate(
					overlayContainer,
					{
						opacity: [0, 1],
						transform: ["scale(0.90)", "scale(1)"]
					},
					{
						duration: 0.25,
						ease: [0.45, -0.05, 0.15, 1.05]
					}
				);
			}
		} else {
			if (overlayContainer) {
				animate(
					overlayContainer,
					{
						opacity: [1, 0],
						transform: ["scale(1)", "scale(0.90)"]
					},
					{
						duration: 0.25,
						ease: [0.45, -0.05, 0.15, 1.05]
					}
				);
			}
		}
	});

	// Form Schema
	const tagValues = Object.values(EventTag) as [string, ...string[]];
	const formSchema = z.object({
		name: z.string().min(1, "Name is required"),
		description: z.string().min(1, "Description is required"),
		communityId: z.string().nullable().default(null),
		tags: z.array(z.enum(tagValues)).min(1, "At least one tag is required")
	});

	// Mutation
	const createEventMutation = createPostEventApiCreate();

	// Form initialization (SPA mode)
	const initialData: CreateEventBody = {
		name: "",
		description: "",
		communityId: null,
		tags: []
	};

	const form = superForm(defaults(initialData as any, zod(formSchema as any)), {
		SPA: true,
		validators: zod(formSchema as any),
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
		"absolute top-0 right-4 z-50 flex h-full w-140 origin-right flex-col pb-4 pt-20 pointer-events-none",
		open ? "" : ""
	)}
>
	<div
		class={cn("flex flex-1 flex-col", open ? "pointer-events-auto" : "pointer-events-none")}
	>
		<div class="flex h-full flex-col justify-between rounded-lg bg-background shadow-lg">
			<div class="flex-1 overflow-y-auto p-6">
				<h2 class="mb-4 text-2xl font-bold tracking-tight">New Event</h2>
				
                				<form method="POST" use:enhance class="space-y-4" id="create-event-form">
				
                					<Form.Field {form} name="name">
				
                						<Form.Control>
				
                							{#snippet children({ props })}
				
                								<Form.Label>Name</Form.Label>
				
                								<Input {...props} bind:value={$formData.name} />
				
                							{/snippet}
				
                						</Form.Control>
				
                						<Form.FieldErrors />
				
                					</Form.Field>
				
                
				
                					<Form.Field {form} name="description">
				
                						<Form.Control>
				
                							{#snippet children({ props })}
				
                								<Form.Label>Description</Form.Label>
				
                								<Textarea {...props} bind:value={$formData.description} />
				
                							{/snippet}
				
                						</Form.Control>
				
                						<Form.FieldErrors />
				
                					</Form.Field>
				
                
				
                					<Form.Field {form} name="tags">
				
                						<Form.Control>
				
                							{#snippet children({ props })}
				
                								<Form.Label>Tags (Hold Ctrl/Cmd to select multiple)</Form.Label>
				
                								<select
				
                									multiple
				
                									class="flex h-32 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
				
                									{...props}
				
                									bind:value={$formData.tags}
				
                								>
				
                									{#each tagValues as tag}
				
                										<option value={tag}>{tag}</option>
				
                									{/each}
				
                								</select>
				
                							{/snippet}
				
                						</Form.Control>
				
                						<Form.Description>Select at least one category for your event.</Form.Description>
				
                						<Form.FieldErrors />
				
                					</Form.Field>
				
                				</form>
				
                
			</div>
			<div
				class="flex items-center justify-end gap-2 rounded-b-lg bg-card p-4 shadow-neu-highlight"
			>
				<Button
					class="self-end"
                    type="submit"
                    form="create-event-form"
                    disabled={createEventMutation.isPending}
				>
                    {#if createEventMutation.isPending}
                        Creating...
                    {:else}
                        Add Event
                    {/if}
                </Button>
				<Button
					variant="secondary"
					class="self-end"
					onclick={() => {
						newEventOverlayState.open = false;
					}}
				>Cancel</Button>
			</div>
		</div>
	</div>
</div>



