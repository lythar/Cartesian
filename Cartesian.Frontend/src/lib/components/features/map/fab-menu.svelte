<script lang="ts">
	import { Button } from "$lib/components/ui/button";
	import { Plus, Calendar, Users, PartyPopper, X } from "@lucide/svelte";
	import { animate, stagger } from "motion";
	import type { ComponentType } from "svelte";

	let isExpanded = $state(false);
	let actionRefs = $state<HTMLDivElement[]>([]);

	const actions: Array<{
		icon: ComponentType;
		label: string;
		action: () => void;
	}> = [
		{ icon: Calendar, label: "Add Event", action: () => console.log("Add Event") },
		{ icon: Users, label: "Add Gathering", action: () => console.log("Add Gathering") },
		{ icon: PartyPopper, label: "Add Meetup", action: () => console.log("Add Meetup") },
	];

	$effect(() => {
		if (!isExpanded || !actionRefs.length) return;

		// Use setTimeout to ensure DOM is updated
		const timeout = setTimeout(() => {
			const validRefs = actionRefs.filter(ref => ref !== null && ref !== undefined);
			if (validRefs.length === 0) return;

			animate(
				validRefs,
				{
					opacity: [0, 1],
					y: [20, 0],
					scale: [0.8, 1]
				},
				{
					duration: 0.4,
					delay: stagger(0.08),
					ease: [0.32, 0.72, 0, 1]
				}
			);
		}, 0);

		return () => clearTimeout(timeout);
	});

	$effect(() => {
		if (isExpanded || !actionRefs.length) return;

		const validRefs = actionRefs.filter(ref => ref !== null && ref !== undefined);
		if (validRefs.length === 0) return;

		animate(
			validRefs,
			{
				opacity: 0,
				y: 20,
				scale: 0.8
			},
			{
				duration: 0.2,
				ease: [0.32, 0.72, 0, 1]
			}
		);
	});
</script>

<div class="absolute bottom-8 right-8 z-20 flex flex-col items-end gap-3">
	{#if isExpanded}
		{#each actions as action, i}
			<div
				bind:this={actionRefs[i]}
				class="flex items-center gap-3"
				style="opacity: 0"
			>
				<span
					class="bg-sidebar/95 backdrop-blur-sm shadow-lg border border-border/50 px-5 py-2.5 rounded-full text-sm font-semibold whitespace-nowrap"
				>
					{action.label}
				</span>
				<Button
					variant="default"
					size="icon-lg"
					class="size-16 rounded-full shadow-2xl hover:scale-105 transition-transform duration-200"
					onclick={() => {
						action.action();
						isExpanded = false;
					}}
				>
					<action.icon class="size-6" />
				</Button>
			</div>
		{/each}
	{/if}

	<Button
		variant="default"
		size="icon-lg"
		class="size-18 rounded-full shadow-2xl hover:scale-105 transition-all duration-200"
		onclick={() => (isExpanded = !isExpanded)}
	>
		{#if isExpanded}
			<X class="size-7" />
		{:else}
			<Plus class="size-7" />
		{/if}
	</Button>
</div>
