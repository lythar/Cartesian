<script lang="ts">
	import * as Dialog from "$lib/components/ui/dialog";
	import * as Tabs from "$lib/components/ui/tabs";
	import * as Avatar from "$lib/components/ui/avatar";
	import { Button } from "$lib/components/ui/button";
	import { Input } from "$lib/components/ui/input";
	import { Label } from "$lib/components/ui/label";
	import { toast } from "$lib/components/ui/sonner";
	import { authStore } from "$lib/stores/auth.svelte";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import {
		Loading03Icon,
		Upload01Icon,
		Delete02Icon,
		UserIcon,
		LockKeyIcon,
		Mail01Icon,
		Cancel01Icon,
		CheckmarkCircle02Icon,
		UserBlock02Icon,
	} from "@hugeicons/core-free-icons";
	import {
		postMediaApiUploadAvatar,
		putAccountApiMeAvatar,
		deleteAccountApiMeAvatar,
		getAccountApiMe,
	} from "$lib/api";
	import { customInstance } from "$lib/api/client";
	import { createQuery, createMutation, useQueryClient } from "@tanstack/svelte-query";
	import type { CartesianUserDto } from "$lib/api/cartesian-client";

	interface UserBlockDto {
		id: string;
		blockedUserId: string;
		blockedUser: CartesianUserDto;
		createdAt: string;
	}
	import { baseUrl } from "$lib/api/client";
	import ChangePasswordForm from "$lib/components/auth/change-password-form.svelte";
	import { ScrollArea } from "$lib/components/ui/scroll-area";
	import { page } from "$app/stores";
	import { pushState } from "$app/navigation";
	import { browser } from "$app/environment";

	let { open = $bindable(false) } = $props();

	const auth = $derived($authStore);
	const queryClient = useQueryClient();

	let previousState = $state(!!$page.state.profileDialog);

	$effect(() => {
		const currentState = !!$page.state.profileDialog;

		if (open) {
			if (!currentState) {
				if (previousState) {
					// State was present, now gone -> Back button pressed
					open = false;
				} else {
					// State wasn't present, still isn't -> Initial open
					if (browser) {
						pushState("", { profileDialog: true });
					}
				}
			}
		} else {
			if (currentState && browser) {
				history.back();
			}
		}

		previousState = currentState;
	});

	const blocksQuery = createQuery(() => ({
		queryKey: ["myBlocks"],
		queryFn: async () => {
			const res = await customInstance<UserBlockDto[]>({
				url: "/account/api/blocks",
				method: "GET",
			});
			return res;
		},
		enabled: open,
	}));

	const unblockMutation = createMutation(() => ({
		mutationFn: async (userId: string) => {
			await customInstance({ url: `/account/api/block/${userId}`, method: "DELETE" });
		},
		onSuccess: () => {
			queryClient.invalidateQueries({ queryKey: ["myBlocks"] });
		},
	}));

	const blocks = $derived(blocksQuery.data ?? []);

	let isLoadingAvatar = $state(false);
	let isSavingProfile = $state(false);
	let fileInput = $state<HTMLInputElement | null>(null);

	// Profile Form State
	let username = $state("");
	let selectedFile = $state<File | null>(null);
	let previewUrl = $state<string | null>(null);

	$effect(() => {
		if (open && auth.user) {
			username = auth.user.name;
			selectedFile = null;
			previewUrl = null;
		}
	});

	$effect(() => {
		return () => {
			if (previewUrl) {
				URL.revokeObjectURL(previewUrl);
			}
		};
	});

	function getInitials(name: string): string {
		return name
			.split(" ")
			.map((n) => n[0])
			.join("")
			.toUpperCase()
			.slice(0, 2);
	}

	function getAvatarUrl(avatar: { id: string } | null): string | null {
		if (!avatar) return null;
		return `${baseUrl}/media/api/${avatar.id}`;
	}

	function handleFileChange(event: Event) {
		const target = event.target as HTMLInputElement;
		const file = target.files?.[0];
		if (!file) return;

		// 5MB limit check (aligned with backend)
		if (file.size > 5 * 1024 * 1024) {
			toast.error("Image must be smaller than 5MB");
			if (fileInput) fileInput.value = "";
			return;
		}

		// Revoke previous preview URL if it exists
		if (previewUrl) {
			URL.revokeObjectURL(previewUrl);
		}

		// Create new preview
		selectedFile = file;
		previewUrl = URL.createObjectURL(file);
	}

	function handleRemoveAvatar() {
		if (previewUrl) {
			URL.revokeObjectURL(previewUrl);
		}
		selectedFile = null;
		previewUrl = null;
		if (fileInput) fileInput.value = "";
	}

	async function handleSaveProfile() {
		if (!auth.user) return;

		isSavingProfile = true;
		try {
			// Upload new avatar if selected
			if (selectedFile) {
				const uploadRes = await postMediaApiUploadAvatar({ file: selectedFile });
				await putAccountApiMeAvatar({ mediaId: uploadRes.id });
				const userData = await getAccountApiMe();
				authStore.setUser(userData);
			}

			toast.success("Profile updated successfully");

			// Clean up
			if (previewUrl) {
				URL.revokeObjectURL(previewUrl);
			}
			selectedFile = null;
			previewUrl = null;
			if (fileInput) fileInput.value = "";
		} catch (error) {
			console.error(error);
			toast.error("Failed to update profile");
		} finally {
			isSavingProfile = false;
		}
	}
