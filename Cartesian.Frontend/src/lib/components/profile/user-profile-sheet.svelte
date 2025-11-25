<script lang="ts">
	import { Sheet, SheetContent, SheetTitle } from "$lib/components/ui/sheet";
	import { Avatar, AvatarImage, AvatarFallback } from "$lib/components/ui/avatar";
	import { Tabs, TabsList, TabsTrigger, TabsContent } from "$lib/components/ui/tabs";
	import { Button } from "$lib/components/ui/button";
	import { Badge } from "$lib/components/ui/badge";
	import { ScrollArea } from "$lib/components/ui/scroll-area";
	import { Skeleton } from "$lib/components/ui/skeleton";
	import { userProfileState, closeUserProfile } from "./profile-state.svelte";
	import { createQuery, useQueryClient } from "@tanstack/svelte-query";
	import { customInstance } from "$lib/api/client";
	import {
		getAccountApiPublicAccountId,
		getChatApiDmChannel,
		getGetAccountApiPublicAccountIdQueryKey,
	} from "$lib/api/cartesian-client";
	import type { CartesianUserDto, EventDto } from "$lib/api/cartesian-client";
	import { getAvatarUrl, getInitials, cn } from "$lib/utils";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import {
		Cancel01Icon,
		Calendar03Icon,
		Mail01Icon,
		UserBlock01Icon,
		UserRemove01Icon,
		Location01Icon,
	} from "@hugeicons/core-free-icons";
	import { format } from "date-fns";
	import { goto } from "$app/navigation";
	import { toast } from "svelte-sonner";
	import { authStore } from "$lib/stores/auth.svelte";

	interface BlockStatusResponse {
		youBlockedThem: boolean;
		theyBlockedYou: boolean;
	}

	const queryClient = useQueryClient();
	const auth = $derived($authStore);

	const userQuery = createQuery(() => ({
		queryKey: getGetAccountApiPublicAccountIdQueryKey(userProfileState.userId ?? ""),
		queryFn: ({ signal }) =>
			getAccountApiPublicAccountId(userProfileState.userId ?? "", signal),
		enabled: !!userProfileState.userId && userProfileState.open,
	}));

	const eventsQuery = createQuery(() => ({
		queryKey: ["userEvents", userProfileState.userId],
		queryFn: async () => {
			const res = await customInstance<EventDto[]>({
				url: `/account/api/public/${userProfileState.userId}/events`,
				method: "GET",
			});
			return res;
		},
		enabled: !!userProfileState.userId && userProfileState.open,
	}));

	const blockStatusQuery = createQuery(() => ({
		queryKey: ["blockStatus", userProfileState.userId],
		queryFn: async () => {
			const res = await customInstance<BlockStatusResponse>({
				url: `/account/api/block/${userProfileState.userId}/status`,
				method: "GET",
			});
			return res;
		},
		enabled: !!userProfileState.userId && userProfileState.open && auth.isAuthenticated,
	}));

	let user = $derived(userQuery.data as CartesianUserDto | undefined);
	let events = $derived((eventsQuery.data as EventDto[] | undefined) ?? []);
	let blockStatus = $derived(blockStatusQuery.data);
	let isBlocked = $derived(blockStatus?.youBlockedThem ?? false);
	let isBlockedByThem = $derived(blockStatus?.theyBlockedYou ?? false);
	let isOwnProfile = $derived(auth.user?.id === userProfileState.userId);

	let isBlockingUser = $state(false);

	async function handleStartDM() {
		if (!userProfileState.userId || !auth.isAuthenticated) {
			toast.error("Please log in to send messages");
			return;
		}

		if (isBlockedByThem) {
			toast.error("You cannot message this user");
			return;
		}

		try {
			const channel = await getChatApiDmChannel({ recipientId: userProfileState.userId });
			closeUserProfile();
			goto(`/app/dm/${channel.id}`);
		} catch (e) {
			console.error("Failed to get/create DM channel", e);
			toast.error("Failed to start conversation");
		}
	}

	async function handleToggleBlock() {
		if (!userProfileState.userId || !auth.isAuthenticated) return;

		isBlockingUser = true;
		try {
			if (isBlocked) {
				await customInstance({
					url: `/account/api/block/${userProfileState.userId}`,
					method: "DELETE",
				});
				toast.success("User unblocked");
			} else {
				await customInstance({
					url: `/account/api/block/${userProfileState.userId}`,
					method: "POST",
				});
				toast.success("User blocked");
			}
			await queryClient.invalidateQueries({
				queryKey: ["blockStatus", userProfileState.userId],
			});
		} catch (e) {
			console.error("Failed to toggle block", e);
			toast.error(isBlocked ? "Failed to unblock user" : "Failed to block user");
		} finally {
			isBlockingUser = false;
		}
	}

	function handleEventClick(eventId: string) {
		closeUserProfile();
		goto(`/app?event=${eventId}`);
	}
</script>

