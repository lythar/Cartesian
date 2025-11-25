<script lang="ts">
	import { page } from "$app/stores";
	import { baseUrl } from "$lib/api/client";
	import {
		createGetMyMembershipsQuery,
		createGetPublicCommunitiesQuery,
	} from "$lib/api/queries/community.query";
	import { newCommunityOverlayState } from "$lib/components/community/community-state.svelte";
	import DmSidebar from "$lib/components/chat/dm-sidebar.svelte";
	import * as Sidebar from "$lib/components/ui/sidebar";
	import { getLayoutContext } from "$lib/context/layout.svelte";
	import { authStore } from "$lib/stores/auth.svelte";
	import { unreadMessagesStore } from "$lib/stores/unread-messages.svelte";
	import { MapsGlobal01Icon, PlusSignIcon } from "@hugeicons/core-free-icons";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import MobileNavigation from "./mobile-navigation.svelte";
	import NavigationHeader from "./navigation-header.svelte";

	const layout = getLayoutContext();

	const { children } = $props();

	const myMembershipsQuery = createGetMyMembershipsQuery();
	const publicCommunitiesQuery = createGetPublicCommunitiesQuery();

	let isOpen = $derived($authStore.isAuthenticated);

	function getAvatarUrl(avatar: { id: string } | null | undefined): string | null {
		if (!avatar) return null;
		return `${baseUrl}/media/api/${avatar.id}`;
	}

	function isActive(href: string) {
		return $page.url.pathname === href;
	}

	function getUnreadCount(channelId: string | null | undefined): number {
		if (!channelId) return 0;
		return unreadMessagesStore.getUnreadCountForChannel(channelId);
	}
</script>

