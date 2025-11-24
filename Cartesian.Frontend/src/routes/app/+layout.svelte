<script lang="ts">
	import Navigation from "$lib/components/layout/navigation.svelte";
	import { createLayoutContext } from "$lib/context/layout.svelte";
	import "mapbox-gl/dist/mapbox-gl.css";

	const { children } = $props();
	createLayoutContext();
</script>

<Navigation>
	{@render children?.()}
</Navigation>
