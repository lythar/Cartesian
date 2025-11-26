<script lang="ts">
	import { page } from "$app/state";
	import { Button } from "$lib/components/ui/button";
	import * as m from "$lib/paraglide/messages";
</script>

<div class="flex h-[80vh] flex-col items-center justify-center gap-6 text-center">
	<div class="space-y-2">
		<h1 class="text-9xl font-bold tracking-tighter text-primary/20">
			{page.status}
		</h1>
		<h2 class="text-2xl font-semibold tracking-tight">
			{page.error?.message || m.error_something_went_wrong()}
		</h2>
		<p class="max-w-[500px] text-muted-foreground">
			{#if page.status === 404}
				{m.error_page_not_found()}
			{:else}
				{m.error_unexpected()}
			{/if}
		</p>
	</div>

	<Button href="/" variant="default" size="lg">{m.return_home()}</Button>
</div>
