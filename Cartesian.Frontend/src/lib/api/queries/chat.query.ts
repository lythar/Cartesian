import { createQuery, type QueryClient } from "@tanstack/svelte-query";
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

export function createSendMessageMutation(queryClient?: QueryClient) {
	return createPostChatApiChannelChannelIdSend({}, queryClient);
}