</script>

<Dialog.Root bind:open>
	<Dialog.Content
		class="fixed inset-0 top-0 left-0 z-50 flex h-full max-h-none w-full max-w-none translate-x-0 translate-y-0 flex-col gap-0 overflow-hidden rounded-none border-0 bg-background/95 p-0 shadow-none backdrop-blur-xl md:fixed md:top-[50%] md:left-[50%] md:h-[600px] md:max-h-[600px] md:w-full md:max-w-[600px] md:-translate-x-1/2 md:-translate-y-1/2 md:rounded-3xl md:border md:border-border/40 md:shadow-2xl"
		showCloseButton={false}
	>
		<div
			class="flex flex-none items-center justify-between border-b border-border/10 px-6 py-4"
		>
			<div>
				<Dialog.Title class="text-xl font-semibold tracking-tight">Settings</Dialog.Title>
				<Dialog.Description class="text-xs font-medium text-muted-foreground">
					Manage your account preferences
				</Dialog.Description>
			</div>
			<Button
				variant="ghost"
				size="icon"
				class="h-8 w-8 rounded-full transition-colors hover:bg-muted"
				onclick={() => (open = false)}
			>
				<HugeiconsIcon icon={Cancel01Icon} className="size-4" strokeWidth={2} />
			</Button>
		</div>

		<Tabs.Root value="profile" class="flex flex-1 flex-col overflow-hidden">
			<div class="flex-none px-6 pt-4">
				<Tabs.List class="flex w-full items-center rounded-xl bg-muted/30 p-1">
					<Tabs.Trigger
						value="profile"
						class="flex-1 gap-2 rounded-lg data-[state=active]:bg-background data-[state=active]:text-foreground data-[state=active]:shadow-sm"
					>
						<HugeiconsIcon icon={UserIcon} className="size-4" />
						Profile
					</Tabs.Trigger>
					<Tabs.Trigger
						value="security"
						class="flex-1 gap-2 rounded-lg data-[state=active]:bg-background data-[state=active]:text-foreground data-[state=active]:shadow-sm"
					>
						<HugeiconsIcon icon={LockKeyIcon} className="size-4" />
						Security
					</Tabs.Trigger>
					<Tabs.Trigger
						value="blocked"
						class="flex-1 gap-2 rounded-lg data-[state=active]:bg-background data-[state=active]:text-foreground data-[state=active]:shadow-sm"
					>
						<HugeiconsIcon icon={UserBlock02Icon} className="size-4" />
						Blocked
					</Tabs.Trigger>
				</Tabs.List>
			</div>

			<div class="flex-1 overflow-y-auto">
				<Tabs.Content value="profile" class="mt-0 h-full space-y-6 p-6">
					<!-- Avatar Section -->
					<div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:gap-6">
						<div class="group relative">
							<Avatar.Root class="size-24 border-4 border-muted/30 shadow-sm">
								<Avatar.Image
									src={previewUrl ?? getAvatarUrl(auth.user?.avatar ?? null)}
									alt={auth.user?.name}
									class="object-cover"
								/>
								<Avatar.Fallback
									class="bg-muted text-2xl font-medium text-muted-foreground"
								>
									{auth.user ? getInitials(auth.user.name) : "?"}
								</Avatar.Fallback>
							</Avatar.Root>
							{#if previewUrl}
								<div
									class="absolute -top-1 -right-1 rounded-full bg-primary p-1 shadow-md"
								>
									<HugeiconsIcon
										icon={CheckmarkCircle02Icon}
										className="size-3 text-primary-foreground"
									/>
								</div>
							{/if}
						</div>

						<div class="flex flex-1 flex-col gap-2">
							<div class="flex flex-col gap-1">
								<h3 class="leading-none font-medium">Profile Picture</h3>
								<p class="text-xs text-muted-foreground">
									JPG, GIF or PNG. Max size of 5MB.
								</p>
							</div>
							<div class="flex gap-2">
								<Button
									variant="outline"
									size="sm"
									class="h-8 rounded-full border-border/50 bg-background/50 hover:bg-muted/50"
									onclick={() => fileInput?.click()}
									disabled={isSavingProfile}
								>
									<HugeiconsIcon icon={Upload01Icon} className="mr-2 size-3.5" />
									{previewUrl ? "Change" : "Upload"}
								</Button>
								{#if previewUrl}
									<Button
										variant="ghost"
										size="sm"
										class="h-8 rounded-full text-muted-foreground hover:text-destructive"
										onclick={handleRemoveAvatar}
										disabled={isSavingProfile}
									>
										<HugeiconsIcon
											icon={Delete02Icon}
											className="mr-2 size-3.5"
										/>
										Remove
									</Button>
								{/if}
							</div>
							<input
								bind:this={fileInput}
								type="file"
								accept="image/jpeg,image/png,image/gif,image/webp"
								class="hidden"
								onchange={handleFileChange}
							/>
						</div>
					</div>

					<div class="h-px w-full bg-border/20"></div>

					<!-- Personal Info Section -->
					<div class="space-y-4">
						<div class="space-y-3">
							<Label
								class="text-xs font-semibold tracking-wider text-muted-foreground uppercase"
							>
								Display Name
							</Label>
							<Input
								bind:value={username}
								placeholder="Your display name"
								class="h-10 border-border/50 bg-muted/30 px-3 font-medium shadow-none transition-all focus-visible:bg-background focus-visible:ring-1"
							/>
						</div>

						<div class="space-y-3">
							<Label
								class="text-xs font-semibold tracking-wider text-muted-foreground uppercase"
							>
								Email Address
							</Label>
							<div class="relative">
								<HugeiconsIcon
									icon={Mail01Icon}
									className="absolute left-3 top-2.5 size-4 text-muted-foreground/70"
								/>
								<Input
									value={auth.user?.email}
									disabled
									class="h-10 border-border/50 bg-muted/10 pl-9 text-muted-foreground shadow-none"
								/>
							</div>
							<p class="text-[10px] text-muted-foreground">
								Email address cannot be changed directly.
							</p>
						</div>
					</div>

					<div class="flex justify-end pt-2">
						<Button
							onclick={handleSaveProfile}
							disabled={isSavingProfile || !selectedFile}
							class="rounded-xl bg-primary font-medium shadow-lg shadow-primary/20 transition-all hover:shadow-primary/30 active:scale-[0.98] disabled:opacity-50"
						>
							{#if isSavingProfile}
								<HugeiconsIcon
									icon={Loading03Icon}
									className="mr-2 size-4 animate-spin"
								/>
								Saving...
							{:else}
								<HugeiconsIcon
									icon={CheckmarkCircle02Icon}
									className="mr-2 size-4"
								/>
								Save Changes
							{/if}
						</Button>
					</div>
				</Tabs.Content>

				<Tabs.Content value="security" class="mt-0 h-full space-y-6 p-6">
					<div class="rounded-xl border border-amber-500/20 bg-amber-500/10 p-4">
						<div class="flex items-start gap-3">
							<div
								class="rounded-full bg-amber-500/20 p-1.5 text-amber-600 dark:text-amber-500"
							>
								<HugeiconsIcon icon={LockKeyIcon} className="size-4" />
							</div>
							<div class="space-y-1">
								<h4 class="text-sm font-medium text-amber-900 dark:text-amber-100">
									Password Security
								</h4>
								<p class="text-xs text-amber-800/80 dark:text-amber-200/70">
									Ensure your account stays secure by using a strong, unique
									password.
								</p>
							</div>
						</div>
					</div>

					<ChangePasswordForm />
				</Tabs.Content>

				<Tabs.Content value="blocked" class="mt-0 h-full space-y-6 p-6">
					<div class="rounded-xl border border-destructive/20 bg-destructive/5 p-4">
						<div class="flex items-start gap-3">
							<div class="rounded-full bg-destructive/20 p-1.5 text-destructive">
								<HugeiconsIcon icon={UserBlock02Icon} className="size-4" />
							</div>
							<div class="space-y-1">
								<h4 class="text-sm font-medium">Blocked Users</h4>
								<p class="text-xs text-muted-foreground">
									Users you've blocked can't send you direct messages. You won't
									see their messages in DMs.
								</p>
							</div>
						</div>
					</div>

					<ScrollArea class="h-[300px]">
						{#if blocksQuery.isLoading}
							<div class="py-8 text-center text-sm text-muted-foreground">
								Loading...
							</div>
						{:else if blocks.length === 0}
							<div class="py-8 text-center text-sm text-muted-foreground">
								You haven't blocked anyone
							</div>
						{:else}
							<div class="space-y-2">
								{#each blocks as block (block.blockedUserId)}
									<div
										class="flex items-center justify-between rounded-lg border border-border/50 bg-muted/20 p-3"
									>
										<div class="flex items-center gap-3">
											<Avatar.Root class="h-10 w-10">
												<Avatar.Image
													src={block.blockedUser?.avatar
														? `${baseUrl}/media/api/${block.blockedUser.avatar.id}`
														: null}
													alt={block.blockedUser?.name}
												/>
												<Avatar.Fallback class="text-sm">
													{block.blockedUser?.name
														?.substring(0, 2)
														.toUpperCase() ?? "??"}
												</Avatar.Fallback>
											</Avatar.Root>
											<div>
												<p class="text-sm font-medium">
													{block.blockedUser?.name ?? "Unknown"}
												</p>
												<p class="text-xs text-muted-foreground">
													Blocked {new Date(
														block.createdAt as string,
													).toLocaleDateString()}
												</p>
											</div>
										</div>
										<Button
											variant="outline"
											size="sm"
											class="text-destructive hover:bg-destructive/10 hover:text-destructive"
											onclick={() =>
												unblockMutation.mutate(block.blockedUserId)}
											disabled={unblockMutation.isPending}
										>
											{unblockMutation.isPending
												? "Unblocking..."
												: "Unblock"}
										</Button>
									</div>
								{/each}
							</div>
						{/if}
					</ScrollArea>
				</Tabs.Content>
			</div>
		</Tabs.Root>
	</Dialog.Content>
</Dialog.Root>
