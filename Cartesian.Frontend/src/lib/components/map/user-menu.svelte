<script lang="ts">
	import { Button } from "$lib/components/ui/button";
	import * as DropdownMenu from "$lib/components/ui/dropdown-menu";
	import * as Avatar from "$lib/components/ui/avatar";
	import { LogOut } from "@lucide/svelte";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import {
		Bookmark02Icon,
		Settings01Icon,
		UserIcon,
		Calendar03Icon,
	} from "@hugeicons/core-free-icons";
	import { cn, getAvatarUrl, getInitials } from "$lib/utils";
	import { authStore } from "$lib/stores/auth.svelte";
	import { goto } from "$app/navigation";
	import ProfileDialog from "./profile-dialog.svelte";
	import EventsDialog from "./events-dialog.svelte";
	import SettingsDialog from "./settings-dialog.svelte";
	import { baseUrl } from "$lib/api/client";
	import { useQueryClient } from "@tanstack/svelte-query";
	import {
		mapInteractionState,
		newEventOverlayState,
		editEventOverlayState,
	} from "./map-state.svelte";
	import { getLayoutContext } from "$lib/context/layout.svelte";
	import LoginAlertDialog from "../auth/login-alert-dialog.svelte";
	import * as m from "$lib/paraglide/messages";

	const { class: className } = $props();

	const queryClient = useQueryClient();
	const auth = $derived($authStore);
	let openProfile = $state(false);
	let openEvents = $state(false);
	let openSettings = $state(false);
	let openLoginAlert = $state(false);

	const layout = getLayoutContext();

	async function handleLogout() {
		try {
			await fetch("/account/api/logout", {
				method: "POST",
				credentials: "include",
			});
		} catch (e) {
			console.error("Logout request failed:", e);
		}

		document.cookie.split(";").forEach((c) => {
			const name = c.trim().split("=")[0];
			document.cookie = `${name}=;expires=Thu, 01 Jan 1970 00:00:00 GMT;path=/`;
		});

		queryClient.clear();

		mapInteractionState.selectedEvent = null;
		mapInteractionState.eventDetailsOpen = false;
		newEventOverlayState.open = false;
		newEventOverlayState.location = null;
		editEventOverlayState.open = false;
		editEventOverlayState.eventId = null;
		editEventOverlayState.location = null;

		authStore.clearUser();
		goto("/login");
	}
</script>

{#if !auth.isAuthenticated}
	{#if layout.isMobile}
		<div class={cn("z-20", className)}>
			<LoginAlertDialog
				bind:open={openLoginAlert}
				description={m.account_required_description()}
				title={m.account_required()}
			/>
			<Button
				variant="secondary"
				size="icon-lg"
				onclick={() => {
					openLoginAlert = true;
				}}
				class="size-12 rounded-full bg-secondary/30 text-foreground"
			>
				<HugeiconsIcon icon={UserIcon} size={20} className="duotone-fill size-5" />
			</Button>
		</div>
	{:else}
		<div class={cn("z-20", className)}>
			<div class="flex gap-2">
				<Button variant="ghost" size="sm" href="/login">{m.sign_in()}</Button>
				<Button size="sm" href="/register">{m.sign_up()}</Button>
			</div>
		</div>
	{/if}
{:else if auth.user}
	<DropdownMenu.Root>
		<div class={cn("z-20", className)}>
			<DropdownMenu.Trigger class="size-12">
				<Button
					variant="ghost"
					size="icon"
					class="size-12 rounded-full shadow-xl backdrop-blur-sm transition-all duration-300"
				>
					<Avatar.Root class="size-12">
						<Avatar.Image src={getAvatarUrl(auth.user.avatar)} alt={auth.user.name} />
						<Avatar.Fallback
							class="bg-primary text-lg font-semibold text-primary-foreground"
						>
							{getInitials(auth.user.name)}
						</Avatar.Fallback>
					</Avatar.Root>
				</Button>
			</DropdownMenu.Trigger>
		</div>
		<DropdownMenu.Content
			class="w-72 border border-border/50 pb-3 shadow-2xl backdrop-blur-sm"
			align="end"
			sideOffset={12}
		>
			<div class="mb-2 px-3 py-4">
				<div class="flex items-center gap-3">
					<Avatar.Root class="size-12">
						<Avatar.Image
							src={getAvatarUrl({ id: auth.user.avatar?.id! })}
							alt={auth.user.name}
						/>
						<Avatar.Fallback
							class="bg-primary text-lg font-semibold text-primary-foreground"
						>
							{getInitials(auth.user.name)}
						</Avatar.Fallback>
					</Avatar.Root>
					<div class="min-w-0 flex-1">
						<p class="truncate text-base font-semibold">{auth.user.name}</p>
						<p class="truncate text-sm text-muted-foreground">{auth.user.email}</p>
					</div>
				</div>
			</div>

			<DropdownMenu.Group class="px-2 [&>div]:py-2">
				<DropdownMenu.Item
					class="cursor-pointer gap-3"
					onclick={() => (openProfile = true)}
				>
					<HugeiconsIcon icon={UserIcon} className="size-5 duotone-fill" />
					<span class="font-medium">{m.profile()}</span>
				</DropdownMenu.Item>
				<DropdownMenu.Item class="cursor-pointer gap-3" onclick={() => (openEvents = true)}>
					<HugeiconsIcon icon={Calendar03Icon} className="size-5 duotone-fill" />
					<span class="font-medium">{m.events()}</span>
				</DropdownMenu.Item>
				<DropdownMenu.Item
					class="cursor-pointer gap-3"
					onclick={() => (openSettings = true)}
				>
					<HugeiconsIcon icon={Settings01Icon} className="size-5 duotone-fill" />
					<span class="font-medium">{m.settings()}</span>
				</DropdownMenu.Item>
			</DropdownMenu.Group>

			<DropdownMenu.Separator class="my-2" />

			<div class="px-2">
				<DropdownMenu.Item class="cursor-pointer gap-3" onclick={handleLogout}>
					<LogOut class="size-5" />
					<span class="font-medium">{m.logout()}</span>
				</DropdownMenu.Item>
			</div>
		</DropdownMenu.Content>
	</DropdownMenu.Root>

	<ProfileDialog bind:open={openProfile} />
	<EventsDialog bind:open={openEvents} />
	<SettingsDialog bind:open={openSettings} />
{/if}
