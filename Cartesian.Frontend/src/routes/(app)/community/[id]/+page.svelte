<script lang="ts">
	import { page } from "$app/stores";
	import {
		createGetCommunityMembersQuery,
		createGetCommunityQuery,
		createGetMyMembershipsQuery,
	} from "$lib/api/queries/community.query";
	import { createGetMeQuery } from "$lib/api/queries/user.query";
	import CommunityChat from "$lib/components/chat/community-chat.svelte";
	import { ChatState } from "$lib/components/chat/chat.state.svelte";
	import CommunityHeader from "$lib/components/community/community-header.svelte";
	import MembersList from "$lib/components/community/members-list.svelte";
	import * as Sheet from "$lib/components/ui/sheet";
	import { Skeleton } from "$lib/components/ui/skeleton";
	import { useQueryClient } from "@tanstack/svelte-query";
	import { onDestroy, untrack } from "svelte";

	const communityId = $derived($page.params.id);
	const queryClient = useQueryClient();


	const communityQuery = createGetCommunityQuery(() => communityId ?? "", {}, queryClient);
	const membersQuery = createGetCommunityMembersQuery(
		() => communityId ?? "",
		() => ({}),
		{},
		queryClient,
	);
	const meQuery = createGetMeQuery({}, queryClient);
	const myMembershipsQuery = createGetMyMembershipsQuery(() => ({}), {}, queryClient);

	const community = $derived(communityQuery.data);
	const members = $derived(membersQuery.data ?? []);
	const currentUser = $derived(meQuery.data);
	const myMemberships = $derived(myMembershipsQuery.data ?? []);

	const isMember = $derived(myMemberships.some((m) => m.communityId === communityId));
	const currentMembership = $derived(myMemberships.find((m) => m.communityId === communityId));
	const hasAdminPermissions = $derived((currentMembership?.permissions ?? 0) >= 2);
	const isOwner = $derived(((currentMembership?.permissions ?? 0) & (1 << 31)) !== 0);

	let chatState = $state<ChatState | null>(null);
	let membersSheetOpen = $state(false);

	$effect(() => {
		if (communityId) {
			const currentState = untrack(() => chatState);
			if (currentState) currentState.dispose();
			chatState = new ChatState(communityId, queryClient);
		}
	});

	onDestroy(() => {
		if (chatState) chatState.dispose();
	});
</script>

<div class="flex flex-1 h-full w-full overflow-hidden bg-background">
	{#if communityQuery.isLoading}
		<div class="flex flex-1 items-center justify-center">
			<div class="flex flex-col gap-4 items-center">
				<Skeleton class="h-12 w-12 rounded-full" />
				<Skeleton class="h-4 w-32" />
			</div>
		</div>
	{:else if communityQuery.isError}
		<div class="flex flex-1 items-center justify-center p-8">
			<div class="rounded-xl border border-destructive/20 bg-destructive/5 p-8 text-center max-w-md">
				<h3 class="text-lg font-semibold text-destructive">Error loading community</h3>
				<p class="mt-2 text-muted-foreground">
					{communityQuery.error?.message || "Something went wrong."}
				</p>
			</div>
		</div>
	{:else if community && chatState}
		<div class="flex flex-1 flex-col min-w-0">
			<CommunityHeader
				{community}
				{currentUser}
				isMember={!!currentMembership}
				isAdmin={hasAdminPermissions}
				{isOwner}
				onToggleMembers={() => (membersSheetOpen = !membersSheetOpen)}
			/>

			<CommunityChat {chatState} {members} {currentUser} />
		</div>

		<aside class="hidden w-64 border-l border-border/40 bg-muted/10 lg:block">
			<MembersList
				{members}
				{currentUser}
				userPermissions={currentMembership?.permissions ?? 0}
				communityId={communityId ?? ""}
			/>
		</aside>

		<Sheet.Root bind:open={membersSheetOpen}>
			<Sheet.Content side="right" class="w-72 p-0 sm:w-80">
				<Sheet.Header class="sr-only">
					<Sheet.Title>Members</Sheet.Title>
					<Sheet.Description>Community members list</Sheet.Description>
				</Sheet.Header>
				<div class="h-full w-full pt-10">
					<MembersList
						{members}
						{currentUser}
						userPermissions={currentMembership?.permissions ?? 0}
						communityId={communityId ?? ""}
					/>
				</div>
			</Sheet.Content>
		</Sheet.Root>
	{/if}
</div>
