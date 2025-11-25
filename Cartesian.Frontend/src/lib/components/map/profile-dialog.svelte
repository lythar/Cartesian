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
	} from "@hugeicons/core-free-icons";
	import { m } from "$lib/paraglide/messages";
	import {
		postMediaApiUploadAvatar,
		putAccountApiMeAvatar,
		deleteAccountApiMeAvatar,
		getAccountApiMe,
	} from "$lib/api";
	import { baseUrl } from "$lib/api/client";

	let { open = $bindable(false) } = $props();

	const auth = $derived($authStore);
	let isLoadingAvatar = $state(false);
	let isSavingProfile = $state(false);
	let isSavingPassword = $state(false);
	let fileInput = $state<HTMLInputElement | null>(null);

	// Profile Form State
	let username = $state("");
	let selectedFile = $state<File | null>(null);
	let previewUrl = $state<string | null>(null);

	// Password Form State
	let currentPassword = $state("");
	let newPassword = $state("");
	let confirmPassword = $state("");

	$effect(() => {
		if (open && auth.user) {
			username = auth.user.name;
			selectedFile = null;
			previewUrl = null;
			currentPassword = "";
			newPassword = "";
			confirmPassword = "";
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
			toast.error(m.toast_image_too_large());
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

			toast.success(m.toast_profile_updated());

			// Clean up
			if (previewUrl) {
				URL.revokeObjectURL(previewUrl);
			}
			selectedFile = null;
			previewUrl = null;
			if (fileInput) fileInput.value = "";
		} catch (error) {
			console.error(error);
			toast.error(m.toast_profile_update_failed());
		} finally {
			isSavingProfile = false;
		}
	}

	function handleUpdatePassword() {
		if (newPassword !== confirmPassword) {
			toast.error(m.toast_passwords_dont_match());
			return;
		}

		isSavingPassword = true;
		// Mock API call
		setTimeout(() => {
			toast.info(m.toast_password_update_not_implemented(), {
				description: m.profile_feature_requires_backend(),
			});
			isSavingPassword = false;
		}, 800);
	}
</script>

<Dialog.Root bind:open>
	<Dialog.Content
		class="flex h-[600px] max-w-[500px] flex-col gap-0 overflow-hidden rounded-3xl border border-border/40 bg-background/95 p-0 shadow-2xl backdrop-blur-xl md:max-w-[600px]"
		showCloseButton={false}
	>
		<div
			class="flex flex-none items-center justify-between border-b border-border/10 px-6 py-4"
		>
			<div>
				<Dialog.Title class="text-xl font-semibold tracking-tight"
					>{m.profile_settings_title()}</Dialog.Title
				>
				<Dialog.Description class="text-xs font-medium text-muted-foreground">
					{m.profile_settings_subtitle()}
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
						{m.profile_tab_profile()}
					</Tabs.Trigger>
					<Tabs.Trigger
						value="security"
						class="flex-1 gap-2 rounded-lg data-[state=active]:bg-background data-[state=active]:text-foreground data-[state=active]:shadow-sm"
					>
						<HugeiconsIcon icon={LockKeyIcon} className="size-4" />
						{m.profile_tab_security()}
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
								<h3 class="leading-none font-medium">
									{m.profile_picture_title()}
								</h3>
								<p class="text-xs text-muted-foreground">
									{m.profile_picture_formats()}
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
									{previewUrl ? m.common_change() : m.common_upload()}
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
										{m.common_remove()}
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
								{m.profile_display_name_label()}
							</Label>
							<Input
								bind:value={username}
								placeholder={m.profile_display_name_placeholder()}
								class="h-10 border-border/50 bg-muted/30 px-3 font-medium shadow-none transition-all focus-visible:bg-background focus-visible:ring-1"
							/>
						</div>

						<div class="space-y-3">
							<Label
								class="text-xs font-semibold tracking-wider text-muted-foreground uppercase"
							>
								{m.profile_email_label()}
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
								{m.profile_email_note()}
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
								{m.common_saving()}
							{:else}
								<HugeiconsIcon
									icon={CheckmarkCircle02Icon}
									className="mr-2 size-4"
								/>
								{m.common_save_changes()}
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
									{m.profile_password_security_title()}
								</h4>
								<p class="text-xs text-amber-800/80 dark:text-amber-200/70">
									{m.profile_password_security_description()}
								</p>
							</div>
						</div>
					</div>

					<div class="space-y-4">
						<div class="space-y-3">
							<Label
								class="text-xs font-semibold tracking-wider text-muted-foreground uppercase"
							>
								{m.profile_current_password_label()}
							</Label>
							<Input
								type="password"
								bind:value={currentPassword}
								class="h-10 border-border/50 bg-muted/30 px-3 shadow-none transition-all focus-visible:bg-background focus-visible:ring-1"
							/>
						</div>

						<div class="grid gap-4 sm:grid-cols-2">
							<div class="space-y-3">
								<Label
									class="text-xs font-semibold tracking-wider text-muted-foreground uppercase"
								>
									{m.profile_new_password_label()}
								</Label>
								<Input
									type="password"
									bind:value={newPassword}
									class="h-10 border-border/50 bg-muted/30 px-3 shadow-none transition-all focus-visible:bg-background focus-visible:ring-1"
								/>
							</div>

							<div class="space-y-3">
								<Label
									class="text-xs font-semibold tracking-wider text-muted-foreground uppercase"
								>
									{m.profile_confirm_password_label()}
								</Label>
								<Input
									type="password"
									bind:value={confirmPassword}
									class="h-10 border-border/50 bg-muted/30 px-3 shadow-none transition-all focus-visible:bg-background focus-visible:ring-1"
								/>
							</div>
						</div>
					</div>

					<div class="flex justify-end pt-2">
						<Button
							onclick={handleUpdatePassword}
							disabled={isSavingPassword}
							class="rounded-xl bg-primary font-medium shadow-lg shadow-primary/20 transition-all hover:shadow-primary/30 active:scale-[0.98]"
						>
							{#if isSavingPassword}
								<HugeiconsIcon
									icon={Loading03Icon}
									className="mr-2 size-4 animate-spin"
								/>
								{m.common_updating()}
							{:else}
								<HugeiconsIcon
									icon={CheckmarkCircle02Icon}
									className="mr-2 size-4"
								/>
								{m.profile_update_password()}
							{/if}
						</Button>
					</div>
				</Tabs.Content>
			</div>
		</Tabs.Root>
	</Dialog.Content>
</Dialog.Root>
