<script lang="ts">
	import { page } from "$app/stores";
	import { baseUrl } from "$lib/api/client";
	import { createGetMyMembershipsQuery, createGetPublicCommunitiesQuery } from "$lib/api/queries/community.query";
	import { newCommunityOverlayState } from "$lib/components/community/community-state.svelte";
	import * as Sidebar from "$lib/components/ui/sidebar";
	import { getLayoutContext } from "$lib/context/layout.svelte";
	import { PlusSignIcon } from "@hugeicons/core-free-icons";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import MobileNavigation from "./mobile-navigation.svelte";
	import NavigationHeader from "./navigation-header.svelte";



	const layout = getLayoutContext();

	const { children } = $props();

	const myMembershipsQuery = createGetMyMembershipsQuery();
	const publicCommunitiesQuery = createGetPublicCommunitiesQuery();
	
	function getAvatarUrl(avatar: { id: string } | null | undefined): string | null {
		if (!avatar) return null;
		return `${baseUrl}/media/api/${avatar.id}`;
	}

	function isActive(href: string) {
		return $page.url.pathname === href;
	}
</script>

<Sidebar.SidebarProvider open={true}>
	<Sidebar.Root variant="inset" collapsible="offcanvas" class="border-none">
		<NavigationHeader />
		<Sidebar.Content>
			<Sidebar.Header>
				Community center
			</Sidebar.Header>

			<Sidebar.Group>
				<Sidebar.GroupLabel>
					Your communities
				</Sidebar.GroupLabel>

				<Sidebar.GroupContent>
					<Sidebar.Menu>
						{#if myMembershipsQuery.isLoading}
							{#each Array(3) as _}
								<Sidebar.MenuItem>
									<Sidebar.MenuButton>
										<div class="h-4 w-4 animate-pulse rounded bg-muted"></div>
										<div class="h-4 w-24 animate-pulse rounded bg-muted"></div>
									</Sidebar.MenuButton>
								</Sidebar.MenuItem>
							{/each}
						{:else if myMembershipsQuery.isError}
							<Sidebar.MenuItem>
								<span class="px-2 text-xs text-destructive">Error loading communities</span>
							</Sidebar.MenuItem>
						{:else if myMembershipsQuery.data}
							{#each myMembershipsQuery.data as membership}
								<Sidebar.MenuItem>
									<Sidebar.MenuButton>
										{#snippet child({ props })}
											<a href="/app/community/{membership.communityId}" {...props}>
												{#if getAvatarUrl(membership.user.avatar)}
													<img
														src={getAvatarUrl(membership.user.avatar)}
														alt={membership.communityId}
														class="h-4 w-4 rounded object-cover"
													/>
												{:else}
													<div class="flex h-4 w-4 items-center justify-center rounded bg-muted text-[10px]">
														{membership.communityId.substring(0, 2).toUpperCase()}
													</div>
												{/if}
												<span>{membership.communityId}</span>
											</a>
										{/snippet}
									</Sidebar.MenuButton>
								</Sidebar.MenuItem>
							{/each}
							{#if myMembershipsQuery.data.length === 0}
								<Sidebar.MenuItem>
									<span class="px-2 text-xs text-muted-foreground">No communities joined</span>
								</Sidebar.MenuItem>
							{/if}
						{/if}
					</Sidebar.Menu>
				</Sidebar.GroupContent>
			</Sidebar.Group>

			<Sidebar.Group>
				<Sidebar.GroupLabel>
					Discover communities
				</Sidebar.GroupLabel>

				<Sidebar.GroupContent>
					<Sidebar.Menu>
						{#if publicCommunitiesQuery.isLoading}
							{#each Array(3) as _}
								<Sidebar.MenuItem>
									<Sidebar.MenuButton>
										<div class="h-4 w-4 animate-pulse rounded bg-muted"></div>
										<div class="h-4 w-24 animate-pulse rounded bg-muted"></div>
									</Sidebar.MenuButton>
								</Sidebar.MenuItem>
							{/each}
						{:else if publicCommunitiesQuery.isError}
							<Sidebar.MenuItem>
								<span class="px-2 text-xs text-destructive">Error loading communities</span>
							</Sidebar.MenuItem>
						{:else if publicCommunitiesQuery.data}
							{#each publicCommunitiesQuery.data as community}
								<Sidebar.MenuItem>
									<Sidebar.MenuButton>
										{#snippet child({ props })}
											<a href="/app/community/{community.id}" {...props}>
												{#if getAvatarUrl(community.avatar)}
													<img
														src={getAvatarUrl(community.avatar)}
														alt={community.name}
														class="h-4 w-4 rounded object-cover"
													/>
												{:else}
													<div class="flex h-4 w-4 items-center justify-center rounded bg-muted text-[10px]">
														{community.name.substring(0, 2).toUpperCase()}
													</div>
												{/if}
												<span>{community.name}</span>
											</a>
										{/snippet}
									</Sidebar.MenuButton>
								</Sidebar.MenuItem>
							{/each}
						{/if}

						<Sidebar.MenuButton
							onclick={() => {
								newCommunityOverlayState.open = true;
							}}
						>
							<HugeiconsIcon icon={PlusSignIcon} />
							New Community
						</Sidebar.MenuButton>
					</Sidebar.Menu>
				</Sidebar.GroupContent>
			</Sidebar.Group>
		</Sidebar.Content>
	</Sidebar.Root>

	<Sidebar.Inset>
		<main
			class="relative flex flex-1 flex-col overflow-hidden {layout.isMobile
				? 'pb-[72px]'
				: ''}"
		>
			{#if !layout.isMobile}
				<!-- <div class="absolute left-2 top-2 z-10">
          <Sidebar.Trigger />
        </div> -->
			{/if}
			{@render children?.()}
		</main>
	</Sidebar.Inset>
	{#if layout.isMobile}
		<!-- <MobileNavigation {navigationElements} translationFunction={getTranslation} /> -->
	{/if}
</Sidebar.SidebarProvider>
