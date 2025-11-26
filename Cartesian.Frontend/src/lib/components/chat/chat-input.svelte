<script lang="ts">
	import { Button } from "$lib/components/ui/button";
	import { Textarea } from "$lib/components/ui/textarea";
	import { SentIcon } from "@hugeicons/core-free-icons";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import * as m from "$lib/paraglide/messages";

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

<div
	class="relative flex w-full items-end gap-2 rounded-xl bg-muted/40 ring-offset-background focus-within:ring-1 focus-within:ring-ring"
>
	<Textarea
		bind:value={content}
		onkeydown={handleKeydown}
		placeholder={m.message_placeholder()}
		class="min-h-[44px] w-full resize-none border-0 bg-transparent px-4 py-3 placeholder:text-muted-foreground/50 focus-visible:ring-0 focus-visible:ring-offset-0"
		rows={1}
		{disabled}
	/>
	<div class="pr-1.5 pb-1.5">
		<Button
			size="icon"
			variant="ghost"
			class="h-8 w-8 shrink-0 rounded-lg text-muted-foreground transition-colors hover:bg-background hover:text-foreground"
			onclick={handleSend}
			disabled={!content.trim() || disabled}
		>
			<HugeiconsIcon icon={SentIcon} size={18} strokeWidth={2} />
			<span class="sr-only">{m.send()}</span>
		</Button>
	</div>
</div>
