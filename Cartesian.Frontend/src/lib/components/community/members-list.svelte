<script lang="ts">
	import type { MembershipDto, MyUserDto } from "$lib/api/cartesian-client";
	import {
		createRemoveMemberMutation,
		createUpdateMemberPermissionsMutation,
	} from "$lib/api/queries/community.query";
	import * as Avatar from "$lib/components/ui/avatar";
	import { Badge } from "$lib/components/ui/badge";
	import { Button } from "$lib/components/ui/button";
	import * as DropdownMenu from "$lib/components/ui/dropdown-menu";
	import { ScrollArea } from "$lib/components/ui/scroll-area";
	import { Permissions } from "$lib/constants/permissions";
	import { MoreHorizontalIcon } from "@hugeicons/core-free-icons";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { useQueryClient } from "@tanstack/svelte-query";

	let {
		members,
		currentUser,
		userPermissions = 0,
		communityId,
	} = $props<{
		members: MembershipDto[];
		currentUser: MyUserDto | undefined;
		userPermissions?: number;
		communityId: string;
	}>();

	const queryClient = useQueryClient();
	const removeMemberMutation = createRemoveMemberMutation(queryClient);
	const updateMemberPermissionsMutation = createUpdateMemberPermissionsMutation(queryClient);

	const canManagePeople = $derived(
		(userPermissions & Permissions.ManagePeople) === Permissions.ManagePeople,
	);

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
				permissions: newPermissions,
			});
		} catch (error) {
			console.error("Failed to update member permissions", error);
		}
	}
</script>

<div class="flex flex-col h-full">
	<div
		class="flex h-16 items-center justify-between border-b border-border/40 bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60 px-4"
	>
		<h2 class="text-xs font-semibold text-foreground/70 uppercase tracking-wider">
			Members
		</h2>
		<Badge variant="outline" class="text-[10px] py-0 h-4 border-border/50 text-muted-foreground"
			>{members.length}</Badge
		>
	</div>

	<ScrollArea class="flex-1">
		<div class="p-2 space-y-0.5">
			{#each members as membership (membership.id)}
				<div
					class="group flex items-center justify-between rounded-md px-2 py-1.5 transition-colors hover:bg-muted/50"
				>
					<div class="flex items-center gap-3 overflow-hidden">
						<Avatar.Root
							class="h-8 w-8 rounded-full border border-border/40"
						>
							<Avatar.Image
								src={membership.user.avatar?.url}
								alt={membership.user.name}
								class="object-cover"
							/>
							<Avatar.Fallback
								class="bg-muted text-[10px] font-medium text-foreground/70"
							>
								{membership.user.name.substring(0, 2).toUpperCase()}
							</Avatar.Fallback>
						</Avatar.Root>
						<div class="min-w-0">
							<div class="flex items-center gap-1.5">
								<p class="text-sm font-medium leading-none text-foreground truncate">
									{membership.user.name}
								</p>
								{#if (membership.permissions & Permissions.Admin) === Permissions.Admin}
									<span class="text-[10px] text-primary font-semibold">Admin</span>
								{/if}
							</div>
							<p class="text-[10px] text-muted-foreground truncate">
								Joined {new Date(membership.createdAt as string).toLocaleDateString()}
							</p>
						</div>
					</div>

					{#if canManagePeople && membership.userId !== currentUser?.id}
						<DropdownMenu.Root>
							<DropdownMenu.Trigger>
								<Button
									variant="ghost"
									size="icon"
									class="h-6 w-6 opacity-0 transition-opacity group-hover:opacity-100"
								>
									<HugeiconsIcon
										icon={MoreHorizontalIcon}
										size={14}
										strokeWidth={1.5}
									/>
									<span class="sr-only">Open menu</span>
								</Button>
							</DropdownMenu.Trigger>
							<DropdownMenu.Content align="end">
								<DropdownMenu.Label>Actions</DropdownMenu.Label>
								{#if (membership.permissions & Permissions.Admin) === Permissions.Admin}
									<DropdownMenu.Item
										onclick={() =>
											handleUpdatePermissions(
												membership.userId,
												Permissions.Member,
											)}
									>
										Make Member
									</DropdownMenu.Item>
								{:else}
									<DropdownMenu.Item
										onclick={() =>
											handleUpdatePermissions(
												membership.userId,
												Permissions.Admin,
											)}
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
