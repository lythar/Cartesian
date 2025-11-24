<script lang="ts">
  import type {
    CartesianUserDto,
    ChatMessageDto,
    MembershipDto,
    MyUserDto
  } from "$lib/api/cartesian-client";
  import { baseUrl } from "$lib/api/client";
  import {
    createGetCommunityChannelQuery,
    createGetMessagesQuery,
    createSendMessageMutation
  } from "$lib/api/queries/chat.query";
  import ChatInput from "$lib/components/chat/chat-input.svelte";
  import ChatMessage from "$lib/components/chat/chat-message.svelte";
  import { ScrollArea } from "$lib/components/ui/scroll-area";
  import { Skeleton } from "$lib/components/ui/skeleton";
  import { useQueryClient } from "@tanstack/svelte-query";
  import { onDestroy, onMount, tick } from "svelte";

  let {
    communityId,
    members,
    currentUser
  } = $props<{
    communityId: string;
    members: MembershipDto[];
    currentUser: MyUserDto | undefined;
  }>();

  const queryClient = useQueryClient();

  // 1. Get Channel
  const channelQuery = createGetCommunityChannelQuery(() => communityId, queryClient);
  const channelId = $derived(channelQuery.data?.id);

  // 2. Get Messages
  const messagesQuery = createGetMessagesQuery(() => channelId ?? "", () => 50, queryClient);
  
  // Local state for real-time messages
  let realtimeMessages = $state<ChatMessageDto[]>([]);
  
  const historyMessages = $derived(messagesQuery.data?.messages ?? []);
  
  // Deduplicate and merge history + realtime
  const allMessages = $derived.by(() => {
    // Merge arrays
    const combined = [...historyMessages, ...realtimeMessages];
    
    // Create a Map to deduplicate by ID, favoring the latest entry
    const uniqueMap = new Map();
    combined.forEach(msg => uniqueMap.set(msg.id, msg));
    
    // Convert back to array and sort
    return Array.from(uniqueMap.values())
      .sort((a, b) => new Date(a.createdAt as string).getTime() - new Date(b.createdAt as string).getTime());
  });

  // 3. Send Message
  const sendMessageMutation = createSendMessageMutation(queryClient);

  async function handleSend(content: string) {
    if (!channelId || !currentUser) return;
    try {
      await sendMessageMutation.mutateAsync({
        channelId,
        data: { content, attachmentIds: [] }
      });
      // We rely on the SSE broadcast to add the message to the view
    } catch (error) {
      console.error("Failed to send message", error);
    }
  }

  // 4. Real-time Updates (SSE)
  let eventSource: EventSource | null = null;

  function setupSSE() {
    if (eventSource) {
      eventSource.close();
    }

    const sseUrl = `${baseUrl}/chat/api/subscribe`;
    
    eventSource = new EventSource(sseUrl, {
      withCredentials: true
    });

    // Standard Message Handler
    eventSource.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data);
        handleChatEvent(data);
      } catch (e) {
        console.error("Failed to parse chat event", e);
      }
    };

    eventSource.onopen = () => console.log("✅ SSE Connected");
    eventSource.onerror = (err) => console.error("❌ SSE Error:", err);
  }

  async function handleChatEvent(event: any) {
    // The $type comes from the [JsonPolymorphic] attribute in C#
    const eventType = event.$type; 
    console.log("🔥 SSE Received:", eventType, event);

    if (eventType === "newMessage") {
      // Handle potential casing differences (PascalCase from C# vs camelCase JSON)
      const message = event.message || event.Message;
      
      if (!message) {
        console.warn("Received newMessage event but payload was empty");
        return;
      }

      // Safe String Comparison for IDs
      const msgChannelId = String(message.channelId).toLowerCase();
      const currentChannelId = String(channelId).toLowerCase();

      if (msgChannelId === currentChannelId) {
        console.log("✅ Message matches current channel. Adding...");
        realtimeMessages = [...realtimeMessages, message];
        await tick(); // Wait for DOM update
        scrollToBottom();
      } else {
        console.log(`⚠️ Message ignored. Channel ID mismatch: ${msgChannelId} vs ${currentChannelId}`);
      }
    } 
    else if (eventType === "messageDeleted") {
      const targetChannelId = String(event.channelId || event.ChannelId).toLowerCase();
      const currentChannelId = String(channelId).toLowerCase();

      if (targetChannelId === currentChannelId) {
         const targetMsgId = event.messageId || event.MessageId;
         realtimeMessages = realtimeMessages.filter(m => m.id !== targetMsgId);
         // Optional: invalidate query to clean up history
         // queryClient.invalidateQueries({ queryKey: ... });
      }
    }
    else if (eventType === "connected") {
      console.log("Chat stream handshake complete");
    }
  }

  onMount(() => {
    setupSSE();
  });

  onDestroy(() => {
    if (eventSource) {
      eventSource.close();
      console.log("SSE Connection closed");
    }
  });

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
  
  // Initial scroll when history loads
  $effect(() => {
    if (allMessages.length > 0) {
      scrollToBottom();
    }
  });

</script>

<div class="flex h-[600px] flex-col rounded-3xl border border-border/40 bg-card shadow-sm">
  <div class="border-b border-border/40 px-6 py-4">
    <h2 class="text-lg font-semibold tracking-tight">Community Chat</h2>
    <p class="text-sm text-muted-foreground">
      {#if channelQuery.isLoading} Connecting... {:else} Live {/if}
    </p>
  </div>

  <div class="flex-1 overflow-hidden p-4">
    {#if channelQuery.isLoading || messagesQuery.isLoading}
      <div class="flex h-full flex-col justify-end space-y-4">
        <Skeleton class="h-10 w-3/4 rounded-2xl" />
        <Skeleton class="h-10 w-1/2 rounded-2xl self-end" />
      </div>
    {:else}
      <ScrollArea class="h-full pr-4" bind:viewportRef={scrollViewport}>
        <div class="flex flex-col gap-4 py-4">
          {#if allMessages.length === 0}
            <div class="flex h-full flex-col items-center justify-center py-10 text-muted-foreground">
              <p>No messages yet.</p>
            </div>
          {:else}
            {#each allMessages as message (message.id)}
              <ChatMessage 
                {message} 
                author={getAuthor(message.authorId)} 
                isCurrentUser={message.authorId === currentUser?.id} 
              />
            {/each}
          {/if}
        </div>
      </ScrollArea>
    {/if}
  </div>

  <div class="border-t border-border/40 p-4">
    <ChatInput 
      onSend={handleSend} 
      disabled={!channelId || sendMessageMutation.isPending} 
    />
  </div>
</div>