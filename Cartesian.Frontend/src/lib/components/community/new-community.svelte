<script lang="ts">
	import { createPostCommunityApiCreate, type PostCreateCommunityBody } from "$lib/api/cartesian-client";
	import * as Form from "$lib/components/ui/form";
	import { Input } from "$lib/components/ui/input";
	import { Label } from "$lib/components/ui/label";
	import { Textarea } from "$lib/components/ui/textarea";
	import { cn } from "$lib/utils";
	import {
		AiEditingIcon,
		Cancel01Icon,
		Globe02Icon,
		LockKeyIcon,
		UserGroupIcon
	} from "@hugeicons/core-free-icons";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { animate } from "motion";
	import { fade } from "svelte/transition";
	import { defaults, superForm } from "sveltekit-superforms";
	import { zod4 } from "sveltekit-superforms/adapters";
	import { z } from "zod";
	import Button from "../ui/button/button.svelte";
	import { newCommunityOverlayState } from "./community-state.svelte";

	let overlayContainer = $state<HTMLDivElement | null>(null);
	let open = $state<boolean>(newCommunityOverlayState.open);
	let isGlowing = $state(false);
	let previousOpen = $state<boolean | null>(null);

	$effect(() => {
		open = newCommunityOverlayState.open;
	});

	$effect(() => {
		if (previousOpen === null) {
			previousOpen = open;
			return;
		}

		if (open !== previousOpen) {
			previousOpen = open;

			if (open) {
				if (overlayContainer) {
					animate(
						overlayContainer,
						{
							opacity: [0, 1],
							x: [20, 0],
							scale: [0.95, 1],
							filter: ["blur(8px)", "blur(0px)"]
						},
						{
							duration: 0.5,
							ease: [0.16, 1, 0.3, 1] // Spring-like ease
						}
					);
				}
			} else {
				if (overlayContainer) {
					animate(
						overlayContainer,
						{
							opacity: [1, 0],
							scale: [1, 0.95],
							filter: ["blur(0px)", "blur(8px)"]
						},
						{
							duration: 0.3,
							ease: [0.16, 1, 0.3, 1]
						}
					);
				}
			}
		}
	});

	const formSchema = z.object({
		name: z.string().min(1, "Name is required"),
		description: z.string().min(1, "Description is required"),
		inviteOnly: z.boolean().default(false)
	});

	const createCommunityMutation = createPostCommunityApiCreate();

	const initialData: z.infer<typeof formSchema> = {
		name: "",
		description: "",
		inviteOnly: false
	};

	const form = superForm(defaults(initialData, zod4(formSchema)), {
		SPA: true,
		validators: zod4(formSchema),
		onUpdate: async ({ form: f }) => {
			if (f.valid) {
				try {
					const communityBody: PostCreateCommunityBody = {
						name: f.data.name,
						description: f.data.description,
						inviteOnly: f.data.inviteOnly
					};

					await createCommunityMutation.mutateAsync({ data: communityBody });

					newCommunityOverlayState.open = false;
					f.data = initialData;
					form.reset();
				} catch (e) {
					console.error("Failed to create community", e);
				}
			}
		}
	});

	const { form: formData, enhance } = form;
</script>

<div
	bind:this={overlayContainer}
	class={cn(
		"pointer-events-none absolute right-4 top-4 bottom-4 z-50 flex w-full max-w-lg origin-top-right flex-col",
		open ? "" : "opacity-0"
	)}
