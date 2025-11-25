<script lang="ts">
	import { goto } from "$app/navigation";
	import type { CommunityDto, MyUserDto } from "$lib/api/cartesian-client";
	import {
		createDeleteCommunityMutation,
		createJoinCommunityMutation,
		createLeaveCommunityMutation,
	} from "$lib/api/queries/community.query";
	import * as Avatar from "$lib/components/ui/avatar";
	import { Button } from "$lib/components/ui/button";
	import * as Sidebar from "$lib/components/ui/sidebar";
	import * as Tooltip from "$lib/components/ui/tooltip";
	import {
		Delete01Icon,
		LockKeyIcon,
		Logout01Icon,
		UserGroupIcon,
	} from "@hugeicons/core-free-icons";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { useQueryClient } from "@tanstack/svelte-query";

	let {
		community,
		currentUser,
		isMember,
		isAdmin,
		isOwner,
		onToggleMembers,
	} = $props<{
		community: CommunityDto;
		currentUser: MyUserDto | undefined;
		isMember: boolean;
		isAdmin: boolean;
		isOwner: boolean;
		onToggleMembers?: () => void;
	}>();

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
	class="flex h-16 items-center justify-between border-b border-border/40 bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60 px-4"
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
				<h1 class="text-sm font-semibold leading-none tracking-tight">{community.name}</h1>
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
				<p class="text-xs text-muted-foreground truncate max-w-[300px] md:max-w-[500px]">
					{community.description}
				</p>
			{/if}
		</div>
	</div>

	<div class="flex items-center gap-2">
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
