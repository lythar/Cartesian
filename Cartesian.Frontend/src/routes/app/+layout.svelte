<script>
	import Navigation from "$lib/components/layout/navigation.svelte";

  let { children } = $props();
</script>

<Navigation>
  {@render children?.()}
</Navigation>