>
	<div
		class={cn(
			"flex h-full flex-col overflow-hidden rounded-3xl border border-border/40 bg-background shadow-2xl transition-all",
			open ? "pointer-events-auto" : "pointer-events-none"
		)}
	>
		<!-- Header -->
		<div class="relative flex items-center justify-between px-6 py-5">
			<div class="space-y-0.5">
				<h2 class="text-xl font-semibold tracking-tight">New Community</h2>
				<p class="text-xs font-medium text-muted-foreground">Create a space for people to gather</p>
			</div>
			<Button
				variant="ghost"
				size="icon"
				class="h-8 w-8 rounded-full transition-colors hover:bg-muted"
				onclick={() => {
					newCommunityOverlayState.open = false;
				}}
			>
				<HugeiconsIcon icon={Cancel01Icon} size={16} strokeWidth={2} />
				<span class="sr-only">Close</span>
			</Button>
		</div>

		<!-- Content -->
		<div
			class="flex-1 overflow-y-auto px-6 pb-6 scrollbar-thin scrollbar-thumb-border scrollbar-track-transparent"
		>
			<form method="POST" use:enhance class="space-y-8" id="create-community-form">
				<!-- Basic Info -->
				<div class="space-y-3">
					<Label class="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
						Details
					</Label>

					<Form.Field {form} name="name" class="space-y-1.5">
						<Form.Control>
							{#snippet children({ props })}
								<Form.Label class="text-xs text-muted-foreground ml-1">Community Name</Form.Label>
								<div class="relative">
									<Input
										{...props}
										bind:value={$formData.name}
										placeholder="e.g. Hiking Enthusiasts"
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
									<Form.Label class="text-xs text-muted-foreground ml-1">Description</Form.Label>
									<Textarea
										{...props}
										bind:value={$formData.description}
										placeholder="What is this community about?"
										rows={4}
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
								class={cn(
									"group relative overflow-hidden rounded-full border border-primary/20 bg-background/50 pl-3 pr-4 text-xs font-medium transition-all hover:border-primary/40 hover:bg-background hover:shadow-sm",
									isGlowing && "border-primary/50 shadow-[0_0_15px_rgba(59,130,246,0.3)]"
								)}
								onmouseenter={() => (isGlowing = true)}
								onmouseleave={() => (isGlowing = false)}
							>
								{#if isGlowing}
									<div
										class="absolute inset-0 bg-gradient-to-r from-transparent via-primary/10 to-transparent"
										transition:fade
									></div>
								{/if}
								<HugeiconsIcon
									icon={AiEditingIcon}
									size={14}
									strokeWidth={2}
									className="mr-2 text-primary"
								/>
								<span
									class="bg-gradient-to-r from-foreground to-foreground/70 bg-clip-text text-transparent transition-all group-hover:to-primary"
								>
									Enhance with AI
								</span>
							</Button>
						</div>
					</div>
				</div>

				<!-- Privacy Settings -->
				<div class="space-y-3">
					<Label class="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
						Privacy
					</Label>

					<Form.Field {form} name="inviteOnly" class="space-y-1.5">
						<Form.Control>
							{#snippet children({ props })}
								<div class="flex items-center justify-between rounded-xl border border-border/50 bg-muted/30 p-4 transition-all hover:bg-muted/50">
									<div class="flex items-start gap-3">
										<div class="flex h-9 w-9 items-center justify-center rounded-lg bg-background shadow-sm ring-1 ring-black/5">
											{#if $formData.inviteOnly}
												<HugeiconsIcon icon={LockKeyIcon} size={18} strokeWidth={2} className="text-primary" />
											{:else}
												<HugeiconsIcon icon={Globe02Icon} size={18} strokeWidth={2} className="text-primary" />
											{/if}
										</div>
										<div class="space-y-0.5">
											<Label class="text-sm font-medium">
												{$formData.inviteOnly ? "Private Community" : "Public Community"}
											</Label>
											<p class="text-xs text-muted-foreground">
												{$formData.inviteOnly
													? "Only people with an invite link can join this community."
													: "Anyone can find and join this community."}
											</p>
										</div>
									</div>
									<button
										type="button"
										role="switch"
										aria-checked={$formData.inviteOnly}
										aria-label="Toggle invite only"
										onclick={() => ($formData.inviteOnly = !$formData.inviteOnly)}
										class={cn(
											"peer inline-flex h-6 w-11 shrink-0 cursor-pointer items-center rounded-full border-2 border-transparent transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background disabled:cursor-not-allowed disabled:opacity-50",
											$formData.inviteOnly ? "bg-primary" : "bg-input"
										)}
									>
										<span
											class={cn(
												"pointer-events-none block h-5 w-5 rounded-full bg-background shadow-lg ring-0 transition-transform",
												$formData.inviteOnly ? "translate-x-5" : "translate-x-0"
											)}
										></span>
									</button>
									<input type="hidden" name="inviteOnly" value={$formData.inviteOnly} />
								</div>
							{/snippet}
						</Form.Control>
						<Form.FieldErrors />
					</Form.Field>
				</div>
			</form>
		</div>

		<!-- Footer -->
		<div class="border-t border-border/40 bg-muted/10 p-6 backdrop-blur-sm">
			<div class="flex items-center justify-end gap-3">
				<Button
					variant="ghost"
					onclick={() => {
						newCommunityOverlayState.open = false;
					}}
				>
					Cancel
				</Button>
				<Button
					type="submit"
					form="create-community-form"
					disabled={createCommunityMutation.isPending}
					class="min-w-[120px]"
				>
					{#if createCommunityMutation.isPending}
						<span class="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-background/30 border-t-background"
						></span>
						Creating...
					{:else}
						<HugeiconsIcon icon={UserGroupIcon} size={16} strokeWidth={2} className="mr-2" />
						Create Community
					{/if}
				</Button>
			</div>
		</div>
	</div>
</div>
