<script lang="ts">
	import type { MyUserDto, MembershipDto, CartesianUserDto, ReactionSummaryDto, PinnedChatMessageDto } from "$lib/api/cartesian-client";
	import ChatInput from "$lib/components/chat/chat-input.svelte";
	import ChatMessage from "$lib/components/chat/chat-message.svelte";
	import { ScrollArea } from "$lib/components/ui/scroll-area";
	import { Skeleton } from "$lib/components/ui/skeleton";
	import type { ChatState } from "./chat.state.svelte";
	import { untrack } from "svelte";

	let {
		chatState,
		members,
		currentUser,
		pinnedMessages = [],
		onPinUpdate,
	} = $props<{
		chatState: ChatState;
		members: MembershipDto[];
		currentUser: MyUserDto | undefined;
		pinnedMessages?: PinnedChatMessageDto[];
		onPinUpdate?: () => void;
	}>();

	const pinnedMessageIds = $derived(new Set(pinnedMessages.map((p: PinnedChatMessageDto) => p.message.id)));

	function getAuthor(userId: string): CartesianUserDto | undefined {
		const member = members.find((m: MembershipDto) => m.userId === userId);
		if (member) return member.user;
		if (currentUser?.id === userId) return currentUser;
		return undefined;
	}

	let scrollViewport = $state<HTMLElement | null>(null);
	let isLoadingMore = $state(false);
	let isInitialLoad = $state(true);

	function scrollToBottom() {
		if (scrollViewport) {
			setTimeout(() => {
				if (scrollViewport) {
					scrollViewport.scrollTop = scrollViewport.scrollHeight;
				}
			}, 50);
		}
	}

	async function handleScroll() {
		if (!scrollViewport || isLoadingMore) return;

		if (scrollViewport.scrollTop < 100 && chatState.hasMore && !chatState.isFetchingMore) {
			isLoadingMore = true;
			const prevScrollHeight = scrollViewport.scrollHeight;
			const prevScrollTop = scrollViewport.scrollTop;

			await chatState.loadMore();

			requestAnimationFrame(() => {
				requestAnimationFrame(() => {
					if (scrollViewport) {
						const newScrollHeight = scrollViewport.scrollHeight;
						const heightDiff = newScrollHeight - prevScrollHeight;
						scrollViewport.scrollTop = prevScrollTop + heightDiff;
					}
					isLoadingMore = false;
				});
			});
		}
	}

	$effect(() => {
		const msgCount = chatState.messages.length;
		const loading = chatState.isLoading;
		const shouldScroll = untrack(() => isInitialLoad);
		if (msgCount > 0 && shouldScroll && !loading) {
			setTimeout(() => scrollToBottom(), 100);
			isInitialLoad = false;
		}
	});

	$effect(() => {
		if (scrollViewport) {
			scrollViewport.addEventListener("scroll", handleScroll);
			return () => scrollViewport?.removeEventListener("scroll", handleScroll);
		}
	});

	async function handleSend(content: string) {
		await chatState.sendMessage(content);
		scrollToBottom();
	}

	function handleReactionUpdate(messageId: string, reactions: ReactionSummaryDto[]) {
		chatState.updateMessageReactions(messageId, reactions);
	}

	function handlePinUpdate() {
		onPinUpdate?.();
	}
</script>

<div class="flex flex-1 flex-col overflow-hidden relative">
	<!-- Messages Area -->
	<div class="absolute inset-0 bottom-[73px] overflow-hidden">
		{#if chatState.isLoading}
			<div class="absolute inset-0 p-4 space-y-4">
				{#each Array(3) as _}
					<div class="flex items-start gap-3">
						<Skeleton class="h-10 w-10 rounded-full" />
						<div class="space-y-2">
							<Skeleton class="h-4 w-[200px]" />
							<Skeleton class="h-4 w-[300px]" />
						</div>
					</div>
				{/each}
			</div>
		{:else}
			<ScrollArea class="h-full pr-4" bind:viewportRef={scrollViewport}>
				<div class="flex flex-col justify-end min-h-full py-4 px-4">
					{#if chatState.isFetchingMore}
						<div class="flex justify-center py-2">
							<Skeleton class="h-4 w-24" />
						</div>
					{/if}
					{#if chatState.messages.length === 0}
						<div class="flex flex-1 flex-col items-center justify-center text-center text-muted-foreground p-8">
							<div class="bg-muted/50 p-4 rounded-full mb-4">
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
									class="w-6 h-6"
									><path d="M7.9 20A9 9 0 1 0 4 16.1L2 22Z" /></svg
								>
							</div>
							<h3 class="font-semibold text-foreground mb-1">Welcome to the beginning</h3>
							<p class="text-sm">Be the first to say something.</p>
						</div>
					{:else}
						{#each chatState.messages as message, index (message.id)}
							{@const previousMessage = chatState.messages[index - 1]}
							{@const isStacked =
								previousMessage &&
								previousMessage.authorId === message.authorId}
							<ChatMessage
								{message}
								author={getAuthor(message.authorId)}
								isCurrentUser={message.authorId === currentUser?.id}
								{isStacked}
								currentUserId={currentUser?.id ?? ""}
								channelId={chatState.channelId ?? ""}
								isPinned={pinnedMessageIds.has(message.id)}
								onReactionUpdate={handleReactionUpdate}
								onPinUpdate={handlePinUpdate}
							/>
						{/each}
					{/if}
				</div>
			</ScrollArea>
		{/if}
	</div>

	<!-- Input Area -->
	<div class="absolute bottom-0 left-0 right-0 p-4 bg-background border-t border-border/50">
		<ChatInput
			onSend={handleSend}
			disabled={!chatState.channelId || chatState.error !== null}
		/>
		{#if chatState.error}
			<p class="text-xs text-destructive mt-2 text-center">{chatState.error}</p>
		{/if}
	</div>
</div>
