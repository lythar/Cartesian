<script lang="ts">
	import { Button } from "$lib/components/ui/button";
	import { PlusSignIcon } from "@hugeicons/core-free-icons";
	import { HugeiconsIcon } from "@hugeicons/svelte";

</script>

<div class="absolute bottom-8 right-8 z-20 flex flex-col items-end gap-3">
	<Button
		variant="default"
    size="lg"
		class="shadow-2xl transition-all duration-200"
	>
    <HugeiconsIcon icon={PlusSignIcon} size={24} strokeWidth={2} />
    <span class="font-bold">Create an Event</span>
  </Button>
</div>
