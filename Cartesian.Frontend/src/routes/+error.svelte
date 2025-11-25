<script lang="ts">
	import { page } from "$app/state";
	import { Button } from "$lib/components/ui/button";
</script>

<div class="flex h-[80vh] flex-col items-center justify-center gap-6 text-center">
	<div class="space-y-2">
		<h1 class="text-9xl font-bold tracking-tighter text-primary/20">
			{page.status}
		</h1>
		<h2 class="text-2xl font-semibold tracking-tight">
			{page.error?.message || "Something went wrong"}
		</h2>
		<p class="text-muted-foreground max-w-[500px]">
			{#if page.status === 404}
				The page you are looking for does not exist. It might have been moved or deleted.
			{:else}
				An unexpected error occurred. Please try again later.
			{/if}
		</p>
	</div>

	<Button href="/" variant="default" size="lg">Return Home</Button>
</div>
