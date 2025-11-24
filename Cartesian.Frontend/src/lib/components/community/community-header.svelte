<script lang="ts">
	import { goto } from "$app/navigation";
	import type { CommunityDto, MyUserDto } from "$lib/api/cartesian-client";
	import {
		createDeleteCommunityMutation,
		createJoinCommunityMutation,
		createLeaveCommunityMutation,
	} from "$lib/api/queries/community.query";
	import * as Avatar from "$lib/components/ui/avatar";
	import { Badge } from "$lib/components/ui/badge";
	import { Button } from "$lib/components/ui/button";
	import {
		Delete01Icon,
		Globe02Icon,
		LockKeyIcon,
		Logout01Icon,
		UserGroupIcon,
	} from "@hugeicons/core-free-icons";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { useQueryClient } from "@tanstack/svelte-query";

	let { community, currentUser, isMember, isAdmin } = $props<{
		community: CommunityDto;
		currentUser: MyUserDto | undefined;
		isMember: boolean;
		isAdmin: boolean;
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
	class="flex flex-col gap-6 rounded-xl border border-border/40 bg-card/30 px-6 py-6 md:flex-row md:items-center md:justify-between"
>
	<div class="flex flex-col gap-5 md:flex-row md:items-center">
		<Avatar.Root class="h-16 w-16 rounded-xl border border-border/50 shadow-sm">
			<Avatar.Image src={community.avatar?.url} alt={community.name} class="object-cover" />
			<Avatar.Fallback class="rounded-xl bg-primary/5 text-xl font-semibold text-primary">
				{community.name.substring(0, 2).toUpperCase()}
			</Avatar.Fallback>
		</Avatar.Root>

		<div class="space-y-1">
			<div class="flex items-center gap-2.5">
				<h1 class="text-lg font-semibold tracking-tight">{community.name}</h1>
				{#if community.inviteOnly}
					<Badge
						variant="secondary"
						class="gap-1 rounded-md px-1.5 py-0 text-[10px] font-medium tracking-wide text-muted-foreground uppercase opacity-80"
					>
						<HugeiconsIcon icon={LockKeyIcon} size={10} strokeWidth={2} />
						Private
					</Badge>
				{:else}
					<Badge
						variant="outline"
						class="gap-1 rounded-md border-primary/20 bg-primary/5 px-1.5 py-0 text-[10px] font-medium tracking-wide text-primary uppercase"
					>
						<HugeiconsIcon icon={Globe02Icon} size={10} strokeWidth={2} />
						Public
					</Badge>
				{/if}
			</div>
			<p class="max-w-xl text-sm leading-relaxed text-muted-foreground">
				{community.description}
			</p>
		</div>
	</div>

	<div class="flex items-center gap-2">
		{#if isMember}
			<Button
				variant="outline"
				size="sm"
				class="h-8 gap-2 rounded-lg border-border/50 bg-background/50 px-3 text-xs hover:bg-muted/50"
				onclick={handleLeave}
				disabled={leaveMutation.isPending}
			>
				{#if leaveMutation.isPending}
					Leaving...
				{:else}
					<HugeiconsIcon icon={Logout01Icon} size={14} strokeWidth={1.5} />
					Leave
				{/if}
			</Button>
		{:else}
			<Button
				size="sm"
				class="h-8 gap-2 rounded-lg px-4 text-xs shadow-none"
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
			<Button
				variant="ghost"
				size="sm"
				class="h-8 gap-2 rounded-lg text-xs text-muted-foreground hover:bg-destructive/10 hover:text-destructive"
				onclick={handleDelete}
				disabled={deleteMutation.isPending}
			>
				{#if deleteMutation.isPending}
					Deleting...
				{:else}
					<HugeiconsIcon icon={Delete01Icon} size={14} strokeWidth={1.5} />
					Delete
				{/if}
			</Button>
		{/if}
	</div>
</div>
