<script lang="ts">
	import NewCommunityOverlay from "$lib/components/community/new-community.svelte";
	import Navigation from "$lib/components/layout/navigation.svelte";
	import { createLayoutContext } from "$lib/context/layout.svelte";
	import { globalChatSse } from "$lib/stores/chat-sse.svelte";
	import { unreadMessagesStore } from "$lib/stores/unread-messages.svelte";
	import { authStore } from "$lib/stores/auth.svelte";
	import { onDestroy } from "svelte";
	import "mapbox-gl/dist/mapbox-gl.css";

	const { children } = $props();

	createLayoutContext();

	let unsubscribeGlobal: (() => void) | null = null;

	$effect(() => {
		if ($authStore.isAuthenticated) {
			globalChatSse.connect();

			unsubscribeGlobal = globalChatSse.addGlobalListener((event) => {
				unreadMessagesStore.handleNewMessage(event);
			});
		} else {
			globalChatSse.disconnect();
			if (unsubscribeGlobal) {
				unsubscribeGlobal();
				unsubscribeGlobal = null;
			}
		}
	});

	onDestroy(() => {
		globalChatSse.disconnect();
		if (unsubscribeGlobal) {
			unsubscribeGlobal();
		}
	});
</script>

<Navigation>
	{@render children?.()}
</Navigation>

<NewCommunityOverlay />
