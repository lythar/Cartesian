<script lang="ts">
	import { ColorPicker } from "$lib/material-gen";
	import { createGetExample, createIpGeoQuery } from "$lib/api";

	const exampleQuery = createGetExample();
	const ipGeoQuery = createIpGeoQuery();

	$effect(() => {
		if (ipGeoQuery.data) {
			console.log("User's IP Geolocation:", ipGeoQuery.data);
		}
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
			<button
				class="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground"
			>
				Primary Button
			</button>
			<button
				class="rounded-md bg-secondary px-4 py-2 text-sm font-medium text-secondary-foreground"
			>
				Secondary Button
			</button>
			<button
				class="rounded-md bg-accent px-4 py-2 text-sm font-medium text-accent-foreground"
			>
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
				Change the theme color above to see the entire UI update with a harmonious Material
				3 color scheme! Your color preference is automatically saved and synced across tabs.
			</p>
		</div>

		<div class="grid gap-4 md:grid-cols-2">
			<div class="rounded-lg border border-border bg-background p-6">
				<h3 class="mb-2 font-semibold">Cartesian API</h3>
				{#if exampleQuery.isLoading}
					<p class="text-sm text-muted-foreground">Loading...</p>
				{:else if exampleQuery.error}
					<p class="text-sm text-destructive">Error: {String(exampleQuery.error)}</p>
				{:else if exampleQuery.data}
					<p class="text-sm text-primary">{exampleQuery.data}</p>
				{:else}
					<p class="text-sm text-muted-foreground">No data</p>
				{/if}
			</div>

			<div class="rounded-lg border border-border bg-background p-6">
				<h3 class="mb-2 font-semibold">IP Geolocation (Effect.ts)</h3>
				{#if ipGeoQuery.isLoading}
					<p class="text-sm text-muted-foreground">Loading...</p>
				{:else if ipGeoQuery.error}
					<p class="text-sm text-destructive">Error: {ipGeoQuery.error.message}</p>
				{:else if ipGeoQuery.data}
					<div class="space-y-1 text-sm">
						<p>Country: {ipGeoQuery.data.countryCode}</p>
						<p>
							Location: {ipGeoQuery.data.lat.toFixed(4)}, {ipGeoQuery.data.lon.toFixed(
								4,
							)}
						</p>
					</div>
				{:else}
					<p class="text-sm text-muted-foreground">No data</p>
				{/if}
			</div>
		</div>

		<div class="rounded-lg border border-border bg-background p-6">
			<p class="text-sm text-muted-foreground">
				Visit <a href="https://svelte.dev/docs/kit" class="text-primary underline"
					>svelte.dev/docs/kit</a
				> to read the documentation
			</p>
		</div>
	</div>
</div>
