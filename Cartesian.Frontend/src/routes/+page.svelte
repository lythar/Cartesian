<script lang="ts">
	import { client } from "$lib/client";
	import { ColorPicker } from "$lib/material-gen";
	import { onMount } from "svelte";

	let text = $state("Loading...");

	onMount(async () => {
		// const res = await client.GET("/example", {
		// 	parseAs: "text",
		// });

		// text = res.data ?? res.response.statusText;
	});
</script>

<div class="min-h-screen bg-background p-8 text-foreground">
	<div class="mx-auto max-w-4xl space-y-8">
		<div class="space-y-4">
			<h1 class="text-4xl font-bold">Welcome to SvelteKit</h1>
			<ColorPicker />
		</div>

		<div class="grid gap-4 md:grid-cols-2">
			<div class="rounded-lg border border-border bg-card p-6">
				<h2 class="mb-2 text-xl font-semibold text-card-foreground">Card Component</h2>
				<p class="text-sm text-muted-foreground">This is muted text on a card</p>
			</div>

			<div class="rounded-lg border border-border bg-popover p-6">
				<h2 class="mb-2 text-xl font-semibold text-popover-foreground">Popover Style</h2>
				<p class="text-sm text-muted-foreground">Different surface variant</p>
			</div>
		</div>

		<div class="flex flex-wrap gap-3">
			<button class="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground">
				Primary Button
			</button>
			<button
				class="rounded-md bg-secondary px-4 py-2 text-sm font-medium text-secondary-foreground"
			>
				Secondary Button
			</button>
			<button class="rounded-md bg-accent px-4 py-2 text-sm font-medium text-accent-foreground">
				Accent Button
			</button>
			<button
				class="rounded-md bg-destructive px-4 py-2 text-sm font-medium text-destructive-foreground"
			>
				Destructive
			</button>
		</div>

		<div class="rounded-lg border border-border bg-muted p-6">
			<p class="text-sm text-muted-foreground">
				Change the theme color above to see the entire UI update with a harmonious Material 3 color
				scheme! Your color preference is automatically saved and synced across tabs.
			</p>
		</div>

		<div class="rounded-lg border border-border bg-background p-6">
			<p class="text-sm">Visit <a href="https://svelte.dev/docs/kit" class="text-primary underline">svelte.dev/docs/kit</a> to read the documentation</p>
			<p class="mt-2 text-sm text-muted-foreground">{text}</p>
		</div>
	</div>
</div>
