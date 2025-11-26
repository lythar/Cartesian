<script lang="ts">
	import { Button } from "$lib/components/ui/button";
	import { cn } from "$lib/utils";
	import { PlusSignIcon } from "@hugeicons/core-free-icons";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { newEventOverlayState } from "./map-state.svelte";
	import { authStore } from "$lib/stores/auth.svelte";
	import LoginAlertDialog from "$lib/components/auth/login-alert-dialog.svelte";
	import { getLayoutContext } from "$lib/context/layout.svelte";
	import * as m from "$lib/paraglide/messages";

	const layout = getLayoutContext();

	let loginAlertOpen = $state(false);
</script>

<LoginAlertDialog bind:open={loginAlertOpen} />

<div class="absolute right-4 bottom-8 z-20 flex flex-col items-end gap-3 lg:right-8">
	{#if layout.isMobile}
		<Button
			variant="secondary"
			size="icon-lg"
			onclick={() => {
				if (!$authStore.isAuthenticated) {
					loginAlertOpen = true;
					return;
				}
				newEventOverlayState.open = true;
				newEventOverlayState.source = "create";
			}}
			class={cn(
				"size-16 rounded-3xl shadow-neu-depth! transition-all duration-200 corner-squircle",
				newEventOverlayState.open
					? "pointer-events-none scale-0 opacity-0"
					: "scale-100 opacity-100",
			)}
		>
			<HugeiconsIcon
				icon={PlusSignIcon}
				size={24}
				strokeWidth={2}
				className="duotone-fill size-7"
			/>
		</Button>
	{:else}
		<Button
			variant="default"
			size="lg"
			onclick={() => {
				if (!$authStore.isAuthenticated) {
					loginAlertOpen = true;
					return;
				}
				newEventOverlayState.open = true;
				newEventOverlayState.source = "create";
			}}
			class={cn(
				"shadow-2xl transition-all duration-200",
				newEventOverlayState.open
					? "pointer-events-none scale-0 opacity-0"
					: "scale-100 opacity-100",
			)}
		>
			<HugeiconsIcon icon={PlusSignIcon} size={24} strokeWidth={2} />
			<span class="font-bold">{m.create_an_event()}</span>
		</Button>
	{/if}
</div>
