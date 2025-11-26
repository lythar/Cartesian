<script lang="ts">
	import { goto } from "$app/navigation";
	import type {
		CartesianUserDto,
		ChatMessageDto,
		ReactionSummaryDto,
	} from "$lib/api/cartesian-client";
	import {
		getEventApiEventId,
		postChatApiMessageMessageIdReact,
		deleteChatApiMessageMessageIdReact,
		postChatApiChannelChannelIdPinMessageId,
		deleteChatApiChannelChannelIdPinMessageId,
	} from "$lib/api/cartesian-client";
	import { baseUrl } from "$lib/api/client";
	import * as Avatar from "$lib/components/ui/avatar";
	import * as Popover from "$lib/components/ui/popover";
	import * as Tooltip from "$lib/components/ui/tooltip";
	import { cn } from "$lib/utils";
	import { parseMessageContent, getCachedEmoji } from "$lib/utils/markdown";
	import { format } from "date-fns";
	import { openUserProfile } from "$lib/components/profile/profile-state.svelte";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { Add01Icon, Pin02Icon } from "@hugeicons/core-free-icons";

	let {
		message,
		author,
		isCurrentUser,
		isStacked = false,
		currentUserId = "",
		channelId = "",
		isPinned = false,
		onReactionUpdate,
		onPinUpdate,
	} = $props<{
		message: ChatMessageDto;
		author: CartesianUserDto | undefined;
		isCurrentUser: boolean;
		isStacked?: boolean;
		currentUserId?: string;
		channelId?: string;
		isPinned?: boolean;
		onReactionUpdate?: (messageId: string, reactions: ReactionSummaryDto[]) => void;
		onPinUpdate?: () => void;
	}>();

	interface EventEmbed {
		id: string;
		name: string;
		description: string;
		authorName: string;
		loading: boolean;
		error: boolean;
	}

	const QUICK_REACTIONS = ["👍", "❤️", "😂", "😮", "😢", "🎉"];

	let eventEmbeds = $state<EventEmbed[]>([]);
	let loadedEventIds = $state<string[]>([]);
	let isHovered = $state(false);
	let showReactionPicker = $state(false);
	let isReacting = $state(false);
	let isPinning = $state(false);

	const parsedContent = $derived(parseMessageContent(message.content));
	const reactions = $derived(message.reactionSummary ?? []);
	const showToolbar = $derived(isHovered || showReactionPicker);

	function handleAuthorClick() {
		if (author?.id) {
			openUserProfile(author.id);
		}
	}

	$effect(() => {
		const { eventLinks } = parsedContent;
		const newLinks = eventLinks.filter((id) => !loadedEventIds.includes(id));
		if (newLinks.length > 0) {
			loadedEventIds = [...loadedEventIds, ...newLinks];
			loadEventEmbeds(newLinks);
		}
	});

	async function loadEventEmbeds(eventIds: string[]) {
		const initialEmbeds: EventEmbed[] = eventIds.map((id) => ({
			id,
			name: "",
			description: "",
			authorName: "",
			loading: true,
			error: false,
		}));
		eventEmbeds = [...eventEmbeds, ...initialEmbeds];

		for (const eventId of eventIds) {
			try {
				const event = await getEventApiEventId(eventId);
				eventEmbeds = eventEmbeds.map((embed) =>
					embed.id === eventId
						? {
								id: eventId,
								name: event.name,
								description: event.description ?? "",
								authorName: event.author.name,
								loading: false,
								error: false,
							}
						: embed,
				);
			} catch {
				eventEmbeds = eventEmbeds.map((embed) =>
					embed.id === eventId
						? {
								...embed,
								loading: false,
								error: true,
							}
						: embed,
				);
			}
		}
	}

	function handleEventClick(eventId: string) {
		goto(`/app?event=${eventId}`);
	}

	function getAvatarUrl(avatar: { id: string } | null | undefined): string | undefined {
		if (!avatar) return undefined;
		return `${baseUrl}/media/api/${avatar.id}`;
	}

	async function handlePin() {
		if (isPinning || !channelId) return;
		isPinning = true;

		try {
			if (isPinned) {
				await deleteChatApiChannelChannelIdPinMessageId(channelId, message.id);
			} else {
				await postChatApiChannelChannelIdPinMessageId(channelId, message.id);
			}
			onPinUpdate?.();
		} catch (e) {
			console.error("Failed to toggle pin", e);
		} finally {
			isPinning = false;
		}
	}

	async function handleReaction(emoji: string) {
		if (isReacting) return;
		isReacting = true;
		showReactionPicker = false;

		try {
			const existingReaction = reactions.find(
				(r: ReactionSummaryDto) => r.emoji === emoji && r.currentUserReacted,
			);

			if (existingReaction) {
				await deleteChatApiMessageMessageIdReact(message.id, { emoji });
				const updatedReactions = reactions
					.map((r: ReactionSummaryDto) => {
						if (r.emoji === emoji) {
							const newCount = Number(r.count) - 1;
							if (newCount <= 0) return null;
							return {
								...r,
								count: newCount,
								currentUserReacted: false,
								userIds: r.userIds.filter((id: string) => id !== currentUserId),
							};
						}
						return r;
					})
					.filter((r: ReactionSummaryDto | null): r is ReactionSummaryDto => r !== null);
				onReactionUpdate?.(message.id, updatedReactions);
			} else {
				await postChatApiMessageMessageIdReact(message.id, { emoji });
				const existingIdx = reactions.findIndex(
					(r: ReactionSummaryDto) => r.emoji === emoji,
				);
				let updatedReactions: ReactionSummaryDto[];

				if (existingIdx >= 0) {
					updatedReactions = reactions.map((r: ReactionSummaryDto, idx: number) => {
						if (idx === existingIdx) {
							return {
								...r,
								count: Number(r.count) + 1,
								currentUserReacted: true,
								userIds: [...r.userIds, currentUserId],
							};
						}
						return r;
					});
				} else {
					updatedReactions = [
						...reactions,
						{
							emoji,
							count: 1,
							currentUserReacted: true,
							userIds: [currentUserId],
						},
					];
				}
				onReactionUpdate?.(message.id, updatedReactions);
			}
		} catch (e) {
			console.error("Failed to toggle reaction", e);
		} finally {
			isReacting = false;
		}
	}
