<script lang="ts">
	import type { CartesianUserDto, ChatMessageDto } from "$lib/api/cartesian-client";
	import * as Avatar from "$lib/components/ui/avatar";
	import { cn } from "$lib/utils";
	import { format } from "date-fns";

	let {
		message,
		author,
		isCurrentUser
	} = $props<{
		message: ChatMessageDto;
		author: CartesianUserDto | undefined;
		isCurrentUser: boolean;
	}>();
</script>

<div class={cn("flex w-full gap-3", isCurrentUser ? "flex-row-reverse" : "flex-row")}>
	<Avatar.Root class="h-8 w-8 shrink-0">
		<Avatar.Image src={author?.avatar?.url} alt={author?.name} class="object-cover" />
		<Avatar.Fallback class="bg-primary/10 text-xs font-medium text-primary">
			{author?.name?.substring(0, 2).toUpperCase() ?? "??"}
		</Avatar.Fallback>
	</Avatar.Root>

	<div class={cn("flex max-w-[70%] flex-col", isCurrentUser ? "items-end" : "items-start")}>
		<div class="flex items-center gap-2">
			<span class="text-xs font-medium text-muted-foreground">
				{author?.name ?? "Unknown User"}
			</span>
			<span class="text-[10px] text-muted-foreground/60">
				{format(new Date(message.createdAt as string), "HH:mm")}
			</span>
		</div>

		<div
			class={cn(
				"mt-1 rounded-2xl px-4 py-2 text-sm",
				isCurrentUser
					? "bg-primary text-primary-foreground rounded-tr-none"
					: "bg-muted text-foreground rounded-tl-none"
			)}
		>
			<p class="whitespace-pre-wrap break-words">{message.content}</p>
		</div>
	</div>
</div>
