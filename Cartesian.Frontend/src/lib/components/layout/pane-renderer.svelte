<script lang="ts">
	import { getPaneContext } from "$lib/context/pane.svelte";
	import { fly } from "svelte/transition";

	const paneManager = getPaneContext();

	const leftPane = $derived(paneManager.getActiveLeftPane());
	const rightPane = $derived(paneManager.getActiveRightPane());
</script>

{#if leftPane}
	<div
		transition:fly={{ x: -300, duration: 300 }}
		class="absolute left-2 top-2 bottom-2 z-20 w-96 max-w-[90vw] rounded-lg bg-sidebar/95 backdrop-blur-sm border border-border shadow-xl overflow-hidden flex flex-col"
	>
		<div class="flex items-center justify-between p-4 border-b border-border">
			<h2 class="text-lg font-semibold">
				{leftPane.id}
			</h2>
			<button
				onclick={() => paneManager.closeLeftPane()}
				class="text-muted-foreground hover:text-foreground transition-colors"
				aria-label="Close pane"
			>
				<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
					<line x1="18" y1="6" x2="6" y2="18"></line>
					<line x1="6" y1="6" x2="18" y2="18"></line>
				</svg>
			</button>
		</div>
		<div class="flex-1 overflow-y-auto p-4">
			<leftPane.component {...(leftPane.props ?? {})} />
		</div>
	</div>
{/if}

{#if rightPane}
	<div
		transition:fly={{ x: 300, duration: 300 }}
		class="absolute right-2 top-2 bottom-2 z-20 w-96 max-w-[90vw] rounded-lg bg-sidebar/95 backdrop-blur-sm border border-border shadow-xl overflow-hidden flex flex-col"
	>
		<div class="flex items-center justify-between p-4 border-b border-border">
			<h2 class="text-lg font-semibold">
				{rightPane.id}
			</h2>
			<button
				onclick={() => paneManager.closeRightPane()}
				class="text-muted-foreground hover:text-foreground transition-colors"
				aria-label="Close pane"
			>
				<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
					<line x1="18" y1="6" x2="6" y2="18"></line>
					<line x1="6" y1="6" x2="18" y2="18"></line>
				</svg>
			</button>
		</div>
		<div class="flex-1 overflow-y-auto p-4">
			<rightPane.component {...(rightPane.props ?? {})} />
		</div>
	</div>
{/if}
