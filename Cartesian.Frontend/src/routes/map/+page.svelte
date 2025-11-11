<script lang="ts">
	import MapRenderer from "$lib/components/features/map/map-renderer.svelte";

	let { data } = $props();
</script>

<MapRenderer ipGeo={data.ipGeo} />