<Sidebar.SidebarProvider open={isOpen}>
	<Sidebar.Root variant="inset" collapsible="offcanvas" class="border-none">
		<NavigationHeader />
		<Sidebar.Content>
			{#if !$authStore.isAuthenticated}
				<div class="flex flex-col items-center justify-center p-6 text-center">
					<p class="text-sm text-muted-foreground">
						Sign in to access communities and join the conversation.
					</p>
					<a
						href="/login"
						class="mt-4 inline-block rounded bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90"
					>
						Sign in
					</a>
				</div>
			{:else}
				<div class="flex-1 overflow-auto px-3 py-4">
          <Sidebar.Group>
            <Sidebar.Menu>
              <Sidebar.MenuItem>
                <Sidebar.MenuButton
                  isActive={isActive('/app')}
                >
                  {#snippet child({ props })}
                    <a href="/app" {...props}>
                      <HugeiconsIcon
                        icon={MapsGlobal01Icon}
                        size={16}
                        strokeWidth={2}
                      />
                      <span>Map</span>
                    </a>
                  {/snippet}
                </Sidebar.MenuButton>
              </Sidebar.MenuItem>
            </Sidebar.Menu>
          </Sidebar.Group>

          <Sidebar.Group>
						<DmSidebar />
					</Sidebar.Group>

					<Sidebar.Group class="mt-6">
						<Sidebar.GroupLabel
							class="px-2 text-[10px] font-semibold tracking-wider text-muted-foreground/40 uppercase"
						>
							Your Communities
						</Sidebar.GroupLabel>

						<Sidebar.GroupContent>
							<Sidebar.Menu>
								{#if myMembershipsQuery.isLoading}
									{#each Array(3) as _}
										<Sidebar.MenuItem>
											<Sidebar.MenuButton
												class="h-8 w-full justify-start gap-2.5 rounded-lg px-2"
											>
												<div
													class="h-5 w-5 animate-pulse rounded-md bg-muted"
												></div>
												<div
													class="h-3 w-20 animate-pulse rounded bg-muted"
												></div>
											</Sidebar.MenuButton>
										</Sidebar.MenuItem>
									{/each}
								{:else if myMembershipsQuery.isError}
									<Sidebar.MenuItem>
										<span class="px-2 text-xs text-destructive"
											>Error loading communities</span
										>
									</Sidebar.MenuItem>
								{:else if myMembershipsQuery.data}
									{#each myMembershipsQuery.data as membership}
										{@const unreadCount = getUnreadCount(membership.channelId)}
										<Sidebar.MenuItem>
											<Sidebar.MenuButton
												isActive={isActive(
													`/community/${membership.communityId}`,
												)}
												class="group h-8 w-full justify-start gap-2.5 rounded-lg px-2 text-sm text-muted-foreground transition-all hover:bg-muted/50 hover:text-foreground data-[active=true]:bg-muted/50 data-[active=true]:font-medium data-[active=true]:text-foreground"
											>
												{#snippet child({ props })}
													<a
														href="/community/{membership.communityId}"
														{...props}
													>
														{#if membership.community?.avatar && getAvatarUrl(membership.community.avatar)}
															<img
																src={getAvatarUrl(membership.community.avatar)}
																alt={membership.community?.name ?? membership.communityId}
																class="h-5 w-5 rounded-md object-cover transition-transform group-hover:scale-105"
															/>
														{:else}
															<div
																class="flex h-5 w-5 items-center justify-center rounded-md bg-muted text-[9px] font-medium text-muted-foreground group-data-[active=true]:bg-primary/10 group-data-[active=true]:text-primary"
															>
																{(membership.community?.name ?? membership.communityId)
																	.substring(0, 2)
																	.toUpperCase()}
															</div>
														{/if}
														<span class="flex-1 truncate"
															>{membership.community?.name ?? membership.communityId}</span
														>
														{#if unreadCount > 0}
															<span class="flex h-5 min-w-5 items-center justify-center rounded-full bg-primary px-1.5 text-[10px] font-semibold text-primary-foreground">
																{unreadCount > 99 ? "99+" : unreadCount}
															</span>
														{/if}
													</a>
												{/snippet}
											</Sidebar.MenuButton>
										</Sidebar.MenuItem>
									{/each}
									{#if myMembershipsQuery.data.length === 0}
										<Sidebar.MenuItem>
											<span class="px-2 text-xs text-muted-foreground"
												>No communities joined</span
											>
										</Sidebar.MenuItem>
									{/if}
								{/if}
							</Sidebar.Menu>
						</Sidebar.GroupContent>
					</Sidebar.Group>

					<Sidebar.Group class="mt-6">
						<Sidebar.GroupLabel
							class="px-2 text-[10px] font-semibold tracking-wider text-muted-foreground/40 uppercase"
						>
							Discover
						</Sidebar.GroupLabel>

						<Sidebar.GroupContent>
							<Sidebar.Menu>
								{#if publicCommunitiesQuery.isLoading}
									{#each Array(3) as _}
										<Sidebar.MenuItem>
											<Sidebar.MenuButton
												class="h-8 w-full justify-start gap-2.5 rounded-lg px-2"
											>
												<div
													class="h-5 w-5 animate-pulse rounded-md bg-muted"
												></div>
												<div
													class="h-3 w-20 animate-pulse rounded bg-muted"
												></div>
											</Sidebar.MenuButton>
										</Sidebar.MenuItem>
									{/each}
								{:else if publicCommunitiesQuery.isError}
									<Sidebar.MenuItem>
										<span class="px-2 text-xs text-destructive"
											>Error loading communities</span
										>
									</Sidebar.MenuItem>
								{:else if publicCommunitiesQuery.data}
									{#each publicCommunitiesQuery.data as community}
										<Sidebar.MenuItem>
											<Sidebar.MenuButton
												isActive={isActive(`/community/${community.id}`)}
												class="group h-8 w-full justify-start gap-2.5 rounded-lg px-2 text-sm text-muted-foreground transition-all hover:bg-muted/50 hover:text-foreground data-[active=true]:bg-muted/50 data-[active=true]:font-medium data-[active=true]:text-foreground"
											>
												{#snippet child({ props })}
													<a href="/community/{community.id}" {...props}>
														{#if getAvatarUrl(community.avatar)}
															<img
																src={getAvatarUrl(community.avatar)}
																alt={community.name}
																class="h-5 w-5 rounded-md object-cover transition-transform group-hover:scale-105"
															/>
														{:else}
															<div
																class="flex h-5 w-5 items-center justify-center rounded-md bg-muted text-[9px] font-medium text-muted-foreground group-data-[active=true]:bg-primary/10 group-data-[active=true]:text-primary"
															>
																{community.name
																	.substring(0, 2)
																	.toUpperCase()}
															</div>
														{/if}
														<span class="truncate"
															>{community.name}</span
														>
													</a>
												{/snippet}
											</Sidebar.MenuButton>
										</Sidebar.MenuItem>
									{/each}
								{/if}

								<Sidebar.MenuItem>
									<Sidebar.MenuButton
										class="group h-8 w-full justify-start gap-2.5 rounded-lg px-2 text-sm text-muted-foreground transition-all hover:bg-muted/50 hover:text-foreground"
										onclick={() => {
											newCommunityOverlayState.open = true;
										}}
									>
										<div
											class="flex h-5 w-5 items-center justify-center rounded-md border border-dashed border-border group-hover:border-foreground/50"
										>
											<HugeiconsIcon
												icon={PlusSignIcon}
												size={12}
												strokeWidth={2}
											/>
										</div>
										<span class="font-medium">New Community</span>
									</Sidebar.MenuButton>
								</Sidebar.MenuItem>
							</Sidebar.Menu>
						</Sidebar.GroupContent>
					</Sidebar.Group>
				</div>
			{/if}
		</Sidebar.Content>
	</Sidebar.Root>

	<Sidebar.Inset>
		<main
			class="relative flex flex-1 flex-col overflow-hidden {layout.isMobile
				? ''
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
