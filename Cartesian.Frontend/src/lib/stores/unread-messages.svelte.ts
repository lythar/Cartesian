import { browser } from "$app/environment";
import type { ChatEvent } from "./chat-sse.svelte";

const STORAGE_KEY = "cartesian_last_read_messages";

interface LastReadMessages {
	[channelId: string]: string;
}

interface UnreadCounts {
	[channelId: string]: number;
}

class UnreadMessagesStore {
	#lastReadMessages: LastReadMessages = {};
	#channelToCommunityMap: Map<string, string> = new Map();

	unreadCounts = $state<UnreadCounts>({});

	constructor() {
		if (browser) {
			this.#loadFromStorage();
		}
	}

	#loadFromStorage() {
		try {
			const stored = localStorage.getItem(STORAGE_KEY);
			if (stored) {
				this.#lastReadMessages = JSON.parse(stored);
			}
		} catch {
			this.#lastReadMessages = {};
		}
	}

	#saveToStorage() {
		if (browser) {
			try {
				localStorage.setItem(STORAGE_KEY, JSON.stringify(this.#lastReadMessages));
			} catch {
				console.error("[Unread] Failed to save to storage");
			}
		}
	}

	registerChannel(channelId: string, communityId: string) {
		this.#channelToCommunityMap.set(channelId.toLowerCase(), communityId);
	}

	getCommunityIdForChannel(channelId: string): string | undefined {
		return this.#channelToCommunityMap.get(channelId.toLowerCase());
	}

	getLastReadMessageId(channelId: string): string | undefined {
		return this.#lastReadMessages[channelId.toLowerCase()];
	}

	markAsRead(channelId: string, messageId: string) {
		const normalizedChannelId = channelId.toLowerCase();
		this.#lastReadMessages[normalizedChannelId] = messageId;
		this.#saveToStorage();

		const newUnreadCounts = { ...this.unreadCounts };
		newUnreadCounts[normalizedChannelId] = 0;
		this.unreadCounts = newUnreadCounts;
	}

	incrementUnread(channelId: string) {
		const normalizedChannelId = channelId.toLowerCase();
		const newUnreadCounts = { ...this.unreadCounts };
		newUnreadCounts[normalizedChannelId] = (newUnreadCounts[normalizedChannelId] ?? 0) + 1;
		this.unreadCounts = newUnreadCounts;
	}

	setUnreadCount(channelId: string, count: number) {
		const normalizedChannelId = channelId.toLowerCase();
		const newUnreadCounts = { ...this.unreadCounts };
		newUnreadCounts[normalizedChannelId] = count;
		this.unreadCounts = newUnreadCounts;
	}

	getUnreadCountForChannel(channelId: string): number {
		return this.unreadCounts[channelId.toLowerCase()] ?? 0;
	}

	getUnreadCountForCommunity(communityId: string): number {
		for (const [channelId, cId] of this.#channelToCommunityMap.entries()) {
			if (cId === communityId) {
				return this.unreadCounts[channelId] ?? 0;
			}
		}
		return 0;
	}

	handleNewMessage(event: ChatEvent, currentChannelId?: string) {
		if (!event.message) return;

		const messageChannelId = String(event.message.channelId).toLowerCase();
		const currentNormalized = currentChannelId?.toLowerCase();

		if (currentNormalized && messageChannelId === currentNormalized) {
			this.markAsRead(messageChannelId, event.message.id);
		} else {
			this.incrementUnread(messageChannelId);
		}
	}
}

export const unreadMessagesStore = new UnreadMessagesStore();
