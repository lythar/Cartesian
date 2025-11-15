<script lang="ts">
	import { Button } from "$lib/components/ui/button";
	import * as DropdownMenu from "$lib/components/ui/dropdown-menu";
	import * as Avatar from "$lib/components/ui/avatar";
	import { User, Settings, Bookmark, LogOut } from "@lucide/svelte";
</script>

<div class="absolute top-6 right-6 z-20">
	<DropdownMenu.Root>
		<DropdownMenu.Trigger>
			<Button
				variant="ghost"
				size="icon"
				class="size-16 rounded-full bg-sidebar/95 backdrop-blur-sm shadow-xl border border-border/50 hover:bg-accent/50 transition-all duration-300"
			>
				<Avatar.Root class="size-12">
					<Avatar.Image src="" alt="User" />
					<Avatar.Fallback class="bg-primary text-primary-foreground text-lg font-semibold">
						<User class="size-6" />
					</Avatar.Fallback>
				</Avatar.Root>
			</Button>
		</DropdownMenu.Trigger>
		<DropdownMenu.Content
			class="w-72 rounded-2xl bg-sidebar/95 backdrop-blur-sm shadow-2xl border border-border/50 p-2"
			align="end"
			sideOffset={12}
		>
			<div class="px-3 py-4 border-b border-border/50 mb-2">
				<div class="flex items-center gap-3">
					<Avatar.Root class="size-14">
						<Avatar.Image src="" alt="User" />
						<Avatar.Fallback class="bg-primary text-primary-foreground text-lg font-semibold">
							JD
						</Avatar.Fallback>
					</Avatar.Root>
					<div class="flex-1 min-w-0">
						<p class="font-semibold text-base truncate">John Doe</p>
						<p class="text-sm text-muted-foreground truncate">john@example.com</p>
					</div>
				</div>
			</div>

			<DropdownMenu.Group>
				<DropdownMenu.Item class="gap-3 py-3 px-3 rounded-xl cursor-pointer">
					<User class="size-5" />
					<span class="font-medium">Profile</span>
				</DropdownMenu.Item>
				<DropdownMenu.Item class="gap-3 py-3 px-3 rounded-xl cursor-pointer">
					<Bookmark class="size-5" />
					<span class="font-medium">Saved Events</span>
				</DropdownMenu.Item>
				<DropdownMenu.Item class="gap-3 py-3 px-3 rounded-xl cursor-pointer">
					<Settings class="size-5" />
					<span class="font-medium">Settings</span>
				</DropdownMenu.Item>
			</DropdownMenu.Group>

			<DropdownMenu.Separator class="my-2" />

			<DropdownMenu.Item class="gap-3 py-3 px-3 rounded-xl cursor-pointer text-destructive focus:text-destructive">
				<LogOut class="size-5" />
				<span class="font-medium">Logout</span>
			</DropdownMenu.Item>
		</DropdownMenu.Content>
	</DropdownMenu.Root>
</div>
