<script lang="ts">
	import type { MyUserDto, MembershipDto, CartesianUserDto } from "$lib/api/cartesian-client";
	import ChatInput from "$lib/components/chat/chat-input.svelte";
	import ChatMessage from "$lib/components/chat/chat-message.svelte";
	import { ScrollArea } from "$lib/components/ui/scroll-area";
	import { Skeleton } from "$lib/components/ui/skeleton";
	import type { ChatState } from "./chat.state.svelte";

	let {
		chatState,
		members,
		currentUser,
	} = $props<{
		chatState: ChatState;
		members: MembershipDto[];
		currentUser: MyUserDto | undefined;
	}>();

	// Helpers
	function getAuthor(userId: string): CartesianUserDto | undefined {
		const member = members.find((m: MembershipDto) => m.userId === userId);
		if (member) return member.user;
		if (currentUser?.id === userId) return currentUser;
		return undefined;
	}

	let scrollViewport = $state<HTMLElement | null>(null);

	function scrollToBottom() {
		if (scrollViewport) {
			setTimeout(() => {
				if (scrollViewport) {
					scrollViewport.scrollTop = scrollViewport.scrollHeight;
				}
			}, 50);
		}
	}

	// Initial scroll and auto-scroll on new messages
	$effect(() => {
		if (chatState.messages.length > 0) {
			scrollToBottom();
		}
	});

	async function handleSend(content: string) {
		await chatState.sendMessage(content);
		scrollToBottom();
	}
</script>

<div class="flex flex-1 flex-col overflow-hidden">
	<!-- Messages Area -->
	<div class="flex-1 overflow-hidden relative">
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
				<div class="flex flex-col justify-end min-h-full py-4 px-4 gap-2">
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
							/>
						{/each}
					{/if}
				</div>
			</ScrollArea>
		{/if}
	</div>

	<!-- Input Area -->
	<div class="p-4 bg-background border-t border-border/50">
		<ChatInput
			onSend={handleSend}
			disabled={!chatState.channelId || chatState.error !== null}
		/>
		{#if chatState.error}
			<p class="text-xs text-destructive mt-2 text-center">{chatState.error}</p>
		{/if}
	</div>
</div>
