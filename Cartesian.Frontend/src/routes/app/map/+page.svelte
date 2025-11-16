<script lang="ts">
	import MapRenderer from "$lib/components/map/map-renderer.svelte";

	let { data } = $props();
</script>

<MapRenderer ipGeo={data.ipGeo} />
