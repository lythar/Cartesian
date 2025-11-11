<script lang="ts">
	import "../app.css";
	import favicon from "$lib/assets/favicon.svg";
	import { ModeWatcher } from "mode-watcher";
	import { themeApplier } from "$lib/material-gen";

	themeApplier.initialize();

	let { children } = $props();
</script>

<svelte:head>
	<link rel="icon" href={favicon} />
</svelte:head>

<ModeWatcher />

{@render children()}
