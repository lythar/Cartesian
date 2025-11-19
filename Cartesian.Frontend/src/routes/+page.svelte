<script lang="ts">
	import { Button } from '$lib/components/ui/button';
	import { onMount } from 'svelte';
	import { animate, stagger } from 'motion';

	let globeContainer: HTMLElement;
	let planetBase: HTMLElement;
	let mouseX = 0;
	let mouseY = 0;
	let isHovering = false;

	const events = [
		{ title: 'Midnight Jazz', category: 'Music', x: 20, y: 15 },
		{ title: 'Night Market', category: 'Food', x: 45, y: 25 },
		{ title: 'Tech Meetup', category: 'Tech', x: 70, y: 10 },
		{ title: 'Run Club', category: 'Sports', x: 85, y: 30 },
		{ title: 'Art Gallery', category: 'Arts', x: 35, y: 40 },
		// "Hidden" hemisphere events that rotate into view
		{ title: 'Sunrise Yoga', category: 'Wellness', x: 25, y: 65 },
		{ title: 'Street Food', category: 'Food', x: 55, y: 75 },
		{ title: 'Indie Game Demo', category: 'Tech', x: 80, y: 60 },
		{ title: 'Poetry Slam', category: 'Arts', x: 15, y: 85 },
		{ title: 'Rooftop Cinema', category: 'Film', x: 60, y: 90 },
		{ title: 'Coffee Tasting', category: 'Food', x: 90, y: 55 },
		{ title: 'Urban Hike', category: 'Sports', x: 40, y: 95 },
		{ title: 'Vinyl Swap', category: 'Music', x: 10, y: 50 }
	];

	function handleMouseMove(e: MouseEvent) {
		if (!planetBase) return;
		const rect = planetBase.getBoundingClientRect();
		mouseX = e.clientX - rect.left;
		mouseY = e.clientY - rect.top;
		isHovering = true;
	}

	function handleMouseLeave() {
		isHovering = false;
	}

	onMount(() => {
		const heroTexts = document.querySelectorAll('.hero-text');

		// Entrance animation for Text
		animate(
			heroTexts,
			{ opacity: [0, 1], y: [30, 0] },
			{ duration: 1.2, delay: stagger(0.15), easing: [0.22, 1, 0.36, 1] }
		);

		// Entrance animation for Globe
		if (globeContainer) {
			animate(
				globeContainer,
				{ opacity: [0, 1], y: [100, 0] },
				{ duration: 1.6, delay: 0.4, easing: 'ease-out' }
			);
		}
	});
</script>

