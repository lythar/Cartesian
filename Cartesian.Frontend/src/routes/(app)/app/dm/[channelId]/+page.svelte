<script lang="ts">
	import { page } from "$app/stores";
	import { createQuery, createInfiniteQuery, useQueryClient } from "@tanstack/svelte-query";
	import {
		getChatApiMessages,
		getAccountApiPublicAccountId,
		getGetAccountApiPublicAccountIdQueryKey,
	} from "$lib/api/cartesian-client";
	import type {
		ChatMessageDto,
		CartesianUserDto,
		ChatChannelDto,
	} from "$lib/api/cartesian-client";
	import { customInstance } from "$lib/api/client";
	import { createSendMessageMutation } from "$lib/api/queries/chat.query";
	import { createGetMeQuery } from "$lib/api/queries/user.query";
	import ChatInput from "$lib/components/chat/chat-input.svelte";
	import ChatMessage from "$lib/components/chat/chat-message.svelte";
	import * as Avatar from "$lib/components/ui/avatar";
	import { Button } from "$lib/components/ui/button";
	import { ScrollArea } from "$lib/components/ui/scroll-area";
	import { Skeleton } from "$lib/components/ui/skeleton";
	import { getAvatarUrl, getInitials } from "$lib/utils";
	import { openUserProfile } from "$lib/components/profile/profile-state.svelte";
	import { globalChatSse, type ChatEvent } from "$lib/stores/chat-sse.svelte";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { ArrowLeft01Icon, MoreHorizontalIcon } from "@hugeicons/core-free-icons";
	import { goto } from "$app/navigation";
	import { untrack, onDestroy } from "svelte";

	interface DmChannelInfoDto {
		id: string;
		type: number;
		isEnabled: boolean;
		participant1Id: string | null;
		participant2Id: string | null;
		otherUser: CartesianUserDto | null;
	}

	const channelId = $derived($page.params.channelId);
	const queryClient = useQueryClient();

	const meQuery = createGetMeQuery({}, queryClient);
	const currentUser = $derived(meQuery.data);

	const channelQuery = createQuery(() => ({
		queryKey: ["chat", "dm", "channel-info", channelId],
		queryFn: ({ signal }) =>
			customInstance<DmChannelInfoDto>({
				url: `/chat/api/dm/channel/${channelId}`,
				method: "GET",
				signal,
			}),
		enabled: !!channelId,
	}));

	let otherUser = $derived(channelQuery.data?.otherUser ?? null);

	const messagesQuery = createInfiniteQuery(() => ({
		queryKey: ["chat", "messages", channelId, "infinite"],
		queryFn: ({ signal, pageParam }) =>
			getChatApiMessages(
				{ channelId: channelId ?? "", limit: 50, before: pageParam },
				signal,
			),
		enabled: !!channelId,
		initialPageParam: undefined as string | undefined,
		getNextPageParam: (lastPage) => {
			if (!lastPage.hasMore || lastPage.messages.length === 0) return undefined;
			return lastPage.messages[lastPage.messages.length - 1]?.id;
		},
	}));

	const sendMessageMutation = createSendMessageMutation(queryClient);

	let realtimeMessages = $state<ChatMessageDto[]>([]);
	let sseUnsubscribe: (() => void) | null = null;

	const historyMessages = $derived.by(() => {
		const pages = messagesQuery.data?.pages ?? [];
		const allMessages: ChatMessageDto[] = [];
		for (const page of pages) {
			allMessages.push(...page.messages);
		}
		return allMessages;
	});

	const messages = $derived.by(() => {
		const combined = [...historyMessages, ...realtimeMessages];
		const uniqueMap = new Map();
		combined.forEach((msg) => uniqueMap.set(msg.id, msg));
		return Array.from(uniqueMap.values()).sort(
			(a, b) =>
				new Date(a.createdAt as string).getTime() -
				new Date(b.createdAt as string).getTime(),
		);
	});

	$effect(() => {
		const currentChannelId = channelId;
		if (!currentChannelId) return;

		if (sseUnsubscribe) {
			sseUnsubscribe();
			sseUnsubscribe = null;
		}
		realtimeMessages = [];

		sseUnsubscribe = globalChatSse.subscribeToChannel(currentChannelId, (event: ChatEvent) => {
			if (event.message) {
				const message = event.message;
				const msgChannelId = String(message.channelId).toLowerCase();
				const targetChannelId = String(currentChannelId).toLowerCase();

				if (msgChannelId === targetChannelId) {
					const exists = realtimeMessages.some((m) => m.id === message.id);
					if (!exists) {
						realtimeMessages = [...realtimeMessages, message];
					}
				}
			} else if (event.messageId && event.channelId) {
				const msgChannelId = String(event.channelId).toLowerCase();
				const targetChannelId = String(currentChannelId).toLowerCase();

				if (msgChannelId === targetChannelId) {
					realtimeMessages = realtimeMessages.filter((m) => m.id !== event.messageId);
					queryClient.invalidateQueries({ queryKey: ["chat", "messages"] });
				}
			}
		});
	});

	onDestroy(() => {
		if (sseUnsubscribe) {
			sseUnsubscribe();
			sseUnsubscribe = null;
		}
	});

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

		if (
			scrollViewport.scrollTop < 100 &&
			messagesQuery.hasNextPage &&
			!messagesQuery.isFetchingNextPage
		) {
			isLoadingMore = true;
			const prevScrollHeight = scrollViewport.scrollHeight;
			const prevScrollTop = scrollViewport.scrollTop;

			await messagesQuery.fetchNextPage();

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
		const msgCount = messages.length;
		const loading = messagesQuery.isLoading;
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
		if (!channelId) return;

		try {
			await sendMessageMutation.mutateAsync({
				channelId,
				data: { content, attachmentIds: [] },
			});
			scrollToBottom();
		} catch (e) {
			console.error("Failed to send message", e);
			throw e;
		}
	}

	function getAuthor(userId: string): CartesianUserDto | undefined {
		if (currentUser?.id === userId) return currentUser;
		if (otherUser?.id === userId) return otherUser;
		return undefined;
	}
