<script lang="ts">
	import type { CartesianUserDto, ChatMessageDto } from "$lib/api/cartesian-client";
	import * as Avatar from "$lib/components/ui/avatar";
	import { cn } from "$lib/utils";
	import { format } from "date-fns";

	let {
		message,
		author,
		isCurrentUser,
		isStacked = false,
	} = $props<{
		message: ChatMessageDto;
		author: CartesianUserDto | undefined;
		isCurrentUser: boolean;
		isStacked?: boolean;
	}>();
</script>

<div
	class={cn(
		"group flex w-full gap-3 -mx-4 px-4 transition-colors relative",
		isStacked ? "py-0.5 hover:bg-muted/30" : "py-2 mt-1 hover:bg-muted/30",
	)}
>
	{#if !isStacked}
		<Avatar.Root class="h-8 w-8 shrink-0 mt-0.5 border border-border/20">
			<Avatar.Image src={author?.avatar?.url} alt={author?.name} class="object-cover" />
			<Avatar.Fallback class="bg-primary/5 text-[10px] font-medium text-foreground/70">
				{author?.name?.substring(0, 2).toUpperCase() ?? "??"}
			</Avatar.Fallback>
		</Avatar.Root>
	{:else}
		<div class="h-8 w-8 shrink-0 flex justify-end">
			<span class="hidden group-hover:block text-[10px] text-muted-foreground/50 pt-1 pr-1">
				{format(new Date(message.createdAt as string), "h:mm a")}
			</span>
		</div>
	{/if}

	<div class="flex flex-col min-w-0 flex-1">
		{#if !isStacked}
			<div class="flex items-center gap-2">
				<span
					class={cn(
						"text-xs font-semibold leading-none hover:underline cursor-pointer",
						isCurrentUser ? "text-primary" : "text-foreground",
					)}
				>
					{author?.name ?? "Unknown User"}
				</span>
				<span class="text-[10px] text-muted-foreground/50 select-none">
					{format(new Date(message.createdAt as string), "h:mm a")}
				</span>
			</div>
		{/if}

		<p
			class={cn(
				"text-sm leading-relaxed text-foreground/90 whitespace-pre-wrap break-words",
				isStacked ? "" : "mt-0.5",
			)}
		>
			{message.content}
		</p>
	</div>
</div>
