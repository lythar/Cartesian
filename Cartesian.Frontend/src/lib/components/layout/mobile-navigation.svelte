<script lang="ts">
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { page } from "$app/stores";

	interface NavigationElement {
		name: string;
		href: string;
		icon: any;
	}

	interface Props {
		navigationElements: NavigationElement[];
		translationFunction: (key: string) => string;
	}

	const { navigationElements, translationFunction }: Props = $props();

	let isActive = (href: string) => $page.url.pathname === href;

	let drawerY = $state(0);
	let isDragging = $state(false);
	let startY = $state(0);
	let initialDrawerY = $state(0);
	let viewportHeight = $state(typeof window !== "undefined" ? window.innerHeight : 800);

	const NAV_HEIGHT = 72;
	const DRAWER_PEEK = 80;
	const SNAP_THRESHOLD = 50;
	const MAX_DRAWER_HEIGHT = 800;

	let drawerHeight = $derived.by(() => {
		const availableHeight = viewportHeight - NAV_HEIGHT;
		return Math.min(MAX_DRAWER_HEIGHT, Math.max(availableHeight * 0.85, 300));
	});

	let snapPointOpen = $derived(Math.max(drawerHeight * 0.85, 200));

	let drawerElement: HTMLDivElement | undefined = $state();

	if (typeof window !== "undefined") {
		window.addEventListener("resize", () => {
			viewportHeight = window.innerHeight;
		});
	}

	$effect(() => {
		if (!drawerElement) return;

		drawerElement.addEventListener("touchstart", handleTouchStart);
		drawerElement.addEventListener("touchmove", handleTouchMove, { passive: false });
		drawerElement.addEventListener("touchend", handleTouchEnd);

		return () => {
			drawerElement?.removeEventListener("touchstart", handleTouchStart);
			drawerElement?.removeEventListener("touchmove", handleTouchMove);
			drawerElement?.removeEventListener("touchend", handleTouchEnd);
		};
	});

	function handleTouchStart(e: TouchEvent) {
		isDragging = true;
		startY = e.touches[0].clientY;
		initialDrawerY = drawerY;
	}

	function handleTouchMove(e: TouchEvent) {
		if (!isDragging) return;

		e.preventDefault();

		const deltaY = startY - e.touches[0].clientY;
		const newY = Math.max(0, Math.min(snapPointOpen, initialDrawerY + deltaY));
		drawerY = newY;
	}

	function handleTouchEnd() {
		if (!isDragging) return;
		isDragging = false;

		const dragDistance = drawerY - initialDrawerY;

		if (Math.abs(dragDistance) > SNAP_THRESHOLD) {
			if (dragDistance > 0) {
				drawerY = snapPointOpen;
			} else {
				drawerY = 0;
			}
		} else {
			const closest = [0, snapPointOpen].reduce((prev: number, curr: number) => {
				return Math.abs(curr - drawerY) < Math.abs(prev - drawerY) ? curr : prev;
			});
			drawerY = closest;
		}
	}
</script>

<div class="fixed right-0 bottom-0 left-0 z-50 md:hidden">
	<div
		bind:this={drawerElement}
		class="fixed right-0 left-0 rounded-t-4xl bg-background pb-4 shadow-neu-highlight {isDragging
			? ''
			: 'drawer-snap'}"
		style="bottom: {NAV_HEIGHT}px; transform: translateY({drawerHeight -
			DRAWER_PEEK -
			drawerY}px); height: {drawerHeight}px;"
		role="dialog"
	>
		<div class="flex w-full cursor-grab justify-center pt-3 pb-2 active:cursor-grabbing">
			<div class="h-1.5 w-12 rounded-full bg-muted-foreground/30"></div>
		</div>

		<div class="overflow-y-auto px-4 py-2 pb-4" style="max-height: {drawerHeight - 60}px;">
			<p class="text-sm text-muted-foreground">Drawer content goes here</p>
		</div>
	</div>

	<nav class="fixed right-0 bottom-0 left-0 z-50 bg-sidebar" style="height: {NAV_HEIGHT}px;">
		<div class="flex h-full items-center justify-around px-4">
			{#each navigationElements as element (element.href)}
				<a
					href={element.href}
					class="flex flex-1 flex-col items-center justify-center gap-1 border-b-2 py-2 transition-colors active:scale-95 {isActive(
						element.href,
					)
						? 'border-primary text-foreground'
						: 'border-transparent text-muted-foreground hover:text-foreground'}"
				>
					<HugeiconsIcon icon={element.icon} strokeWidth={2} className="w-6 h-6" />
					<span class="text-xs font-medium">{translationFunction(element.name)}</span>
				</a>
			{/each}
		</div>
	</nav>
</div>

<style>
	:global(.drawer-snap) {
		transition: transform 300ms cubic-bezier(0.25, 1, 0.5, 1);
	}
</style>
