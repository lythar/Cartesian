<script lang="ts">
	import "../app.css";
	import favicon from "$lib/assets/favicon.svg";
	import { ModeWatcher } from "mode-watcher";
	import { themeApplier } from "$lib/material-gen";
	import { QueryClientProvider } from "@tanstack/svelte-query";
	import { queryClient } from "$lib/api/query-client";

	themeApplier.initialize();

	let { children } = $props();
</script>

<svelte:head>
	<link rel="icon" href={favicon} />
</svelte:head>

<ModeWatcher />

<QueryClientProvider client={queryClient}>
	{@render children()}
</QueryClientProvider>
