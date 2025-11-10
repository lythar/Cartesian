<script lang="ts">
	import { client } from "$lib/client";
	import { onMount } from "svelte";

	let text = $state("Loading...");

	// (this is just an example)
	onMount(async () => {
		const res = await client.GET("/example", {
			parseAs: "text",
		});

		text = res.data ?? res.response.statusText;
	});
</script>

<h1>Welcome to SvelteKit</h1>
<p>Visit <a href="https://svelte.dev/docs/kit">svelte.dev/docs/kit</a> to read the documentation</p>
<p>{text}</p>
