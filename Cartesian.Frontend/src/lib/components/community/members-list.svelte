<script lang="ts">
	import type { MembershipDto, MyUserDto } from "$lib/api/cartesian-client";
	import {
		createRemoveMemberMutation,
		createUpdateMemberPermissionsMutation
	} from "$lib/api/queries/community.query";
	import * as Avatar from "$lib/components/ui/avatar";
	import { Badge } from "$lib/components/ui/badge";
	import { Button } from "$lib/components/ui/button";
	import * as DropdownMenu from "$lib/components/ui/dropdown-menu";
	import { ScrollArea } from "$lib/components/ui/scroll-area";
	import { Permissions } from "$lib/constants/permissions";
	import { Delete01Icon, MoreHorizontalIcon } from "@hugeicons/core-free-icons";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { useQueryClient } from "@tanstack/svelte-query";

	let {
		members,
		currentUser,
		userPermissions = 0,
		communityId
	} = $props<{
		members: MembershipDto[];
		currentUser: MyUserDto | undefined;
		userPermissions?: number;
		communityId: string;
	}>();

	const queryClient = useQueryClient();
	const removeMemberMutation = createRemoveMemberMutation(queryClient);
	const updateMemberPermissionsMutation = createUpdateMemberPermissionsMutation(queryClient);

	const canManagePeople = $derived((userPermissions & Permissions.ManagePeople) === Permissions.ManagePeople);

	async function handleRemoveMember(userId: string) {
		if (!confirm("Are you sure you want to remove this member?")) return;
		try {
			await removeMemberMutation.mutateAsync({ communityId, userId });
		} catch (error) {
			console.error("Failed to remove member", error);
		}
	}

	async function handleUpdatePermissions(userId: string, newPermissions: number) {
		try {
			await updateMemberPermissionsMutation.mutateAsync({
				communityId,
				userId,
				permissions: newPermissions
			});
		} catch (error) {
			console.error("Failed to update member permissions", error);
		}
	}
</script>

<div class="rounded-3xl border border-border/40 bg-card shadow-sm">
	<div class="border-b border-border/40 px-6 py-4">
		<h2 class="text-lg font-semibold tracking-tight">Members</h2>
		<p class="text-sm text-muted-foreground">{members.length} members</p>
	</div>

	<ScrollArea class="h-[400px]">
		<div class="p-2">
			{#each members as membership (membership.id)}
				<div class="flex items-center justify-between rounded-xl p-3 transition-colors hover:bg-muted/50">
					<div class="flex items-center gap-3">
						<Avatar.Root class="h-10 w-10 rounded-full border border-border">
							<Avatar.Image
								src={membership.user.avatar?.url}
								alt={membership.user.name}
								class="object-cover"
							/>
							<Avatar.Fallback class="bg-primary/10 font-medium text-primary">
								{membership.user.name.substring(0, 2).toUpperCase()}
							</Avatar.Fallback>
						</Avatar.Root>
						<div>
							<div class="flex items-center gap-2">
								<p class="font-medium leading-none">{membership.user.name}</p>
								{#if (membership.permissions & Permissions.Admin) === Permissions.Admin}
									<Badge variant="default" class="h-5 px-1.5 text-[10px]">Admin</Badge>
								{:else}
									<Badge variant="secondary" class="h-5 px-1.5 text-[10px]">Member</Badge>
								{/if}
							</div>
							<p class="text-xs text-muted-foreground">Joined {new Date(membership.createdAt as string).toLocaleDateString()}</p>
						</div>
					</div>

					{#if canManagePeople && membership.userId !== currentUser?.id}
						<DropdownMenu.Root>
							<DropdownMenu.Trigger>
								<Button variant="ghost" size="icon" class="h-8 w-8 text-muted-foreground">
									<HugeiconsIcon icon={MoreHorizontalIcon} size={16} strokeWidth={2} />
									<span class="sr-only">Open menu</span>
								</Button>
							</DropdownMenu.Trigger>
							<DropdownMenu.Content align="end">
								<DropdownMenu.Label>Actions</DropdownMenu.Label>
								{#if (membership.permissions & Permissions.Admin) === Permissions.Admin}
									<DropdownMenu.Item
										onclick={() => handleUpdatePermissions(membership.userId, Permissions.Member)}
									>
										Make Member
									</DropdownMenu.Item>
								{:else}
									<DropdownMenu.Item
										onclick={() => handleUpdatePermissions(membership.userId, Permissions.Admin)}
									>
										Make Admin
									</DropdownMenu.Item>
								{/if}
								<DropdownMenu.Separator />
								<DropdownMenu.Item
									class="text-destructive focus:text-destructive"
									onclick={() => handleRemoveMember(membership.userId)}
								>
									Remove Member
								</DropdownMenu.Item>
							</DropdownMenu.Content>
						</DropdownMenu.Root>
					{/if}
				</div>
			{/each}
		</div>
	</ScrollArea>
</div>
