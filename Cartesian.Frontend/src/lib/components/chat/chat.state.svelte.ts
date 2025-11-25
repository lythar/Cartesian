import type { ChatMessageDto } from "$lib/api/cartesian-client";
import {
	createGetCommunityChannelQuery,
	createGetMessagesInfiniteQuery,
	createSendMessageMutation,
} from "$lib/api/queries/chat.query";
import { getGetCommunityApiCommunityIdMembersQueryKey } from "$lib/api/cartesian-client";
import { globalChatSse, type ChatEvent } from "$lib/stores/chat-sse.svelte";
import { unreadMessagesStore } from "$lib/stores/unread-messages.svelte";
import type { QueryClient } from "@tanstack/svelte-query";

export class ChatState {
	#communityId: string;
	#queryClient: QueryClient;
	#knownAuthorIds = new Set<string>();

	realtimeMessages = $state<ChatMessageDto[]>([]);
	isConnected = $derived(globalChatSse.isConnected);
	error = $derived(globalChatSse.error);

	#channelQuery;
	#messagesQuery;
	#sendMessageMutation;
	#unsubscribe: (() => void) | null = null;

	// @ts-ignore
	channelId = $derived(this.#channelQuery.data?.id);
	// @ts-ignore
	isLoading = $derived(this.#channelQuery.isLoading || this.#messagesQuery.isLoading);
	// @ts-ignore
	isFetchingMore = $derived(this.#messagesQuery.isFetchingNextPage);
	// @ts-ignore
	hasMore = $derived(this.#messagesQuery.hasNextPage);

	historyMessages = $derived.by(() => {
		const pages = this.#messagesQuery.data?.pages ?? [];
		const allMessages: ChatMessageDto[] = [];
		for (const page of pages) {
			allMessages.push(...page.messages);
		}
		for (const msg of allMessages) {
			this.#knownAuthorIds.add(msg.authorId);
		}
		return allMessages;
	});

	messages = $derived.by(() => {
		const combined = [...this.historyMessages, ...this.realtimeMessages];
		const uniqueMap = new Map();
		combined.forEach((msg) => uniqueMap.set(msg.id, msg));
		return Array.from(uniqueMap.values()).sort(
			(a, b) =>
				new Date(a.createdAt as string).getTime() -
				new Date(b.createdAt as string).getTime(),
		);
	});

	constructor(communityId: string, queryClient: QueryClient) {
		this.#communityId = communityId;
		this.#queryClient = queryClient;

		this.#channelQuery = createGetCommunityChannelQuery(() => this.#communityId, queryClient);

		this.#messagesQuery = createGetMessagesInfiniteQuery(
			() => this.channelId ?? "",
			() => 50,
			queryClient,
		);

		this.#sendMessageMutation = createSendMessageMutation(queryClient);

		this.#subscribeToChannel();
	}

	async loadMore() {
		if (this.hasMore && !this.isFetchingMore) {
			await this.#messagesQuery.fetchNextPage();
		}
	}

	#subscribeToChannel() {
		$effect(() => {
			const channelId = this.channelId;
			if (!channelId) return;

			unreadMessagesStore.registerChannel(channelId, this.#communityId);

			if (this.#unsubscribe) {
				this.#unsubscribe();
			}

			this.#unsubscribe = globalChatSse.subscribeToChannel(channelId, (event) => {
				this.#handleEvent(event);
			});
		});
	}

	#handleEvent(event: ChatEvent) {
		if (event.message) {
			const message = event.message;
			const msgChannelId = String(message.channelId).toLowerCase();
			const currentChannelId = String(this.channelId ?? "").toLowerCase();

			if (msgChannelId === currentChannelId) {
				const exists = this.realtimeMessages.some((m) => m.id === message.id);
				if (!exists) {
					this.realtimeMessages = [...this.realtimeMessages, message];

					if (!this.#knownAuthorIds.has(message.authorId)) {
						this.#queryClient.invalidateQueries({
							queryKey: getGetCommunityApiCommunityIdMembersQueryKey(
								this.#communityId,
							),
						});
						this.#knownAuthorIds.add(message.authorId);
					}
				}

				unreadMessagesStore.markAsRead(currentChannelId, message.id);
			}
		} else if (event.messageId && event.channelId) {
			const targetChannelId = String(event.channelId).toLowerCase();
			const currentChannelId = String(this.channelId).toLowerCase();

			if (targetChannelId === currentChannelId) {
				this.realtimeMessages = this.realtimeMessages.filter(
					(m) => m.id !== event.messageId,
				);
				this.#queryClient.invalidateQueries({ queryKey: ["chat", "messages"] });
			}
		}
	}

	async sendMessage(content: string) {
		if (!this.channelId) return;

		try {
			await this.#sendMessageMutation.mutateAsync({
				channelId: this.channelId,
				data: { content, attachmentIds: [] },
			});
		} catch (e) {
			console.error("Failed to send message", e);
			throw e;
		}
	}

	dispose() {
		if (this.#unsubscribe) {
			this.#unsubscribe();
			this.#unsubscribe = null;
		}
	}
}
