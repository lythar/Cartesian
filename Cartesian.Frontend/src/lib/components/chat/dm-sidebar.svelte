<script lang="ts">
	import {
		createGetChatApiDmChannels,
		type DmChannelListItemDto,
	} from "$lib/api/cartesian-client";
	import * as Avatar from "$lib/components/ui/avatar";
	import { Skeleton } from "$lib/components/ui/skeleton";
	import { getAvatarUrl, getInitials, cn } from "$lib/utils";
	import { page } from "$app/stores";
	import { formatDistanceToNow } from "date-fns";
	import { openUserProfile } from "$lib/components/profile/profile-state.svelte";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { MessageMultiple01Icon } from "@hugeicons/core-free-icons";

	const recentDmsQuery = createGetChatApiDmChannels(
		{ limit: 10 },
		{
			query: {
				refetchInterval: 30000,
			},
		},
	);

	let channels = $derived(recentDmsQuery.data?.channels ?? []);

	function isActive(channelId: string) {
		return $page.url.pathname === `/app/dm/${channelId}`;
	}

	function formatTime(dateStr: unknown) {
		if (!dateStr) return "";
		return formatDistanceToNow(new Date(dateStr as string), { addSuffix: true });
	}

	function truncateMessage(msg: string | null | undefined, maxLength = 30) {
		if (!msg) return "No messages yet";
		return msg.length > maxLength ? msg.substring(0, maxLength) + "..." : msg;
	}
</script>

<div class="flex flex-col">
	<h3
		class="px-2 py-2 text-[10px] font-semibold tracking-wider text-muted-foreground/40 uppercase"
	>
		Direct Messages
	</h3>

	{#if recentDmsQuery.isLoading}
		<div class="space-y-1 px-1">
			{#each Array(3) as _}
				<div class="flex items-center gap-2 rounded-lg px-2 py-2">
					<Skeleton class="h-8 w-8 rounded-full" />
					<div class="flex-1 space-y-1">
						<Skeleton class="h-3 w-20" />
						<Skeleton class="h-2 w-24" />
					</div>
				</div>
			{/each}
		</div>
	{:else if recentDmsQuery.isError}
		<p class="px-2 text-xs text-destructive">Error loading messages</p>
	{:else if channels.length === 0}
		<div class="px-2 py-4 text-center">
			<HugeiconsIcon
				icon={MessageMultiple01Icon}
				size={24}
				className="mx-auto text-muted-foreground/30"
			/>
			<p class="mt-2 text-xs text-muted-foreground">No conversations yet</p>
		</div>
	{:else}
		<div class="space-y-0.5 px-1">
			{#each channels as channel (channel.channelId)}
				<a
					href="/app/dm/{channel.channelId}"
					class={cn(
						"group flex items-center gap-2.5 rounded-lg px-2 py-2 text-sm transition-colors",
						isActive(channel.channelId)
							? "bg-muted/50 text-foreground"
							: "text-muted-foreground hover:bg-muted/30 hover:text-foreground",
					)}
				>
					<button
						type="button"
						class="relative"
						onclick={(e) => {
							e.preventDefault();
							e.stopPropagation();
							if (channel.otherUserId) openUserProfile(channel.otherUserId);
						}}
					>
						<Avatar.Root class="h-8 w-8 border border-border/40">
							<Avatar.Image
								src={getAvatarUrl(channel.otherUser?.avatar ?? null)}
								alt={channel.otherUser?.name}
								class="object-cover"
							/>
							<Avatar.Fallback
								class="bg-muted text-[10px] font-medium text-muted-foreground"
							>
								{channel.otherUser?.name
									? getInitials(channel.otherUser.name)
									: "??"}
							</Avatar.Fallback>
						</Avatar.Root>
					</button>
					<div class="min-w-0 flex-1">
						<div class="flex items-center justify-between gap-2">
							<span class="truncate text-sm font-medium">
								{channel.otherUser?.name ?? "Unknown User"}
							</span>
							<span class="shrink-0 text-[10px] text-muted-foreground/50">
								{formatTime(channel.lastMessageAt)}
							</span>
						</div>
						<p class="truncate text-xs text-muted-foreground/70">
							{truncateMessage(channel.lastMessagePreview)}
						</p>
					</div>
				</a>
			{/each}
		</div>
	{/if}
</div>
