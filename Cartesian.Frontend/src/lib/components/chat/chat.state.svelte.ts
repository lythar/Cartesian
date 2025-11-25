import type { ChatMessageDto, MyUserDto } from "$lib/api/cartesian-client";
import { baseUrl } from "$lib/api/client";
import {
	createGetCommunityChannelQuery,
	createGetMessagesQuery,
	createSendMessageMutation,
} from "$lib/api/queries/chat.query";
import type { QueryClient } from "@tanstack/svelte-query";
import { onDestroy } from "svelte";

export class ChatState {
	// State
	#communityId: string;
	#queryClient: QueryClient;

	realtimeMessages = $state<ChatMessageDto[]>([]);
	isConnected = $state(false);
	error = $state<string | null>(null);

	// Queries
	#channelQuery;
	#messagesQuery;
	#sendMessageMutation;

	// Derived
	channelId = $derived(this.#channelQuery.data?.id);
	isLoading = $derived(this.#channelQuery.isLoading || this.#messagesQuery.isLoading);
	historyMessages = $derived(this.#messagesQuery.data?.messages ?? []);

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

	// SSE
	#eventSource: EventSource | null = null;

	constructor(communityId: string, queryClient: QueryClient) {
		this.#communityId = communityId;
		this.#queryClient = queryClient;

		this.#channelQuery = createGetCommunityChannelQuery(() => this.#communityId, queryClient);

		// We use a derived function for the channel ID to ensure reactivity
		this.#messagesQuery = createGetMessagesQuery(
			() => this.channelId ?? "",
			() => 50,
			queryClient,
		);

		this.#sendMessageMutation = createSendMessageMutation(queryClient);

		// Initialize SSE
		this.connect();
	}

	connect() {
		if (typeof EventSource === "undefined") return;
		if (this.#eventSource) this.#eventSource.close();

		const sseUrl = `${baseUrl}/chat/api/subscribe`;
		this.#eventSource = new EventSource(sseUrl, { withCredentials: true });

		this.#eventSource.onopen = () => {
			this.isConnected = true;
			this.error = null;
			console.log("✅ Chat Connected");
		};

		this.#eventSource.onerror = (err) => {
			this.isConnected = false;
			this.error = "Connection lost";
			console.error("❌ Chat Connection Error:", err);
		};

		this.#eventSource.onmessage = (event) => {
			try {
				const data = JSON.parse(event.data);
				this.handleEvent(data);
			} catch (e) {
				console.error("Failed to parse chat event", e);
			}
		};
	}

	handleEvent(event: any) {
		const eventType = event.$type;

		if (eventType === "newMessage") {
			const message = event.message || event.Message;
			if (!message) return;

			// Check channel match
			const msgChannelId = String(message.channelId).toLowerCase();
			const currentChannelId = String(this.channelId).toLowerCase();

			if (msgChannelId === currentChannelId) {
				this.realtimeMessages = [...this.realtimeMessages, message];
			}
		} else if (eventType === "messageDeleted") {
			const targetChannelId = String(event.channelId || event.ChannelId).toLowerCase();
			const currentChannelId = String(this.channelId).toLowerCase();

			if (targetChannelId === currentChannelId) {
				const targetMsgId = event.messageId || event.MessageId;
				this.realtimeMessages = this.realtimeMessages.filter((m) => m.id !== targetMsgId);
				// Optionally invalidate history
				this.#queryClient.invalidateQueries({ queryKey: this.#messagesQuery.queryKey });
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
		if (this.#eventSource) {
			this.#eventSource.close();
			this.#eventSource = null;
		}
	}
}