</script>

<!-- svelte-ignore a11y_no_static_element_interactions -->
<div
	class={cn(
		"group relative -mx-4 flex w-full gap-3 px-4 transition-colors",
		isStacked ? "hover:bg-muted/30" : "pt-3 hover:bg-muted/30",
		isPinned && "bg-amber-500/5",
	)}
	onmouseenter={() => (isHovered = true)}
	onmouseleave={() => {
		if (!showReactionPicker) {
			isHovered = false;
		}
	}}
>
	{#if !isStacked}
		<button class="mt-0.5 h-8 w-8 shrink-0" onclick={handleAuthorClick} type="button">
			<Avatar.Root class="h-8 w-8 border border-border/20">
				<Avatar.Image
					src={getAvatarUrl(author?.avatar)}
					alt={author?.name}
					class="object-cover"
				/>
				<Avatar.Fallback class="bg-primary/5 text-[10px] font-medium text-foreground/70">
					{author?.name?.substring(0, 2).toUpperCase() ?? "??"}
				</Avatar.Fallback>
			</Avatar.Root>
		</button>
	{:else}
		<div class="flex w-8 shrink-0 items-start justify-end">
			<span
				class="absolute top-1/2 hidden -translate-y-1/2 pr-1 text-[10px] text-muted-foreground/50 group-hover:block"
			>
				{format(new Date(message.createdAt as string), "h:mm a")}
			</span>
		</div>
	{/if}

	<div class="flex min-w-0 flex-1 flex-col">
		{#if !isStacked}
			<div class="flex items-center gap-2">
				<button
					type="button"
					onclick={handleAuthorClick}
					class={cn(
						"cursor-pointer text-xs leading-none font-semibold hover:underline",
						isCurrentUser ? "text-primary" : "text-foreground",
					)}
				>
					{author?.name ?? "Unknown User"}
				</button>
				<span class="text-[10px] whitespace-nowrap text-muted-foreground/50 select-none">
					{format(new Date(message.createdAt as string), "h:mm a")}
				</span>
			</div>
		{/if}
		<div
			class={cn(
				"message-content text-sm leading-relaxed wrap-break-word text-foreground/90",
				isStacked ? "" : "mt-0.5",
			)}
		>
			{@html parsedContent.html}
		</div>

		{#if eventEmbeds.length > 0}
			<div class="mt-2 space-y-2">
				{#each eventEmbeds as embed}
					{#if embed.loading}
						<div
							class="flex items-center gap-3 rounded-lg border border-border/40 bg-muted/30 p-3"
						>
							<div class="h-12 w-12 animate-pulse rounded-md bg-muted"></div>
							<div class="flex-1 space-y-2">
								<div class="h-4 w-32 animate-pulse rounded bg-muted"></div>
								<div class="h-3 w-48 animate-pulse rounded bg-muted"></div>
							</div>
						</div>
					{:else if embed.error}
						<div
							class="rounded-lg border border-border/40 bg-muted/30 p-3 text-xs text-muted-foreground"
						>
							Failed to load event
						</div>
					{:else}
						<button
							onclick={() => handleEventClick(embed.id)}
							class="flex w-full items-start gap-3 rounded-lg border border-border/40 bg-muted/20 p-3 text-left transition-colors hover:bg-muted/40"
						>
							<div
								class="flex h-14 w-14 items-center justify-center rounded-md bg-primary/10"
							>
								<svg
									xmlns="http://www.w3.org/2000/svg"
									width="24"
									height="24"
									viewBox="0 0 24 24"
									fill="none"
									stroke="currentColor"
									stroke-width="2"
									stroke-linecap="round"
									stroke-linejoin="round"
									class="text-primary"
									><path d="M8 2v4" /><path d="M16 2v4" /><rect
										width="18"
										height="18"
										x="3"
										y="4"
										rx="2"
									/><path d="M3 10h18" /></svg
								>
							</div>
							<div class="min-w-0 flex-1">
								<h4 class="truncate text-sm font-medium text-foreground">
									{embed.name}
								</h4>
								<p class="line-clamp-2 text-xs text-muted-foreground">
									{embed.description}
								</p>
								<p class="mt-1 text-[10px] text-muted-foreground/70">
									by {embed.authorName}
								</p>
							</div>
							<div class="flex items-center text-xs text-primary">
								<span>View</span>
								<svg
									xmlns="http://www.w3.org/2000/svg"
									width="14"
									height="14"
									viewBox="0 0 24 24"
									fill="none"
									stroke="currentColor"
									stroke-width="2"
									stroke-linecap="round"
									stroke-linejoin="round"
									class="ml-1"><path d="m9 18 6-6-6-6" /></svg
								>
							</div>
						</button>
					{/if}
				{/each}
			</div>
		{/if}

		{#if reactions.length > 0}
			<div class="mt-1.5 flex flex-wrap gap-1">
				{#each reactions as reaction}
					<button
						type="button"
						onclick={() => handleReaction(reaction.emoji)}
						disabled={isReacting}
						class={cn(
							"inline-flex items-center gap-1 rounded-full border px-2 py-0.5 text-xs transition-colors",
							reaction.currentUserReacted
								? "border-primary/40 bg-primary/10 text-primary hover:bg-primary/20"
								: "border-border/50 bg-muted/30 text-muted-foreground hover:bg-muted/50",
						)}
					>
						<span class="reaction-emoji">{@html getCachedEmoji(reaction.emoji)}</span>
						<span class="text-[10px] font-medium">{reaction.count}</span>
					</button>
				{/each}
			</div>
		{/if}
	</div>

	{#if showToolbar}
		<div
			class="absolute top-0 right-2 z-10 -translate-y-1/2"
			onmouseenter={() => (isHovered = true)}
			onmouseleave={() => {
				if (!showReactionPicker) {
					isHovered = false;
				}
			}}
		>
			<div
				class="flex items-center gap-0.5 rounded-md border border-border/50 bg-background p-0.5 shadow-md"
			>
				{#each QUICK_REACTIONS as emoji}
					<button
						type="button"
						onclick={() => handleReaction(emoji)}
						disabled={isReacting}
						class="rounded p-1 transition-colors hover:bg-muted"
						title={`React with ${emoji}`}
					>
						<span class="reaction-picker-emoji">{@html getCachedEmoji(emoji)}</span>
					</button>
				{/each}
				<Popover.Root
					bind:open={showReactionPicker}
					onOpenChange={(open) => {
						showReactionPicker = open;
						if (!open) {
							isHovered = false;
						}
					}}
				>
					<Popover.Trigger class="rounded p-1 transition-colors hover:bg-muted">
						<HugeiconsIcon icon={Add01Icon} className="size-4 text-muted-foreground" />
					</Popover.Trigger>
					<Popover.Content class="w-auto p-2" side="top" align="end">
						<div class="grid grid-cols-8 gap-1">
							{#each ["😀", "😃", "😄", "😁", "😆", "😅", "🤣", "😂", "🙂", "🙃", "😉", "😊", "😇", "🥰", "😍", "🤩", "😘", "😗", "😚", "😙", "🥲", "😋", "😛", "😜", "🤪", "😝", "🤑", "🤗", "🤭", "🤫", "🤔", "🤐", "🤨", "😐", "😑", "😶", "😏", "😒", "🙄", "😬", "🤥", "😌", "😔", "😪", "🤤", "😴", "😷", "🤒", "🤕", "🤢", "🤮", "🤧", "🥵", "🥶", "🥴", "😵", "🤯", "🤠", "🥳", "🥸", "😎", "🤓", "🧐", "👍", "👎", "👏", "🙌", "🤝", "💪", "❤️", "🧡", "💛", "💚", "💙", "💜", "🖤", "🤍", "💔", "❣️", "💕", "💞", "💓", "💗", "💖", "💝", "💘", "🎉", "🎊", "✨", "🔥", "💯", "⭐", "🌟", "💫", "✅", "❌"] as emoji}
								<button
									type="button"
									onclick={() => handleReaction(emoji)}
									disabled={isReacting}
									class="rounded p-1.5 transition-colors hover:bg-muted"
								>
									<span class="reaction-picker-emoji"
										>{@html getCachedEmoji(emoji)}</span
									>
								</button>
							{/each}
						</div>
					</Popover.Content>
				</Popover.Root>

				<div class="mx-0.5 h-4 w-px bg-border/50"></div>

				<Tooltip.Root>
					<Tooltip.Trigger>
						<button
							type="button"
							onclick={handlePin}
							disabled={isPinning}
							class={cn(
								"rounded p-1 transition-colors hover:bg-muted",
								isPinned && "text-amber-500",
							)}
						>
							<HugeiconsIcon icon={Pin02Icon} className="size-4" />
						</button>
					</Tooltip.Trigger>
					<Tooltip.Content>{isPinned ? "Unpin message" : "Pin message"}</Tooltip.Content>
				</Tooltip.Root>
			</div>
		</div>
	{/if}
</div>

<style>
	:global(.message-content p) {
		margin: 0;
	}
	:global(.message-content p + p) {
		margin-top: 0.5rem;
	}
	:global(.message-content code) {
		background: hsl(var(--muted));
		padding: 0.125rem 0.375rem;
		border-radius: 0.25rem;
		font-size: 0.85em;
	}
	:global(.message-content pre) {
		background: hsl(var(--muted));
		padding: 0.75rem;
		border-radius: 0.375rem;
		overflow-x: auto;
		margin: 0.5rem 0;
	}
	:global(.message-content pre code) {
		background: transparent;
		padding: 0;
	}
	:global(.message-content blockquote) {
		border-left: 3px solid hsl(var(--border));
		padding-left: 0.75rem;
		margin: 0.5rem 0;
		color: hsl(var(--muted-foreground));
	}
	:global(.message-content a) {
		color: hsl(var(--primary));
		text-decoration: underline;
	}
	:global(.message-content a:hover) {
		opacity: 0.8;
	}
	:global(.message-content ul),
	:global(.message-content ol) {
		margin: 0.25rem 0;
		padding-left: 1.25rem;
	}
	:global(.message-content img.emoji) {
		height: 1.2em;
		width: 1.2em;
		margin: 0 0.05em;
		vertical-align: -0.1em;
		display: inline;
	}
	:global(.message-content img.twemoji) {
		height: 1.2em;
		width: 1.2em;
		margin: 0 0.05em;
		vertical-align: -0.1em;
		display: inline;
	}
	:global(.reaction-emoji img.twemoji) {
		height: 1em;
		width: 1em;
		display: inline;
		vertical-align: -0.1em;
	}
	:global(.reaction-picker-emoji img.twemoji) {
		height: 1.25rem;
		width: 1.25rem;
		display: inline;
	}
</style>
