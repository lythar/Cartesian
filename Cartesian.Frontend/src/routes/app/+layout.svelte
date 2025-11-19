<script>
	import Navigation from "$lib/components/layout/navigation.svelte";
	import { createLayoutContext } from "$lib/context/layout.svelte";

  let { children } = $props();

  createLayoutContext();
</script>

<Navigation>
  {@render children?.()}
</Navigation>
