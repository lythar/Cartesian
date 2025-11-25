<script lang="ts">
	import { createMutation, createQuery, useQueryClient } from "@tanstack/svelte-query";
	import { customInstance, baseUrl } from "$lib/api/client";
	import type { CartesianUserDto } from "$lib/api/cartesian-client";
	import * as Avatar from "$lib/components/ui/avatar";
	import * as AlertDialog from "$lib/components/ui/alert-dialog";
	import { Badge } from "$lib/components/ui/badge";
	import { Button } from "$lib/components/ui/button";
	import { ScrollArea } from "$lib/components/ui/scroll-area";
	import { Cancel01Icon } from "@hugeicons/core-free-icons";
	import { HugeiconsIcon } from "@hugeicons/svelte";

	interface CommunityBanDto {
		id: string;
		communityId: string;
		userId: string;
		user: CartesianUserDto;
		bannedById: string;
		bannedBy: CartesianUserDto;
		reason: string | null;
		createdAt: string;
	}

	let { communityId } = $props<{ communityId: string }>();

	const queryClient = useQueryClient();

	const bansQuery = createQuery(() => ({
		queryKey: ["community", communityId, "bans"],
		queryFn: async () => {
			const res = await customInstance<CommunityBanDto[]>({
				url: `/community/api/${communityId}/bans`,
				method: "GET",
			});
			return res;
		},
	}));

	const unbanMutation = createMutation(() => ({
		mutationFn: async (userId: string) => {
			await customInstance({
				url: `/community/api/${communityId}/ban/${userId}`,
				method: "DELETE",
			});
		},
		onSuccess: () => {
			queryClient.invalidateQueries({ queryKey: ["community", communityId, "bans"] });
		},
	}));

	let unbanDialogOpen = $state(false);
	let selectedBan = $state<{ userId: string; userName: string } | null>(null);

	function openUnbanDialog(userId: string, userName: string) {
		selectedBan = { userId, userName };
		unbanDialogOpen = true;
	}

	async function handleUnban() {
		if (!selectedBan) return;
		try {
			await unbanMutation.mutateAsync(selectedBan.userId);
			unbanDialogOpen = false;
			selectedBan = null;
		} catch (error) {
			console.error("Failed to unban user", error);
		}
	}

	const bans = $derived(bansQuery.data ?? []);
</script>

<div class="flex flex-col h-full">
	<div
		class="flex h-16 items-center justify-between border-b border-border/40 bg-background/95 backdrop-blur px-4"
	>
		<h2 class="text-xs font-semibold text-foreground/70 uppercase tracking-wider">
			Banned Users
		</h2>
		<Badge variant="outline" class="text-[10px] py-0 h-4 border-border/50 text-muted-foreground">
			{bans.length}
		</Badge>
	</div>

	<ScrollArea class="flex-1">
		{#if bansQuery.isLoading}
			<div class="p-4 text-center text-muted-foreground text-sm">Loading...</div>
		{:else if bans.length === 0}
			<div class="p-4 text-center text-muted-foreground text-sm">No banned users</div>
		{:else}
			<div class="p-2 space-y-0.5">
				{#each bans as ban (ban.userId)}
					<div
						class="group flex items-center justify-between rounded-md px-2 py-1.5 transition-colors hover:bg-muted/50"
					>
						<div class="flex items-center gap-3 overflow-hidden">
							<Avatar.Root class="h-8 w-8 rounded-full border border-border/40">
								<Avatar.Image
									src={ban.user?.avatar ? `${baseUrl}/media/api/${ban.user.avatar.id}` : null}
									alt={ban.user?.name}
									class="object-cover"
								/>
								<Avatar.Fallback class="bg-muted text-[10px] font-medium text-foreground/70">
									{ban.user?.name?.substring(0, 2).toUpperCase() ?? "??"}
								</Avatar.Fallback>
							</Avatar.Root>
							<div class="min-w-0">
								<p class="text-sm font-medium leading-none text-foreground truncate">
									{ban.user?.name ?? "Unknown"}
								</p>
								{#if ban.reason}
									<p class="text-[10px] text-muted-foreground truncate">
										{ban.reason}
									</p>
								{/if}
								<p class="text-[10px] text-muted-foreground truncate">
									Banned {new Date(ban.createdAt as string).toLocaleDateString()}
								</p>
							</div>
						</div>

						<Button
							variant="ghost"
							size="icon"
							class="h-6 w-6 opacity-0 transition-opacity group-hover:opacity-100 text-destructive hover:text-destructive"
							onclick={() => openUnbanDialog(ban.userId, ban.user?.name ?? "Unknown")}
						>
							<HugeiconsIcon icon={Cancel01Icon} size={14} strokeWidth={1.5} />
							<span class="sr-only">Unban</span>
						</Button>
					</div>
				{/each}
			</div>
		{/if}
	</ScrollArea>
</div>

<AlertDialog.Root bind:open={unbanDialogOpen}>
	<AlertDialog.Content>
		<AlertDialog.Header>
			<AlertDialog.Title>Unban User</AlertDialog.Title>
			<AlertDialog.Description>
				Are you sure you want to unban <span class="font-semibold">{selectedBan?.userName}</span>? They will be able to rejoin the community.
			</AlertDialog.Description>
		</AlertDialog.Header>
		<AlertDialog.Footer>
			<AlertDialog.Cancel>Cancel</AlertDialog.Cancel>
			<AlertDialog.Action onclick={handleUnban} disabled={unbanMutation.isPending}>
				{unbanMutation.isPending ? "Unbanning..." : "Unban"}
			</AlertDialog.Action>
		</AlertDialog.Footer>
	</AlertDialog.Content>
</AlertDialog.Root>
