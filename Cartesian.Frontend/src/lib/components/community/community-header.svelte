<script lang="ts">
	import { goto } from "$app/navigation";
	import type { CommunityDto, MyUserDto, PinnedChatMessageDto } from "$lib/api/cartesian-client";
	import {
		createDeleteCommunityMutation,
		createJoinCommunityMutation,
		createLeaveCommunityMutation,
	} from "$lib/api/queries/community.query";
	import * as Avatar from "$lib/components/ui/avatar";
	import { Button } from "$lib/components/ui/button";
	import * as Sidebar from "$lib/components/ui/sidebar";
	import * as Tooltip from "$lib/components/ui/tooltip";
	import * as Sheet from "$lib/components/ui/sheet";
	import { ScrollArea } from "$lib/components/ui/scroll-area";
	import {
		Delete01Icon,
		LockKeyIcon,
		Logout01Icon,
		UserGroupIcon,
		UserBlock02Icon,
		Pin02Icon,
	} from "@hugeicons/core-free-icons";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { useQueryClient } from "@tanstack/svelte-query";
	import { format } from "date-fns";

	let {
		community,
		currentUser,
		isMember,
		isAdmin,
		isOwner,
		pinnedMessages = [],
		onToggleMembers,
		onToggleBans,
		onTogglePins,
	} = $props<{
		community: CommunityDto;
		currentUser: MyUserDto | undefined;
		isMember: boolean;
		isAdmin: boolean;
		isOwner: boolean;
		pinnedMessages?: PinnedChatMessageDto[];
		onToggleMembers?: () => void;
		onToggleBans?: () => void;
		onTogglePins?: () => void;
	}>();

	let pinsSheetOpen = $state(false);

	const queryClient = useQueryClient();
	const joinMutation = createJoinCommunityMutation(queryClient);
	const leaveMutation = createLeaveCommunityMutation(queryClient);
	const deleteMutation = createDeleteCommunityMutation(queryClient);

	async function handleJoin() {
		if (!currentUser) return;
		try {
			await joinMutation.mutateAsync({ communityId: community.id });
		} catch (error) {
			console.error("Failed to join community", error);
		}
	}

	async function handleLeave() {
		if (!currentUser) return;
		try {
			await leaveMutation.mutateAsync({ communityId: community.id, userId: currentUser.id });
		} catch (error) {
			console.error("Failed to leave community", error);
		}
	}

	async function handleDelete() {
		if (
			!confirm(
				"Are you sure you want to delete this community? This action cannot be undone.",
			)
		)
			return;
		try {
			await deleteMutation.mutateAsync({ communityId: community.id });
			goto("/map");
		} catch (error) {
			console.error("Failed to delete community", error);
		}
	}
</script>

<div
	class="flex h-16 items-center justify-between border-b border-border/40 bg-background/95 px-4 backdrop-blur supports-[backdrop-filter]:bg-background/60"
