<script lang="ts">
	import type { MembershipDto, MyUserDto } from "$lib/api/cartesian-client";
	import {
		createRemoveMemberMutation,
		createUpdateMemberPermissionsMutation,
	} from "$lib/api/queries/community.query";
	import { customInstance } from "$lib/api/client";
	import * as Avatar from "$lib/components/ui/avatar";
	import * as AlertDialog from "$lib/components/ui/alert-dialog";
	import { Badge } from "$lib/components/ui/badge";
	import { Button } from "$lib/components/ui/button";
	import * as DropdownMenu from "$lib/components/ui/dropdown-menu";
	import { ScrollArea } from "$lib/components/ui/scroll-area";
	import { Permissions } from "$lib/constants/permissions";
	import {
		MoreHorizontalIcon,
		UserIcon,
		Cancel01Icon,
		UserBlock02Icon,
	} from "@hugeicons/core-free-icons";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { useQueryClient, createMutation } from "@tanstack/svelte-query";
	import { openUserProfile } from "$lib/components/profile/profile-state.svelte";

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

	let kickDialogOpen = $state(false);
	let banDialogOpen = $state(false);
	let selectedMember = $state<MembershipDto | null>(null);
	let banReason = $state("");

	const banMutation = createMutation(() => ({
		mutationFn: async ({ userId, reason }: { userId: string; reason?: string }) => {
			await customInstance({
				url: `/community/api/${communityId}/ban/${userId}`,
				method: "POST",
				data: { reason },
			});
		},
		onSuccess: () => {
			queryClient.invalidateQueries({ queryKey: ["community", communityId] });
			queryClient.invalidateQueries({ queryKey: ["community", communityId, "members"] });
		},
	}));

	const canManagePeople = $derived(
		(userPermissions & Permissions.ManagePeople) === Permissions.ManagePeople,
	);

	function openKickDialog(member: MembershipDto) {
		selectedMember = member;
		kickDialogOpen = true;
	}

	function openBanDialog(member: MembershipDto) {
		selectedMember = member;
		banReason = "";
		banDialogOpen = true;
	}

	async function handleKick() {
		if (!selectedMember) return;
		try {
			await removeMemberMutation.mutateAsync({ communityId, userId: selectedMember.userId });
			kickDialogOpen = false;
			selectedMember = null;
		} catch (error) {
			console.error("Failed to kick member", error);
		}
	}

	async function handleBan() {
		if (!selectedMember) return;
		try {
			await banMutation.mutateAsync({
				userId: selectedMember.userId,
				reason: banReason || undefined,
			});
			banDialogOpen = false;
			selectedMember = null;
			banReason = "";
		} catch (error) {
			console.error("Failed to ban member", error);
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

<div class="flex h-full flex-col">
	<div
		class="flex h-16 items-center justify-between border-b border-border/40 bg-background/95 px-4 backdrop-blur supports-backdrop-filter:bg-background/60"
	>
		<h2 class="text-xs font-semibold tracking-wider text-foreground/70 uppercase">Members</h2>
		<Badge variant="outline" class="h-4 border-border/50 py-0 text-[10px] text-muted-foreground"
			>{members.length}</Badge
		>
	</div>

	<ScrollArea class="flex-1">
		<div class="space-y-0.5 p-2">
			{#each members as membership (membership.id)}
				<div
					class="group flex items-center justify-between rounded-md px-2 py-1.5 transition-colors hover:bg-muted/50"
				>
					<div class="flex items-center gap-3 overflow-hidden">
						<Avatar.Root class="h-8 w-8 rounded-full border border-border/40">
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
								<p
									class="truncate text-sm leading-none font-medium text-foreground"
								>
									{membership.user.name}
								</p>
								{#if (membership.permissions & Permissions.Admin) === Permissions.Admin}
									<span class="text-[10px] font-semibold text-primary">Admin</span
									>
								{/if}
							</div>
							<p class="truncate text-[10px] text-muted-foreground">
								Joined {new Date(
									membership.createdAt as string,
								).toLocaleDateString()}
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
								<DropdownMenu.Item
									onclick={() => openUserProfile(membership.userId)}
								>
									<HugeiconsIcon icon={UserIcon} size={14} />
									<span class="ml-2">View Profile</span>
								</DropdownMenu.Item>
								<DropdownMenu.Separator />
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
									onclick={() => openKickDialog(membership)}
								>
									<HugeiconsIcon icon={Cancel01Icon} size={14} />
									<span class="ml-2">Kick</span>
								</DropdownMenu.Item>
								<DropdownMenu.Item
									class="text-destructive focus:text-destructive"
									onclick={() => openBanDialog(membership)}
								>
									<HugeiconsIcon icon={UserBlock02Icon} size={14} />
									<span class="ml-2">Ban</span>
								</DropdownMenu.Item>
							</DropdownMenu.Content>
						</DropdownMenu.Root>
					{/if}
				</div>
			{/each}
		</div>
	</ScrollArea>
</div>

<AlertDialog.Root bind:open={kickDialogOpen}>
	<AlertDialog.Content>
		<AlertDialog.Header>
			<AlertDialog.Title>Kick Member</AlertDialog.Title>
			<AlertDialog.Description>
				Are you sure you want to kick <span class="font-semibold"
					>{selectedMember?.user.name}</span
				> from this community? They will be removed but can rejoin.
			</AlertDialog.Description>
		</AlertDialog.Header>
		<AlertDialog.Footer>
			<AlertDialog.Cancel>Cancel</AlertDialog.Cancel>
			<AlertDialog.Action
				class="bg-destructive text-destructive-foreground hover:bg-destructive/90"
				onclick={handleKick}
				disabled={removeMemberMutation.isPending}
			>
				{removeMemberMutation.isPending ? "Kicking..." : "Kick"}
			</AlertDialog.Action>
		</AlertDialog.Footer>
	</AlertDialog.Content>
</AlertDialog.Root>

<AlertDialog.Root bind:open={banDialogOpen}>
	<AlertDialog.Content>
		<AlertDialog.Header>
			<AlertDialog.Title>Ban Member</AlertDialog.Title>
			<AlertDialog.Description>
				Are you sure you want to ban <span class="font-semibold"
					>{selectedMember?.user.name}</span
				> from this community? They will be removed and cannot rejoin until unbanned.
			</AlertDialog.Description>
		</AlertDialog.Header>
		<div class="py-4">
			<label for="ban-reason" class="text-sm font-medium"> Reason (optional) </label>
			<input
				id="ban-reason"
				type="text"
				bind:value={banReason}
				placeholder="Enter reason for ban..."
				class="mt-1 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:ring-2 focus-visible:ring-ring focus-visible:outline-none"
			/>
		</div>
		<AlertDialog.Footer>
			<AlertDialog.Cancel>Cancel</AlertDialog.Cancel>
			<AlertDialog.Action
				class="bg-destructive text-destructive-foreground hover:bg-destructive/90"
				onclick={handleBan}
				disabled={banMutation.isPending}
			>
				{banMutation.isPending ? "Banning..." : "Ban"}
			</AlertDialog.Action>
		</AlertDialog.Footer>
	</AlertDialog.Content>
</AlertDialog.Root>
