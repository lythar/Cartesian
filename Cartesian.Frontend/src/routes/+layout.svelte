<script lang="ts">
	import "../app.css";
	import favicon from "$lib/assets/favicon.svg";
	import { QueryClientProvider } from "@tanstack/svelte-query";
	import { queryClient } from "$lib/api/query-client";
	import ThemeShell from "$lib/components/theme-shell.svelte";

	let { children } = $props();
</script>

<svelte:head>
	<link rel="icon" href={favicon} />
</svelte:head>

<ThemeShell />

<QueryClientProvider client={queryClient}>
	{@render children()}
</QueryClientProvider>
