<script lang="ts">
	import { Button } from "$lib/components/ui/button";
	import * as DropdownMenu from "$lib/components/ui/dropdown-menu";
	import * as Avatar from "$lib/components/ui/avatar";
	import { LogOut } from "@lucide/svelte";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { Bookmark02Icon, Settings01Icon, UserIcon } from "@hugeicons/core-free-icons";
	import { cn } from "$lib/utils";
	import { authStore } from "$lib/stores/auth.svelte";
	import { goto } from "$app/navigation";

	const { class: className } = $props();

	const auth = $derived($authStore);

	async function handleLogout() {
		authStore.clearUser();
		goto("/login");
	}

	function getInitials(name: string): string {
		return name
			.split(" ")
			.map((n) => n[0])
			.join("")
			.toUpperCase()
			.slice(0, 2);
	}

	function getAvatarUrl(avatar: { bucketName: string; objectKey: string } | null): string | null {
		if (!avatar) return null;
		return `/${avatar.bucketName}/${avatar.objectKey}`;
	}
</script>

{#if !auth.isAuthenticated}
	<div class={cn("z-20", className)}>
		<div class="flex gap-2">
			<Button variant="ghost" size="sm" href="/login">Sign in</Button>
			<Button size="sm" href="/register">Sign up</Button>
		</div>
	</div>
{:else if auth.user}
	<DropdownMenu.Root>
		<div class={cn("z-20", className)}>
			<DropdownMenu.Trigger class="size-12">
				<Button
					variant="ghost"
					size="icon"
					class="size-12 rounded-full backdrop-blur-sm shadow-xl transition-all duration-300"
				>
					<Avatar.Root class="size-12">
						<Avatar.Image src={getAvatarUrl(auth.user.avatar)} alt={auth.user.name} />
						<Avatar.Fallback class="bg-primary text-primary-foreground text-lg font-semibold">
							{getInitials(auth.user.name)}
						</Avatar.Fallback>
					</Avatar.Root>
				</Button>
			</DropdownMenu.Trigger>
		</div>
		<DropdownMenu.Content
			class="w-72 backdrop-blur-sm shadow-2xl border border-border/50 pb-3"
			align="end"
			sideOffset={12}
		>
			<div class="px-3 py-4 mb-2">
				<div class="flex items-center gap-3">
					<Avatar.Root class="size-12">
						<Avatar.Image src={getAvatarUrl(auth.user.avatar)} alt={auth.user.name} />
						<Avatar.Fallback class="bg-primary text-primary-foreground text-lg font-semibold">
							{getInitials(auth.user.name)}
						</Avatar.Fallback>
					</Avatar.Root>
					<div class="flex-1 min-w-0">
						<p class="font-semibold text-base truncate">{auth.user.name}</p>
						<p class="text-sm text-muted-foreground truncate">{auth.user.email}</p>
					</div>
				</div>
			</div>

			<DropdownMenu.Group class="[&>div]:py-2 px-2">
				<DropdownMenu.Item class="gap-3 cursor-pointer">
					<HugeiconsIcon icon={UserIcon} className="size-5 duotone-fill" />
					<span class="font-medium">Profile</span>
				</DropdownMenu.Item>
				<DropdownMenu.Item class="gap-3 cursor-pointer">
					<HugeiconsIcon icon={Bookmark02Icon} className="size-5 duotone-fill" />
					<span class="font-medium">Saved Events</span>
				</DropdownMenu.Item>
				<DropdownMenu.Item class="gap-3 cursor-pointer">
					<HugeiconsIcon icon={Settings01Icon} className="size-5 duotone-fill" />
					<span class="font-medium">Settings</span>
				</DropdownMenu.Item>
			</DropdownMenu.Group>

			<DropdownMenu.Separator class="my-2" />

			<div class="px-2">
				<DropdownMenu.Item class="gap-3 cursor-pointer" onclick={handleLogout}>
					<LogOut class="size-5" />
					<span class="font-medium">Logout</span>
				</DropdownMenu.Item>
			</div>
		</DropdownMenu.Content>
	</DropdownMenu.Root>
{/if}