<div class="relative flex min-h-screen flex-col overflow-hidden" onmousemove={handleMouseMove} onmouseleave={handleMouseLeave} role="application">

	<!-- Navigation -->
	<header class="fixed top-0 z-40 w-full px-6 py-6 md:px-12">
		<div class="flex items-center justify-between">
			<div class="text-lg font-bold tracking-tighter uppercase text-white mix-blend-difference">
				Kart
			</div>
			<nav>
				<Button
					href="/login"
					variant="ghost"
					class="text-xs font-medium uppercase tracking-widest text-white/70 hover:bg-white/10 hover:text-white"
				>
					Login
				</Button>
			</nav>
		</div>
	</header>

	<!-- Main Content -->
	<main class="flex flex-1 flex-col items-center pt-32 md:pt-40 px-4 relative z-10">

		<!-- Typography -->
		<div class="relative z-30 text-center flex flex-col items-center gap-8">
			<div class="space-y-2 md:space-y-4 mix-blend-screen">
				<h1 class="hero-text text-7xl md:text-9xl font-black tracking-tighter text-transparent bg-clip-text bg-gradient-to-b from-white via-white/90 to-white/50 opacity-0 drop-shadow-2xl">
					FIND YOUR<br />PEOPLE
				</h1>
				<p class="hero-text text-sm md:text-lg font-mono text-white/60 tracking-widest uppercase opacity-0 max-w-md mx-auto leading-relaxed">
					The social map for local events.<br/>Connect. Explore. Belong.
				</p>
			</div>

			<div class="hero-text opacity-0 pt-4 z-40">
				<Button
					href="/app/map"
					size="lg"
					class="relative overflow-hidden bg-white text-zinc-950 hover:bg-zinc-200 transition-all duration-500 px-8 h-14 rounded-full group shadow-[0_0_40px_-10px_rgba(255,255,255,0.3)] border-none"
				>
					<span class="relative z-10 font-bold tracking-tight flex items-center gap-2">
						Launch Map
						<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="group-hover:translate-x-1 transition-transform"><path d="M5 12h14"/><path d="m12 5 7 7-7 7"/></svg>
					</span>
				</Button>
			</div>
		</div>

		<!-- Bottom Globe Visual -->
		<div bind:this={globeContainer} class="absolute bottom-0 left-0 right-0 flex justify-center items-end pointer-events-none opacity-0 z-0">
			<!-- The Sphere Wrapper (Sizing & Position) -->
			<div class="relative w-[160vw] h-[160vw] md:w-[90vw] md:h-[90vw] translate-y-[65%]">

				<!-- 1. Planet Base & Texture (Clipped) -->
				<div
					bind:this={planetBase}
					class="absolute inset-0 rounded-full bg-[#050505] border border-white/5 shadow-[0_-20px_100px_-20px_rgba(100,100,255,0.15)] overflow-hidden"
				>
					<!-- Inner Glow (Atmosphere) -->
					<div class="absolute inset-0 rounded-full bg-[radial-gradient(circle_at_50%_0%,rgba(40,40,70,0.4)_0%,transparent_60%)]"></div>

					<!-- Grid Lines -->
					<div class="absolute inset-0 opacity-20">
						<div class="absolute inset-0 bg-[repeating-conic-gradient(from_0deg,transparent_0deg,transparent_29deg,rgba(255,255,255,0.1)_30deg)] opacity-30"></div>
						<div class="absolute inset-0 bg-[repeating-radial-gradient(circle_at_50%_-20%,transparent_0,transparent_50px,rgba(255,255,255,0.05)_51px)]"></div>
					</div>

					<!-- Hidden Map Layer (Sliding) -->
					<div class="absolute inset-0 opacity-[0.05] pointer-events-none mix-blend-overlay" style="filter: url(#spherize)">
						<div class="flex h-full w-[200%] animate-slide">
							<div class="w-1/2 h-full bg-[url('/world.svg')] bg-contain bg-repeat-x"></div>
							<div class="w-1/2 h-full bg-[url('/world.svg')] bg-contain bg-repeat-x"></div>
						</div>
					</div>

					<!-- Dithered Reveal Layer (Terrain) -->
					<div
						class="absolute inset-0 z-10 pointer-events-none transition-opacity duration-300"
						style="
							opacity: {isHovering ? 1 : 0};
							mask-image: radial-gradient(circle 120px at {mouseX}px {mouseY}px, black 100%, transparent 100%);
							-webkit-mask-image: radial-gradient(circle 120px at {mouseX}px {mouseY}px, black 100%, transparent 100%);
						"
					>
						<!-- Sliding Map Content (Revealed) -->
						<div class="flex h-full w-[200%] animate-slide" style="filter: url(#spherize)">
							<!-- Map 1 -->
							<div class="w-1/2 h-full relative">
								<div class="absolute inset-0 bg-[url('/world.svg')] bg-contain bg-repeat-x invert opacity-80"></div>
								<!-- Dither Pattern on Map -->
								<div class="absolute inset-0 mix-blend-hard-light bg-[url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNCIgaGVpZ2h0PSI0IiB2aWV3Qm94PSIwIDAgNCA0IiBmaWxsPSJub25lIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPjxyZWN0IHg9IjAiIHk9IjAiIHdpZHRoPSIxIiBoZWlnaHQ9IjEiIGZpbGw9IndoaXRlIi8+PHJlY3QgeD0iMiIgeT0iMiIgd2lkdGg9IjEiIGhlaWdodD0iMSIgZmlsbD0id2hpdGUiLz48L3N2Zz4=')] opacity-40"></div>
							</div>
							<!-- Map 2 (Duplicate for Loop) -->
							<div class="w-1/2 h-full relative">
								<div class="absolute inset-0 bg-[url('/world.svg')] bg-contain bg-repeat-x invert opacity-80"></div>
								<!-- Dither Pattern on Map -->
								<div class="absolute inset-0 mix-blend-hard-light bg-[url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNCIgaGVpZ2h0PSI0IiB2aWV3Qm94PSIwIDAgNCA0IiBmaWxsPSJub25lIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPjxyZWN0IHg9IjAiIHk9IjAiIHdpZHRoPSIxIiBoZWlnaHQ9IjEiIGZpbGw9IndoaXRlIi8+PHJlY3QgeD0iMiIgeT0iMiIgd2lkdGg9IjEiIGhlaWdodD0iMSIgZmlsbD0id2hpdGUiLz48L3N2Zz4=')] opacity-40"></div>
							</div>
						</div>
					</div>

					<!-- Rotating Surface Textures (Stars) -->
					<div class="absolute inset-0 animate-[spin_240s_linear_infinite]">
						<!-- Constellations & Stars -->
						<div class="absolute inset-0 opacity-50">
							{#each Array(12) as _, i}
							<div
								class="absolute w-[2px] h-[2px] bg-white/40 rounded-full"
								style="
									top: {20 + Math.random() * 30}%;
									left: {Math.random() * 100}%;
									box-shadow: 0 0 {Math.random() * 5 + 2}px rgba(255,255,255,0.5);
								"
							></div>
							{/each}
						</div>
					</div>
				</div>

				<!-- 2. Floating Markers (Unclipped) -->
				<div class="absolute inset-0 animate-[spin_240s_linear_infinite]">
					{#each events as event}
					<div
						class="absolute"
						style="top: {event.y}%; left: {event.x}%;"
					>
						<!-- Content Container (Counter-rotated) -->
						<div class="animate-[spin_240s_linear_infinite_reverse] flex flex-col items-center gap-2 -translate-y-full pb-2">
							<!-- Bubble -->
							<div class="relative w-max max-w-[140px] rounded-xl border border-white/10 bg-black/60 p-2.5 backdrop-blur-md shadow-2xl transition-all hover:bg-black/80 hover:scale-105">
								<div class="flex items-center gap-1.5 mb-1">
									<div class="h-1.5 w-1.5 rounded-full bg-indigo-400 shadow-[0_0_6px_rgba(129,140,248,0.8)]"></div>
									<span class="text-[9px] uppercase tracking-wider text-white/50">{event.category}</span>
								</div>
								<div class="text-[11px] font-medium text-white/90 leading-none">{event.title}</div>

								<!-- Decorative corner accents -->
								<div class="absolute top-0 left-0 w-2 h-2 border-t border-l border-white/20 rounded-tl-md"></div>
								<div class="absolute bottom-0 right-0 w-2 h-2 border-b border-r border-white/20 rounded-br-md"></div>
							</div>

							<!-- Marker + Tether -->
							<div class="flex flex-col items-center">
								<div class="w-[1px] h-8 bg-gradient-to-b from-white/20 to-transparent"></div>
								<div class="w-1.5 h-1.5 bg-white rounded-full shadow-[0_0_10px_white] relative">
									<div class="absolute inset-0 animate-ping bg-white/50 rounded-full opacity-75"></div>
								</div>
							</div>
						</div>
					</div>
					{/each}
				</div>

			</div>
		</div>
	</main>

	<!-- Footer -->
	<footer class="fixed bottom-6 w-full px-6 md:px-12 flex justify-between items-end pointer-events-none z-40 mix-blend-difference text-white/40">
		<div class="hidden md:flex flex-col gap-1 text-[10px] font-mono uppercase tracking-widest">
			<span>System: Online</span>
			<span>Loc: Global</span>
		</div>

		<div class="text-[10px] font-mono uppercase tracking-widest text-right">
			© {new Date().getFullYear()} Kart Inc.<br />
			All rights reserved.
		</div>
	</footer>
</div>