<Sheet
	open={userProfileState.open}
	onOpenChange={(open) => {
		if (!open) closeUserProfile();
	}}
>
	<SheetContent side="right" class="w-full p-0 sm:max-w-md">
		<div class="flex h-full flex-col">
			<div
				class="flex items-center justify-between border-b border-border/40 px-4 py-3"
			>
				<SheetTitle class="text-lg font-semibold">Profile</SheetTitle>
				<Button
					variant="ghost"
					size="icon"
					class="h-8 w-8 rounded-full"
					onclick={closeUserProfile}
				>
					<HugeiconsIcon icon={Cancel01Icon} size={16} strokeWidth={2} />
					<span class="sr-only">Close</span>
				</Button>
			</div>

			<ScrollArea class="flex-1">
				{#if userQuery.isLoading}
					<div class="p-6 space-y-4">
						<div class="flex flex-col items-center gap-4">
							<Skeleton class="h-24 w-24 rounded-full" />
							<Skeleton class="h-6 w-32" />
						</div>
						<div class="space-y-2">
							<Skeleton class="h-10 w-full" />
							<Skeleton class="h-10 w-full" />
						</div>
					</div>
				{:else if userQuery.isError}
					<div class="flex flex-col items-center justify-center p-8">
						<p class="text-sm text-muted-foreground">Failed to load profile</p>
					</div>
				{:else if user}
					<div class="p-6">
						<div class="flex flex-col items-center gap-4">
							<Avatar class="h-24 w-24 border-4 border-muted/30">
								<AvatarImage
									src={getAvatarUrl(user.avatar)}
									alt={user.name}
									class="object-cover"
								/>
								<AvatarFallback
									class="bg-muted text-2xl font-medium text-muted-foreground"
								>
									{getInitials(user.name)}
								</AvatarFallback>
							</Avatar>

							<div class="text-center">
								<h2 class="text-xl font-semibold">{user.name}</h2>
								{#if isBlockedByThem}
									<Badge variant="outline" class="mt-2 text-muted-foreground">
										This user has blocked you
									</Badge>
								{/if}
							</div>

							{#if !isOwnProfile && auth.isAuthenticated}
								<div class="flex gap-2">
									<Button
										variant="default"
										class="gap-2"
										onclick={handleStartDM}
										disabled={isBlockedByThem}
									>
										<HugeiconsIcon icon={Mail01Icon} size={16} />
										Message
									</Button>
									<Button
										variant={isBlocked ? "outline" : "ghost"}
										class={cn(
											"gap-2",
											isBlocked
												? "text-muted-foreground"
												: "text-destructive hover:text-destructive",
										)}
										onclick={handleToggleBlock}
										disabled={isBlockingUser}
									>
										<HugeiconsIcon
											icon={isBlocked ? UserRemove01Icon : UserBlock01Icon}
											size={16}
										/>
										{isBlocked ? "Unblock" : "Block"}
									</Button>
								</div>
							{/if}
						</div>

						<Tabs value="events" class="mt-6">
							<TabsList class="w-full">
								<TabsTrigger value="events" class="flex-1 gap-2">
									<HugeiconsIcon icon={Calendar03Icon} size={14} />
									Events
								</TabsTrigger>
							</TabsList>

							<TabsContent value="events" class="mt-4 space-y-3">
								{#if eventsQuery.isLoading}
									{#each Array(3) as _}
										<div class="rounded-lg border border-border/40 p-3">
											<Skeleton class="h-4 w-3/4" />
											<Skeleton class="mt-2 h-3 w-1/2" />
										</div>
									{/each}
								{:else if events.length === 0}
									<div
										class="rounded-lg border border-dashed border-border/40 p-6 text-center"
									>
										<HugeiconsIcon
											icon={Calendar03Icon}
											size={32}
											className="mx-auto text-muted-foreground/50"
										/>
										<p class="mt-2 text-sm text-muted-foreground">
											No public events
										</p>
									</div>
								{:else}
									{#each events as event}
										<button
											class="w-full rounded-lg border border-border/40 bg-muted/20 p-3 text-left transition-colors hover:bg-muted/40"
											onclick={() => handleEventClick(event.id)}
										>
											<h4 class="font-medium text-sm truncate">
												{event.name}
											</h4>
											{#if event.description}
												<p
													class="mt-1 text-xs text-muted-foreground line-clamp-2"
												>
													{event.description}
												</p>
											{/if}
											{#if event.windows && event.windows.length > 0}
												{@const firstWindow = event.windows[0]}
												<div
													class="mt-2 flex items-center gap-2 text-[10px] text-muted-foreground"
												>
													<HugeiconsIcon icon={Calendar03Icon} size={12} />
													<span>
														{format(
															new Date(firstWindow.startTime as string),
															"MMM d, yyyy",
														)}
													</span>
												</div>
											{/if}
										</button>
									{/each}
								{/if}
							</TabsContent>
						</Tabs>
					</div>
				{/if}
			</ScrollArea>
		</div>
	</SheetContent>
</Sheet>