>
	<div class="flex items-center gap-3">
		<div class="md:hidden">
			<Sidebar.Trigger />
		</div>
		<Avatar.Root class="h-10 w-10 border border-border/40">
			<Avatar.Image src={community.avatar?.url} alt={community.name} class="object-cover" />
			<Avatar.Fallback class="bg-primary/5 text-sm font-semibold text-primary">
				{community.name.substring(0, 2).toUpperCase()}
			</Avatar.Fallback>
		</Avatar.Root>

		<div class="flex flex-col">
			<div class="flex items-center gap-2">
				<h1 class="text-sm leading-none font-semibold tracking-tight">{community.name}</h1>
				{#if community.inviteOnly}
					<Tooltip.Root>
						<Tooltip.Trigger>
							<span class="text-muted-foreground">
								<HugeiconsIcon icon={LockKeyIcon} size={12} />
							</span>
						</Tooltip.Trigger>
						<Tooltip.Content>Private Community</Tooltip.Content>
					</Tooltip.Root>
				{/if}
			</div>
			{#if community.description}
				<p class="max-w-[300px] truncate text-xs text-muted-foreground md:max-w-[500px]">
					{community.description}
				</p>
			{/if}
		</div>
	</div>

	<div class="flex items-center gap-2">
		{#if pinnedMessages.length > 0}
			<Tooltip.Root>
				<Tooltip.Trigger>
					<Button
						variant="ghost"
						size="icon"
						class="h-8 w-8 text-amber-500 hover:bg-amber-500/10 hover:text-amber-600"
						onclick={() => (pinsSheetOpen = true)}
					>
						<HugeiconsIcon icon={Pin02Icon} size={16} strokeWidth={1.5} />
					</Button>
				</Tooltip.Trigger>
				<Tooltip.Content>Pinned Messages ({pinnedMessages.length})</Tooltip.Content>
			</Tooltip.Root>
		{/if}

		<div class="lg:hidden">
			<Button
				variant="ghost"
				size="icon"
				class="h-8 w-8 text-muted-foreground hover:text-foreground"
				onclick={onToggleMembers}
			>
				<HugeiconsIcon icon={UserGroupIcon} size={16} strokeWidth={1.5} />
			</Button>
		</div>
		{#if isMember && !isOwner}
			<Tooltip.Root>
				<Tooltip.Trigger>
					<Button
						variant="ghost"
						size="icon"
						class="h-8 w-8 text-muted-foreground hover:text-foreground"
						onclick={handleLeave}
						disabled={leaveMutation.isPending}
					>
						<HugeiconsIcon icon={Logout01Icon} size={16} strokeWidth={1.5} />
					</Button>
				</Tooltip.Trigger>
				<Tooltip.Content>Leave Community</Tooltip.Content>
			</Tooltip.Root>
		{:else if !isMember}
			<Button
				size="sm"
				class="h-8 gap-2 rounded-md px-3 text-xs"
				onclick={handleJoin}
				disabled={joinMutation.isPending}
			>
				{#if joinMutation.isPending}
					Joining...
				{:else}
					<HugeiconsIcon icon={UserGroupIcon} size={14} strokeWidth={1.5} />
					Join
				{/if}
			</Button>
		{/if}

		{#if isAdmin}
			<Tooltip.Root>
				<Tooltip.Trigger>
					<Button
						variant="ghost"
						size="icon"
						class="h-8 w-8 text-muted-foreground hover:text-foreground"
						onclick={onToggleBans}
					>
						<HugeiconsIcon icon={UserBlock02Icon} size={16} strokeWidth={1.5} />
					</Button>
				</Tooltip.Trigger>
				<Tooltip.Content>Banned Users</Tooltip.Content>
			</Tooltip.Root>

			<Tooltip.Root>
				<Tooltip.Trigger>
					<Button
						variant="ghost"
						size="icon"
						class="h-8 w-8 text-muted-foreground hover:bg-destructive/10 hover:text-destructive"
						onclick={handleDelete}
						disabled={deleteMutation.isPending}
					>
						<HugeiconsIcon icon={Delete01Icon} size={16} strokeWidth={1.5} />
					</Button>
				</Tooltip.Trigger>
				<Tooltip.Content>Delete Community</Tooltip.Content>
			</Tooltip.Root>
		{/if}
	</div>
</div>

<Sheet.Root bind:open={pinsSheetOpen}>
	<Sheet.Content side="right" class="w-80 p-0 sm:w-96">
		<Sheet.Header class="border-b border-border/40 p-4">
			<Sheet.Title class="flex items-center gap-2">
				<HugeiconsIcon icon={Pin02Icon} size={18} className="text-amber-500" />
				Pinned Messages
			</Sheet.Title>
			<Sheet.Description class="text-xs">
				{pinnedMessages.length} pinned message{pinnedMessages.length !== 1 ? "s" : ""}
			</Sheet.Description>
		</Sheet.Header>
		<ScrollArea class="h-[calc(100vh-8rem)]">
			<div class="flex flex-col gap-2 p-4">
				{#each pinnedMessages as pin (pin.pinId)}
					<div class="rounded-lg border border-border/40 bg-muted/20 p-3">
						<p class="text-sm break-words whitespace-pre-wrap text-foreground/90">
							{pin.message.content}
						</p>
						<div
							class="mt-2 flex items-center justify-between text-[10px] text-muted-foreground"
						>
							<span>Pinned by {pin.pinnedByUsername}</span>
							<span>{format(new Date(pin.pinnedAt as string), "MMM d, h:mm a")}</span>
						</div>
					</div>
				{:else}
					<div
						class="flex flex-col items-center justify-center py-8 text-center text-muted-foreground"
					>
						<HugeiconsIcon icon={Pin02Icon} size={32} className="mb-2 opacity-50" />
						<p class="text-sm">No pinned messages</p>
					</div>
				{/each}
			</div>
		</ScrollArea>
	</Sheet.Content>
</Sheet.Root>
