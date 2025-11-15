<script lang="ts">
	import { Input } from "$lib/components/ui/input";
	import { Button } from "$lib/components/ui/button";
	import { animate } from "motion";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { AiMagicIcon, Cancel01Icon, Search01Icon } from "@hugeicons/core-free-icons";

	let searchValue = $state("");
	let isAIMode = $state(false);
	let containerRef: HTMLDivElement | undefined = $state();
	let contentRef: HTMLDivElement | undefined = $state();
	let inputWrapperRef: HTMLDivElement | undefined = $state();
</script>

<div class="absolute top-4 left-4 z-20 flex items-center gap-2">
	<div
		bind:this={containerRef}
		class="relative flex gap-2 items-center rounded-full bg-background backdrop-blur-sm shadow-neu-highlight border border-border/50 overflow-hidden px-2 py-2"
	>
    <div class="flex items-center gap-2 w-70 h-full px-2 bg-card rounded-full">
      <HugeiconsIcon icon={Search01Icon} className="size-6 fill-current/30" />
      <div bind:this={inputWrapperRef} class="flex-1">
        <Input
          bind:value={searchValue}
          placeholder={isAIMode ? "Ask AI to find events..." : "Search events, locations..."}
          class="border-0 shadow-none bg-transparent! focus-visible:ring-0 h-10 text-base px-0"
        />
      </div>
    </div>
    <Button
      variant={isAIMode ? "default" : "ghost"}
      size="lg"
      class="rounded-full p-0 bg-card h-10 w-10 flex items-center justify-center"
      onclick={() => (isAIMode = !isAIMode)}
    >
      <HugeiconsIcon icon={AiMagicIcon} className="fill-current" />
      <span class="sr-only">Toggle AI mode</span>
    </Button>
	</div>
</div>
