<script lang="ts">
	import { Button } from "$lib/components/ui/button";
	import Globe from "$lib/components/landing/globe.svelte";
	import { onMount } from "svelte";
	import { animate, stagger } from "motion";

	let globeContainer: HTMLElement | undefined = undefined;

	const events = [
		{ title: "Midnight Jazz", category: "Music", x: 20, y: 15 },
		{ title: "Night Market", category: "Food", x: 42, y: 25 },
		{ title: "Tech Meetup", category: "Tech", x: 80, y: 10 },
		{ title: "Run Club", category: "Sports", x: 85, y: 30 },
		{ title: "Art Gallery", category: "Arts", x: 35, y: 40 },
		{ title: "Urban Hike", category: "Sports", x: 82, y: 25 },
		{ title: "Rooftop Cinema", category: "Film", x: 78, y: 30 },
		// "Hidden" hemisphere events that rotate into view
		{ title: "Sunrise Yoga", category: "Wellness", x: 25, y: 65 },
		{ title: "Street Food", category: "Food", x: 55, y: 75 },
		{ title: "Indie Game Demo", category: "Tech", x: 80, y: 60 },
		{ title: "Poetry Slam", category: "Arts", x: 15, y: 85 },
		{ title: "Rooftop Cinema", category: "Film", x: 60, y: 90 },
		{ title: "Coffee Tasting", category: "Food", x: 90, y: 55 },
		{ title: "Urban Hike", category: "Sports", x: 40, y: 95 },
		{ title: "Vinyl Swap", category: "Music", x: 10, y: 50 },
	];

	onMount(() => {
		const heroTexts = document.querySelectorAll(".hero-text");

		// Entrance animation for Text
		animate(
			heroTexts,
			{ opacity: [0, 1], y: [30, 0] },
			{ duration: 1.2, delay: stagger(0.15), ease: [0.22, 1, 0.36, 1] },
		);

		// Entrance animation for Globe
		if (globeContainer) {
			animate(
				globeContainer,
				{ opacity: [0, 1], y: [100, 0] },
				{ duration: 1.6, delay: 0.4, ease: "easeOut" },
			);
		}
	});
</script>

<div class="relative flex min-h-screen flex-col overflow-hidden" role="application">
	<!-- Navigation -->
	<header class="fixed top-0 z-40 w-full px-6 py-6 md:px-12">
		<div class="flex items-center justify-between">
			<div
				class="text-lg font-bold tracking-tighter text-white uppercase mix-blend-difference"
			>
				Kart
			</div>
			<nav>
				<Button
					href="/login"
					variant="ghost"
					class="text-xs font-medium tracking-widest text-white/70 uppercase hover:bg-white/10 hover:text-white"
				>
					Login
				</Button>
			</nav>
		</div>
	</header>

	<!-- Main Content -->
	<main class="relative z-10 flex flex-1 flex-col items-center px-4 pt-32 md:pt-40">
		<!-- Typography -->
		<div class="relative z-30 flex flex-col items-center gap-8 text-center">
			<div class="space-y-2 mix-blend-screen md:space-y-4">
				<h1
					class="hero-text bg-gradient-to-b from-white via-white/90 to-white/50 bg-clip-text text-7xl font-black tracking-tighter text-transparent opacity-0 drop-shadow-2xl md:text-9xl"
				>
					FIND YOUR<br />PEOPLE
				</h1>
				<p
					class="hero-text mx-auto max-w-md font-mono text-sm leading-relaxed tracking-widest text-white/60 uppercase opacity-0 md:text-lg"
				>
					The social map for local events.<br />Connect. Explore. Belong.
				</p>
			</div>

			<div class="hero-text z-40 pt-4 opacity-0">
				<Button
					href="/app"
					size="lg"
					class="group relative h-14 overflow-hidden rounded-full border-none bg-white px-8 text-zinc-950 shadow-[0_0_40px_-10px_rgba(255,255,255,0.3)] transition-all duration-500 hover:bg-zinc-200"
				>
					<span class="relative z-10 flex items-center gap-2 font-bold tracking-tight">
						Launch Map
						<svg
							xmlns="http://www.w3.org/2000/svg"
							width="16"
							height="16"
							viewBox="0 0 24 24"
							fill="none"
							stroke="currentColor"
							stroke-width="2"
							stroke-linecap="round"
							stroke-linejoin="round"
							class="transition-transform group-hover:translate-x-1"
							><path d="M5 12h14" /><path d="m12 5 7 7-7 7" /></svg
						>
					</span>
				</Button>
			</div>
		</div>

		<!-- Bottom Globe Visual -->
		<div
			bind:this={globeContainer}
			class="pointer-events-none absolute right-0 bottom-0 left-0 z-0 flex items-end justify-center opacity-0"
		>
			<div class="relative h-[160vw] w-[160vw] translate-y-[65%] md:h-[90vw] md:w-[90vw]">
				<Globe {events}>
					{#snippet children({ event })}
						<div class="flex -translate-y-full flex-col items-center gap-2 pb-2">
							<!-- Bubble -->
							<div
								class="relative w-max max-w-[140px] rounded-xl border border-white/10 bg-black/60 p-2.5 shadow-2xl backdrop-blur-md transition-all hover:scale-105 hover:bg-black/80"
							>
								<div class="mb-1 flex items-center gap-1.5">
									<div
										class="h-1.5 w-1.5 rounded-full bg-indigo-400 shadow-[0_0_6px_rgba(129,140,248,0.8)]"
									></div>
									<span class="text-[9px] tracking-wider text-white/50 uppercase"
										>{event.category}</span
									>
								</div>
								<div class="text-[11px] leading-none font-medium text-white/90">
									{event.title}
								</div>

								<!-- Decorative corner accents -->
								<div
									class="absolute top-0 left-0 h-2 w-2 rounded-tl-md border-t border-l border-white/20"
								></div>
								<div
									class="absolute right-0 bottom-0 h-2 w-2 rounded-br-md border-r border-b border-white/20"
								></div>
							</div>

							<!-- Marker + Tether -->
							<div class="flex flex-col items-center">
								<div
									class="h-8 w-px bg-linear-to-b from-white/20 to-transparent"
								></div>
								<div
									class="relative h-1.5 w-1.5 rounded-full bg-white shadow-[0_0_10px_white]"
								>
									<div
										class="absolute inset-0 animate-ping rounded-full bg-white/50 opacity-75"
									></div>
								</div>
							</div>
						</div>
					{/snippet}
				</Globe>
			</div>
		</div>
	</main>

	<!-- Footer -->
	<footer
		class="pointer-events-none fixed bottom-6 z-40 flex w-full items-end justify-between px-6 text-white/40 mix-blend-difference md:px-12"
	>
		<div class="hidden flex-col gap-1 font-mono text-[10px] tracking-widest uppercase md:flex">
			<span>System: Online</span>
			<span>Loc: Global</span>
		</div>

		<div class="text-right font-mono text-[10px] tracking-widest uppercase">
			© {new Date().getFullYear()} Kart Inc.<br />
			All rights reserved.
		</div>
	</footer>
</div>
