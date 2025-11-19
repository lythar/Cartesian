<script lang="ts">
	import { QueryClientProvider } from "@tanstack/svelte-query";
	import { queryClient } from "$lib/api"
	import "../app.css";
	import { ModeWatcher } from "mode-watcher";
	import { createLayoutContext } from "$lib/context/layout.svelte";

	let { children } = $props();
</script>

<QueryClientProvider client={queryClient}>
	<ModeWatcher />

	<div
		class="dark min-h-screen overflow-x-hidden bg-background font-sans text-foreground antialiased selection:bg-white/20 selection:text-white"
	>
		<!-- Noise Overlay -->
		<div
			class="pointer-events-none fixed inset-0 z-50 h-full w-full opacity-[0.03] mix-blend-overlay"
			style="background-image: url('data:image/svg+xml,%3Csvg viewBox=%220 0 200 200%22 xmlns=%22http://www.w3.org/2000/svg%22%3E%3Cfilter id=%22noiseFilter%22%3E%3CfeTurbulence type=%22fractalNoise%22 baseFrequency=%220.65%22 numOctaves=%223%22 stitchTiles=%22stitch%22/%3E%3C/filter%3E%3Crect width=%22100%25%22 height=%22100%25%22 filter=%22url(%23noiseFilter)%22/%3E%3C/svg%3E');"
		></div>

		<!-- Scanlines (Subtle) -->
		<div
			class="pointer-events-none fixed inset-0 z-50 h-full w-full bg-[linear-gradient(rgba(18,16,16,0)_50%,rgba(0,0,0,0.1)_50%),linear-gradient(90deg,rgba(255,0,0,0.03),rgba(0,255,0,0.01),rgba(0,0,255,0.03))] bg-[length:100%_2px,3px_100%] bg-repeat opacity-10"
		></div>

		<!-- Content -->
		<div class="relative z-10 flex min-h-screen flex-col">
			{@render children()}
		</div>
	</div>
</QueryClientProvider>

<style>
	/* Custom Scrollbar for that "Technical" feel */
	:global(::-webkit-scrollbar) {
		width: 6px;
	}
	:global(::-webkit-scrollbar-track) {
		background: #09090b;
	}
	:global(::-webkit-scrollbar-thumb) {
		background: #27272a;
		border-radius: 3px;
	}
	:global(::-webkit-scrollbar-thumb:hover) {
		background: #3f3f46;
	}
</style>
