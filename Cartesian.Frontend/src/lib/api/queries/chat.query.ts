import { createInfiniteQuery, createQuery, type QueryClient } from "@tanstack/svelte-query";
import {
	createPostChatApiChannelChannelIdSend,
	getChatApiCommunityChannel,
	getChatApiMessages,
	getGetChatApiCommunityChannelQueryKey,
	getGetChatApiMessagesQueryKey,
} from "../cartesian-client";

export function createGetCommunityChannelQuery(
	communityId: () => string,
	queryClient?: QueryClient,
) {
	return createQuery(() => {
		const id = communityId();
		return {
			queryKey: getGetChatApiCommunityChannelQueryKey({ communityId: id }),
			queryFn: ({ signal }) => getChatApiCommunityChannel({ communityId: id }, signal),
			enabled: !!id,
			queryClient,
		};
	});
}

export function createGetMessagesQuery(
	channelId: () => string,
	limit: () => number,
	queryClient?: QueryClient,
) {
	return createQuery(() => {
		const id = channelId();
		const lmt = limit();
		return {
			queryKey: getGetChatApiMessagesQueryKey({ channelId: id, limit: lmt }),
			queryFn: ({ signal }) => getChatApiMessages({ channelId: id, limit: lmt }, signal),
			enabled: !!id,
			queryClient,
		};
	});
}

export function createGetMessagesInfiniteQuery(
	channelId: () => string,
	limit: () => number,
	queryClient?: QueryClient,
) {
	return createInfiniteQuery(() => {
		const id = channelId();
		const lmt = limit();
		return {
			queryKey: ["chat", "messages", id, "infinite"],
			queryFn: ({ signal, pageParam }) =>
				getChatApiMessages({ channelId: id, limit: lmt, before: pageParam }, signal),
			enabled: !!id,
			initialPageParam: undefined as string | undefined,
			getNextPageParam: (lastPage) => {
				if (!lastPage.hasMore || lastPage.messages.length === 0) return undefined;
				return lastPage.messages[lastPage.messages.length - 1]?.id;
			},
			queryClient,
		};
	});
}

export function createSendMessageMutation(queryClient?: QueryClient) {
	return createPostChatApiChannelChannelIdSend({}, queryClient);
}
