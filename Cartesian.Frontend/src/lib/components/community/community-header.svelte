<script lang="ts">
	import { goto } from "$app/navigation";
	import type { CommunityDto, MyUserDto } from "$lib/api/cartesian-client";
	import { createDeleteCommunityMutation, createJoinCommunityMutation, createLeaveCommunityMutation } from "$lib/api/queries/community.query";
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
		if (!confirm("Are you sure you want to delete this community? This action cannot be undone.")) return;
		try {
			await deleteMutation.mutateAsync({ communityId: community.id });
			goto("/map");
		} catch (error) {
			console.error("Failed to delete community", error);
		}
	}
</script>

<div class="relative overflow-hidden rounded-3xl border border-border/40 bg-card shadow-sm">
	<div class="relative px-6 pt-8 pb-4">
		<div class="flex flex-col gap-6 md:flex-row md:items-start md:justify-between">
			<div class="flex flex-col gap-4 md:flex-row md:items-center">
				<Avatar.Root class="h-24 w-24 rounded-2xl border-4 border-background shadow-xl">
					<Avatar.Image
						src={community.avatar?.url}
						alt={community.name}
						class="object-cover"
					/>
					<Avatar.Fallback class="rounded-2xl bg-primary/10 text-2xl font-bold text-primary">
						{community.name.substring(0, 2).toUpperCase()}
					</Avatar.Fallback>
				</Avatar.Root>

				<div class="space-y-1 pt-2">
					<div class="flex items-center gap-2">
						<h1 class="text-2xl font-bold tracking-tight">{community.name}</h1>
						{#if community.inviteOnly}
							<Badge variant="secondary" class="gap-1 rounded-full px-2 py-0.5 text-[10px]">
								<HugeiconsIcon icon={LockKeyIcon} size={10} strokeWidth={2.5} />
                  Private
							</Badge>
						{:else}
							<Badge variant="outline" class="gap-1 rounded-full px-2 py-0.5 text-[10px]">
								<HugeiconsIcon icon={Globe02Icon} size={10} strokeWidth={2.5} />
								Public
							</Badge>
						{/if}
					</div>
					<p class="max-w-xl text-sm text-muted-foreground">{community.description}</p>
				</div>
			</div>

			<div class="flex items-center gap-2">
				{#if isMember}
					<Button variant="outline" size="sm" onclick={handleLeave} disabled={leaveMutation.isPending}>
						{#if leaveMutation.isPending}
							Leaving...
						{:else}
							<HugeiconsIcon icon={Logout01Icon} size={16} strokeWidth={2} />
							Leave
						{/if}
					</Button>
				{:else}
					<Button size="sm" onclick={handleJoin} disabled={joinMutation.isPending}>
						{#if joinMutation.isPending}
							Joining...
						{:else}
							<HugeiconsIcon icon={UserGroupIcon} size={16} strokeWidth={2} />
							Join Community
						{/if}
					</Button>
				{/if}

				{#if isAdmin}
					<Button variant="destructive" size="sm" onclick={handleDelete} disabled={deleteMutation.isPending}>
						{#if deleteMutation.isPending}
							Deleting...
						{:else}
							<HugeiconsIcon icon={Delete01Icon} size={16} strokeWidth={2} />
							Delete
						{/if}
					</Button>
				{/if}
			</div>
		</div>
	</div>
</div>
