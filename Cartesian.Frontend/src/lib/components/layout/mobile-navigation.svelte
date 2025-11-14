<script lang="ts">
	import { HugeiconsIcon } from "@hugeicons/svelte";

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

	let drawerY = $state(0);
	let isDragging = $state(false);
	let startY = $state(0);
	let initialDrawerY = $state(0);
	let viewportHeight = $state(typeof window !== 'undefined' ? window.innerHeight : 800);

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

	if (typeof window !== 'undefined') {
		window.addEventListener('resize', () => {
			viewportHeight = window.innerHeight;
		});
	}

	$effect(() => {
		if (!drawerElement) return;

		drawerElement.addEventListener('touchstart', handleTouchStart);
		drawerElement.addEventListener('touchmove', handleTouchMove, { passive: false });
		drawerElement.addEventListener('touchend', handleTouchEnd);

		return () => {
			drawerElement?.removeEventListener('touchstart', handleTouchStart);
			drawerElement?.removeEventListener('touchmove', handleTouchMove);
			drawerElement?.removeEventListener('touchend', handleTouchEnd);
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

<style>
	:global(.drawer-snap) {
		transition: transform 300ms cubic-bezier(0.25,1,0.5,1);
	}
</style>

<div class="fixed bottom-0 left-0 right-0 z-50 md:hidden">
	<div
		bind:this={drawerElement}
		class="fixed left-0 right-0 bg-background shadow-neu-highlight rounded-t-4xl pb-4 {isDragging ? '' : 'drawer-snap'}"
		style="bottom: {NAV_HEIGHT}px; transform: translateY({drawerHeight - DRAWER_PEEK - drawerY}px); height: {drawerHeight}px;"
		role="dialog"
	>
		<div class="w-full flex justify-center pt-3 pb-2 cursor-grab active:cursor-grabbing">
			<div class="w-12 h-1.5 bg-muted-foreground/30 rounded-full"></div>
		</div>

		<div class="px-4 py-2 overflow-y-auto pb-4" style="max-height: {drawerHeight - 60}px;">
			<p class="text-muted-foreground text-sm">Drawer content goes here</p>
		</div>
	</div>

	<nav class="fixed bottom-0 left-0 right-0 bg-sidebar z-50" style="height: {NAV_HEIGHT}px;">
		<div class="flex items-center justify-around h-full px-4">
			{#each navigationElements as element (element.href)}
				<a
					href={element.href}
					class="flex flex-col items-center justify-center gap-1 flex-1 py-2 text-muted-foreground hover:text-foreground transition-colors active:scale-95"
				>
					<HugeiconsIcon icon={element.icon} strokeWidth={2} className="w-6 h-6" />
					<span class="text-xs font-medium">{translationFunction(element.name)}</span>
				</a>
			{/each}
		</div>
	</nav>
</div>
