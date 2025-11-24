<script lang="ts">
	import type { MembershipDto, MyUserDto } from "$lib/api/cartesian-client";
	import { createRemoveMemberMutation } from "$lib/api/queries/community.query";
	import * as Avatar from "$lib/components/ui/avatar";
	import { Button } from "$lib/components/ui/button";
	import { ScrollArea } from "$lib/components/ui/scroll-area";
	import { Delete01Icon } from "@hugeicons/core-free-icons";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { useQueryClient } from "@tanstack/svelte-query";

	let { members, currentUser, isAdmin, communityId } = $props<{
		members: MembershipDto[];
		currentUser: MyUserDto | undefined;
		isAdmin: boolean;
		communityId: string;
	}>();

	const queryClient = useQueryClient();
	const removeMemberMutation = createRemoveMemberMutation(queryClient);

	async function handleRemoveMember(userId: string) {
		if (!confirm("Are you sure you want to remove this member?")) return;
		try {
			await removeMemberMutation.mutateAsync({ communityId, userId });
		} catch (error) {
			console.error("Failed to remove member", error);
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
							<p class="font-medium leading-none">{membership.user.name}</p>
							<p class="text-xs text-muted-foreground">Joined {new Date(membership.createdAt as string).toLocaleDateString()}</p>
						</div>
					</div>

					{#if isAdmin && membership.userId !== currentUser?.id}
						<Button
							variant="ghost"
							size="icon"
							class="h-8 w-8 text-muted-foreground hover:text-destructive"
							onclick={() => handleRemoveMember(membership.userId)}
							disabled={removeMemberMutation.isPending}
						>
							<HugeiconsIcon icon={Delete01Icon} size={16} strokeWidth={2} />
							<span class="sr-only">Remove member</span>
						</Button>
					{/if}
				</div>
			{/each}
		</div>
	</ScrollArea>
</div>
