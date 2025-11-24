<script lang="ts">
	import { Button } from "$lib/components/ui/button";
	import { Textarea } from "$lib/components/ui/textarea";
	import { SentIcon } from "@hugeicons/core-free-icons";
	import { HugeiconsIcon } from "@hugeicons/svelte";

	let { onSend, disabled = false } = $props<{
		onSend: (content: string) => void;
		disabled?: boolean;
	}>();

	let content = $state("");

	function handleSend() {
		if (!content.trim() || disabled) return;
		onSend(content);
		content = "";
	}

	function handleKeydown(e: KeyboardEvent) {
		if (e.key === "Enter" && !e.shiftKey) {
			e.preventDefault();
			handleSend();
		}
	}
</script>

<div class="flex items-end gap-2 rounded-3xl border border-border/40 bg-card p-2 shadow-sm">
	<Textarea
		bind:value={content}
		onkeydown={handleKeydown}
		placeholder="Type a message..."
		class="min-h-[44px] w-full resize-none border-0 bg-transparent px-4 py-3 focus-visible:ring-0 focus-visible:ring-offset-0"
		rows={1}
		{disabled}
	/>
	<Button
		size="icon"
		variant="ghost"
		class="h-10 w-10 shrink-0 rounded-full text-primary hover:bg-primary/10 hover:text-primary"
		onclick={handleSend}
		disabled={!content.trim() || disabled}
	>
		<HugeiconsIcon icon={SentIcon} size={20} strokeWidth={2} />
		<span class="sr-only">Send</span>
	</Button>
</div>
