<script lang="ts">
	import { goto } from "$app/navigation";
	import type { CartesianUserDto, ChatMessageDto } from "$lib/api/cartesian-client";
	import { getEventApiEventId } from "$lib/api/cartesian-client";
	import { baseUrl } from "$lib/api/client";
	import * as Avatar from "$lib/components/ui/avatar";
	import { cn } from "$lib/utils";
	import { parseMessageContent } from "$lib/utils/markdown";
	import { format } from "date-fns";

	let {
		message,
		author,
		isCurrentUser,
		isStacked = false,
	} = $props<{
		message: ChatMessageDto;
		author: CartesianUserDto | undefined;
		isCurrentUser: boolean;
		isStacked?: boolean;
	}>();

	interface EventEmbed {
		id: string;
		name: string;
		description: string;
		authorName: string;
		loading: boolean;
		error: boolean;
	}

	let eventEmbeds = $state<EventEmbed[]>([]);
	let loadedEventIds = $state<string[]>([]);

	const parsedContent = $derived(parseMessageContent(message.content));

	$effect(() => {
		const { eventLinks } = parsedContent;
		const newLinks = eventLinks.filter((id) => !loadedEventIds.includes(id));
		if (newLinks.length > 0) {
			loadedEventIds = [...loadedEventIds, ...newLinks];
			loadEventEmbeds(newLinks);
		}
	});

	async function loadEventEmbeds(eventIds: string[]) {
		const initialEmbeds: EventEmbed[] = eventIds.map((id) => ({
			id,
			name: "",
			description: "",
			authorName: "",
			loading: true,
			error: false,
		}));
		eventEmbeds = [...eventEmbeds, ...initialEmbeds];

		for (const eventId of eventIds) {
			try {
				const event = await getEventApiEventId(eventId);
				eventEmbeds = eventEmbeds.map((embed) =>
					embed.id === eventId
						? {
								id: eventId,
								name: event.name,
								description: event.description ?? "",
								authorName: event.author.name,
								loading: false,
								error: false,
							}
						: embed,
				);
			} catch {
				eventEmbeds = eventEmbeds.map((embed) =>
					embed.id === eventId
						? {
								...embed,
								loading: false,
								error: true,
							}
						: embed,
				);
			}
		}
	}

	function handleEventClick(eventId: string) {
		goto(`/app?event=${eventId}`);
	}

	function getAvatarUrl(avatar: { id: string } | null | undefined): string | undefined {
		if (!avatar) return undefined;
		return `${baseUrl}/media/api/${avatar.id}`;
	}
</script>

<div
	class={cn(
		"group flex w-full gap-3 -mx-4 px-4 transition-colors relative",
		isStacked ? "hover:bg-muted/30" : "pt-3 hover:bg-muted/30",
	)}
>
	{#if !isStacked}
		<Avatar.Root class="h-8 w-8 shrink-0 mt-0.5 border border-border/20">
			<Avatar.Image src={getAvatarUrl(author?.avatar)} alt={author?.name} class="object-cover" />
			<Avatar.Fallback class="bg-primary/5 text-[10px] font-medium text-foreground/70">
				{author?.name?.substring(0, 2).toUpperCase() ?? "??"}
			</Avatar.Fallback>
		</Avatar.Root>
	{:else}
		<div class="w-8 shrink-0 flex items-start justify-end">
			<span class="hidden group-hover:block text-[10px] text-muted-foreground/50 pr-1">
				{format(new Date(message.createdAt as string), "h:mm a")}
			</span>
		</div>
	{/if}

	<div class="flex flex-col min-w-0 flex-1">
		{#if !isStacked}
			<div class="flex items-center gap-2">
				<span
					class={cn(
						"text-xs font-semibold leading-none hover:underline cursor-pointer",
						isCurrentUser ? "text-primary" : "text-foreground",
					)}
				>
					{author?.name ?? "Unknown User"}
				</span>
				<span class="text-[10px] text-muted-foreground/50 select-none">
					{format(new Date(message.createdAt as string), "h:mm a")}
				</span>
			</div>
		{/if}

		<div
			class={cn(
				"message-content text-sm leading-relaxed text-foreground/90 wrap-break-word",
				isStacked ? "" : "mt-0.5",
			)}
		>
			{@html parsedContent.html}
		</div>

		{#if eventEmbeds.length > 0}
			<div class="mt-2 space-y-2">
				{#each eventEmbeds as embed}
					{#if embed.loading}
						<div class="flex items-center gap-3 rounded-lg border border-border/40 bg-muted/30 p-3">
							<div class="h-12 w-12 animate-pulse rounded-md bg-muted"></div>
							<div class="flex-1 space-y-2">
								<div class="h-4 w-32 animate-pulse rounded bg-muted"></div>
								<div class="h-3 w-48 animate-pulse rounded bg-muted"></div>
							</div>
						</div>
					{:else if embed.error}
						<div class="rounded-lg border border-border/40 bg-muted/30 p-3 text-xs text-muted-foreground">
							Failed to load event
						</div>
					{:else}
						<button
							onclick={() => handleEventClick(embed.id)}
							class="flex w-full items-start gap-3 rounded-lg border border-border/40 bg-muted/20 p-3 text-left transition-colors hover:bg-muted/40"
						>
							<div class="flex h-14 w-14 items-center justify-center rounded-md bg-primary/10">
								<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-primary"><path d="M8 2v4"/><path d="M16 2v4"/><rect width="18" height="18" x="3" y="4" rx="2"/><path d="M3 10h18"/></svg>
							</div>
							<div class="flex-1 min-w-0">
								<h4 class="text-sm font-medium text-foreground truncate">{embed.name}</h4>
								<p class="text-xs text-muted-foreground line-clamp-2">{embed.description}</p>
								<p class="mt-1 text-[10px] text-muted-foreground/70">by {embed.authorName}</p>
							</div>
							<div class="flex items-center text-xs text-primary">
								<span>View</span>
								<svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="ml-1"><path d="m9 18 6-6-6-6"/></svg>
							</div>
						</button>
					{/if}
				{/each}
			</div>
		{/if}
	</div>
</div>

<style>
	:global(.message-content p) {
		margin: 0;
	}
	:global(.message-content p + p) {
		margin-top: 0.5rem;
	}
	:global(.message-content code) {
		background: hsl(var(--muted));
		padding: 0.125rem 0.375rem;
		border-radius: 0.25rem;
		font-size: 0.85em;
	}
	:global(.message-content pre) {
		background: hsl(var(--muted));
		padding: 0.75rem;
		border-radius: 0.375rem;
		overflow-x: auto;
		margin: 0.5rem 0;
	}
	:global(.message-content pre code) {
		background: transparent;
		padding: 0;
	}
	:global(.message-content blockquote) {
		border-left: 3px solid hsl(var(--border));
		padding-left: 0.75rem;
		margin: 0.5rem 0;
		color: hsl(var(--muted-foreground));
	}
	:global(.message-content a) {
		color: hsl(var(--primary));
		text-decoration: underline;
	}
	:global(.message-content a:hover) {
		opacity: 0.8;
	}
	:global(.message-content ul),
	:global(.message-content ol) {
		margin: 0.25rem 0;
		padding-left: 1.25rem;
	}
	:global(.message-content img.emoji) {
		height: 1.2em;
		width: 1.2em;
		margin: 0 0.05em;
		vertical-align: -0.1em;
		display: inline;
	}
</style>