</script>

<div class="flex h-full flex-1 flex-col overflow-hidden bg-background">
	<div
		class="flex h-16 items-center gap-3 border-b border-border/40 bg-background/95 px-4 backdrop-blur supports-backdrop-filter:bg-background/60"
	>
		<Button
			variant="ghost"
			size="icon"
			class="h-8 w-8 shrink-0 lg:hidden"
			onclick={() => goto("/app")}
		>
			<HugeiconsIcon icon={ArrowLeft01Icon} size={18} />
		</Button>

		{#if channelQuery.isLoading}
			<Skeleton class="h-10 w-10 rounded-full" />
			<Skeleton class="h-4 w-24" />
		{:else if otherUser}
			<button
				class="flex items-center gap-3"
				onclick={() => otherUser && openUserProfile(otherUser.id)}
			>
				<Avatar.Root class="h-10 w-10 border border-border/40">
					<Avatar.Image
						src={getAvatarUrl(otherUser.avatar)}
						alt={otherUser.name}
						class="object-cover"
					/>
					<Avatar.Fallback class="bg-muted text-sm font-medium text-muted-foreground">
						{getInitials(otherUser.name)}
					</Avatar.Fallback>
				</Avatar.Root>
				<div>
					<h2 class="text-left text-sm leading-none font-semibold">{otherUser.name}</h2>
					<p class="text-xs text-muted-foreground">Direct Message</p>
				</div>
			</button>
		{/if}
	</div>

	<div class="relative flex flex-1 flex-col overflow-hidden">
		<!-- Messages Area -->
		<div class="absolute inset-0 bottom-[73px] overflow-hidden">
			{#if messagesQuery.isLoading}
				<div class="absolute inset-0 space-y-4 p-4">
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
					<div class="flex min-h-full flex-col justify-end px-4 py-4">
						{#if messagesQuery.isFetchingNextPage}
							<div class="flex justify-center py-2">
								<div
									class="h-5 w-5 animate-spin rounded-full border-2 border-primary border-t-transparent"
								></div>
							</div>
						{/if}

						{#each messages as message, i (message.id)}
							{@const prevMessage = messages[i - 1]}
							{@const isStacked =
								prevMessage &&
								prevMessage.authorId === message.authorId &&
								new Date(message.createdAt as string).getTime() -
									new Date(prevMessage.createdAt as string).getTime() <
									5 * 60 * 1000}
							<ChatMessage
								{message}
								author={getAuthor(message.authorId)}
								isCurrentUser={message.authorId === currentUser?.id}
								{isStacked}
							/>
						{/each}

						{#if messages.length === 0}
							<div
								class="flex flex-1 flex-col items-center justify-center text-center"
							>
								{#if otherUser}
									<Avatar.Root class="h-16 w-16 border-2 border-border/40">
										<Avatar.Image
											src={getAvatarUrl(otherUser.avatar)}
											alt={otherUser.name}
											class="object-cover"
										/>
										<Avatar.Fallback
											class="bg-muted text-xl font-medium text-muted-foreground"
										>
											{getInitials(otherUser.name)}
										</Avatar.Fallback>
									</Avatar.Root>
									<h3 class="mt-4 text-lg font-semibold">{otherUser.name}</h3>
									<p class="mt-1 text-sm text-muted-foreground">
										This is the beginning of your conversation with {otherUser.name}
									</p>
								{/if}
							</div>
						{/if}
					</div>
				</ScrollArea>
			{/if}
		</div>

		<!-- Input Area -->
		<div class="absolute right-0 bottom-0 left-0 border-t border-border/40 bg-background p-4">
			<ChatInput onSend={handleSend} disabled={!channelId} />
		</div>
	</div>
</div>
