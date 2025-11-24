<script lang="ts">
	import { cn, type WithElementRef } from "$lib/utils.js";
	import type { HTMLAttributes } from "svelte/elements";

	let {
		ref = $bindable(null),
		class: className,
		children,
		...restProps
	}: WithElementRef<HTMLAttributes<HTMLElement>> = $props();
</script>

<!-- md:peer-data-[variant=inset]:peer-data-[state=collapsed]:ml-2 -->
<main
	bind:this={ref}
	data-slot="sidebar-inset"
	class={cn(
		"relative flex w-full flex-1 flex-col bg-background",
		"md:peer-data-[variant=inset]:m-2 md:peer-data-[variant=inset]:ml-2  md:peer-data-[variant=inset]:rounded-xl md:peer-data-[variant=inset]:shadow-sm",
		className,
	)}
	{...restProps}
>
	{@render children?.()}
</main>
