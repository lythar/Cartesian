<script lang="ts">
	import { Input } from "$lib/components/ui/input";
	import { Button } from "$lib/components/ui/button";
	import { animate } from "motion";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { AiMagicIcon, Cancel01Icon, Search01Icon } from "@hugeicons/core-free-icons";
	import { getLayoutContext } from "$lib/context/layout.svelte";
	import UserMenu from "./user-menu.svelte";

  const layout = getLayoutContext();

	let searchValue = $state("");
	let isAIMode = $state(false);
	let containerRef: HTMLDivElement | undefined = $state();
	let contentRef: HTMLDivElement | undefined = $state();
	let inputWrapperRef: HTMLDivElement | undefined = $state();
</script>

<div class="absolute top-4 left-0 right-0 z-20 flex items-center gap-2 px-4 lg:left-4 lg:w-fit lg:right-auto">
	<div
		bind:this={containerRef}
		class="relative flex gap-2 items-center rounded-full bg-card backdrop-blur-sm shadow-neu-highlight overflow-hidden px-2 py-2 h-16 w-full lg:w-auto"
	>
    <div class="flex items-center gap-2 w-full lg:w-70 h-12 lg:h-10 px-2 bg-secondary rounded-full">
      <HugeiconsIcon icon={Search01Icon} className="size-6 duotone-fill" />
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
      class="rounded-full bg-secondary w-12 h-12 lg:h-10 lg:w-10 flex items-center justify-center  active:scale-[0.98] transition-all duration-50"
      onclick={() => (isAIMode = !isAIMode)}
    >
      <HugeiconsIcon icon={AiMagicIcon} size={30} className="duotone-fill" />
      <span class="sr-only">Toggle AI mode</span>
    </Button>
    {#if layout.isMobile}
      <UserMenu class="h-12 w-12 flex items-center justify-center" />
    {/if}
	</div>
</div>
