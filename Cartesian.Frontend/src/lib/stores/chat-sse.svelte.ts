import type { ChatMessageDto } from "$lib/api/cartesian-client";
import { baseUrl } from "$lib/api/client";

export interface ChatEvent {
	message?: ChatMessageDto;
	messageId?: string;
	channelId?: string;
	pinId?: string;
	pinnedById?: string;
	pinnedAt?: string;
	reactionId?: string;
	userId?: string;
	emoji?: string;
	createdAt?: string;
}

type EventCallback = (event: ChatEvent) => void;

interface ChannelSubscription {
	channelId: string;
	callback: EventCallback;
}

class GlobalChatSseService {
	#eventSource: EventSource | null = null;
	#subscriptions: ChannelSubscription[] = [];
	#globalListeners: EventCallback[] = [];
	#reconnectAttempts = 0;
	#maxReconnectAttempts = 5;
	#reconnectDelay = 1000;

	isConnected = $state(false);
	error = $state<string | null>(null);

	connect() {
		if (typeof EventSource === "undefined") return;
		if (this.#eventSource) return;

		const sseUrl = `${baseUrl}/chat/api/subscribe`;
		this.#eventSource = new EventSource(sseUrl, { withCredentials: true });

		this.#eventSource.onopen = () => {
			this.isConnected = true;
			this.error = null;
			this.#reconnectAttempts = 0;
			console.log("[SSE] Connected globally");
		};

		this.#eventSource.onerror = () => {
			this.isConnected = false;
			this.error = "Connection lost";
			console.error("[SSE] Connection error");

			this.#eventSource?.close();
			this.#eventSource = null;

			if (this.#reconnectAttempts < this.#maxReconnectAttempts) {
				this.#reconnectAttempts++;
				const delay = this.#reconnectDelay * Math.pow(2, this.#reconnectAttempts - 1);
				console.log(`[SSE] Reconnecting in ${delay}ms (attempt ${this.#reconnectAttempts})`);
				setTimeout(() => this.connect(), delay);
			}
		};

		this.#eventSource.addEventListener("chat", (event) => {
			try {
				const data = JSON.parse((event as MessageEvent).data) as ChatEvent;
				this.#handleEvent(data);
			} catch (e) {
				console.error("[SSE] Failed to parse chat event", e);
			}
		});
	}

	disconnect() {
		if (this.#eventSource) {
			this.#eventSource.close();
			this.#eventSource = null;
		}
		this.isConnected = false;
		this.#reconnectAttempts = 0;
	}

	#handleEvent(event: ChatEvent) {
		const channelId = event.message?.channelId ?? event.channelId;

		for (const listener of this.#globalListeners) {
			listener(event);
		}

		if (channelId) {
			const normalizedChannelId = String(channelId).toLowerCase();
			for (const sub of this.#subscriptions) {
				if (sub.channelId.toLowerCase() === normalizedChannelId) {
					sub.callback(event);
				}
			}
		}
	}

	subscribeToChannel(channelId: string, callback: EventCallback): () => void {
		const subscription: ChannelSubscription = { channelId, callback };
		this.#subscriptions.push(subscription);

		return () => {
			const index = this.#subscriptions.indexOf(subscription);
			if (index > -1) {
				this.#subscriptions.splice(index, 1);
			}
		};
	}

	addGlobalListener(callback: EventCallback): () => void {
		this.#globalListeners.push(callback);

		return () => {
			const index = this.#globalListeners.indexOf(callback);
			if (index > -1) {
				this.#globalListeners.splice(index, 1);
			}
		};
	}
}

export const globalChatSse = new GlobalChatSseService();
