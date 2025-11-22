<script lang="ts">
	import { Button } from "$lib/components/ui/button";
	import { cn } from "$lib/utils";
	import { PlusSignIcon } from "@hugeicons/core-free-icons";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { newEventOverlayState } from "./map-state.svelte";
	import { authStore } from "$lib/stores/auth.svelte";
	import LoginAlertDialog from "$lib/components/auth/login-alert-dialog.svelte";

	let loginAlertOpen = $state(false);
</script>

<LoginAlertDialog bind:open={loginAlertOpen} />

<div class="absolute bottom-8 right-8 z-20 flex flex-col items-end gap-3">
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
		class={cn("shadow-2xl transition-all duration-200", newEventOverlayState.open ? "scale-0 opacity-0 pointer-events-none" : "scale-100 opacity-100")}
	>
    <HugeiconsIcon icon={PlusSignIcon} size={24} strokeWidth={2} />
    <span class="font-bold">Create an Event</span>
  </Button>
</div>
