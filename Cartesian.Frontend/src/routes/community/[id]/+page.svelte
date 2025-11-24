<script lang="ts">
	import { page } from "$app/stores";
	import {
		createGetCommunityMembersQuery,
		createGetCommunityQuery,
		createGetMyMembershipsQuery
	} from "$lib/api/queries/community.query";
	import { createGetMeQuery } from "$lib/api/queries/user.query";
	import CommunityChat from "$lib/components/chat/community-chat.svelte";
	import CommunityHeader from "$lib/components/community/community-header.svelte";
	import MembersList from "$lib/components/community/members-list.svelte";
	import { Skeleton } from "$lib/components/ui/skeleton";
	import { useQueryClient } from "@tanstack/svelte-query";

	const communityId = $derived($page.params.id);
	const queryClient = useQueryClient();

	const communityQuery = createGetCommunityQuery(() => communityId ?? "", {}, queryClient);
	const membersQuery = createGetCommunityMembersQuery(() => communityId ?? "", () => ({}), {}, queryClient);
	const meQuery = createGetMeQuery({}, queryClient);
	const myMembershipsQuery = createGetMyMembershipsQuery(() => ({}), {}, queryClient);

	const community = $derived(communityQuery.data);
	const members = $derived(membersQuery.data ?? []);
	const currentUser = $derived(meQuery.data);
	const myMemberships = $derived(myMembershipsQuery.data ?? []);

	const isMember = $derived(myMemberships.some((m) => m.communityId === communityId));

  const currentMembership = $derived(myMemberships.find((m) => m.communityId === communityId));
  const hasAdminPermissions = $derived((currentMembership?.permissions ?? 0) >= 2);

</script>

<div class="container mx-auto max-w-5xl space-y-8 py-8">
	{#if communityQuery.isLoading}
		<div class="space-y-6">
			<Skeleton class="h-64 w-full rounded-3xl" />
			<Skeleton class="h-12 w-48" />
		</div>
	{:else if communityQuery.isError}
		<div class="rounded-3xl border border-destructive/20 bg-destructive/5 p-8 text-center">
			<h3 class="text-lg font-semibold text-destructive">Error loading community</h3>
			<p class="text-muted-foreground">
				{communityQuery.error?.message || "Something went wrong."}
			</p>
		</div>
	{:else if community}
		<CommunityHeader
			{community}
			{currentUser}
			isMember={!!currentMembership}
			isAdmin={hasAdminPermissions}
		/>

		<div class="grid gap-8 md:grid-cols-3">
			<div class="md:col-span-2 space-y-8">
				<CommunityChat
					communityId={communityId ?? ""}
					{members}
					{currentUser}
				/>
			</div>

			<div class="space-y-8">
				<MembersList
					{members}
					{currentUser}
					userPermissions={currentMembership?.permissions ?? 0}
					communityId={communityId ? communityId : ""}
				/>
			</div>
		</div>
	{/if}
</div>
