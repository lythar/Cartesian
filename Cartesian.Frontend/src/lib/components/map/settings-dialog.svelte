<script lang="ts">
	import * as Dialog from "$lib/components/ui/dialog";
	import { Button } from "$lib/components/ui/button";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import {
		Cancel01Icon,
		Sun02Icon,
		Moon02Icon,
		ComputerIcon,
		LanguageCircleIcon,
	} from "@hugeicons/core-free-icons";
	import { mode, setMode } from "mode-watcher";
	import { getLocale, setLocale, locales } from "$lib/paraglide/runtime";
	import { page } from "$app/stores";
	import { pushState } from "$app/navigation";
	import { browser } from "$app/environment";

	let { open = $bindable(false) } = $props();

	let previousState = $state(!!$page.state.settingsDialog);

	$effect(() => {
		const currentState = !!$page.state.settingsDialog;

		if (open) {
			if (!currentState) {
				if (previousState) {
					open = false;
				} else {
					if (browser) {
						pushState("", { settingsDialog: true });
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

	const currentLocale = $derived(getLocale());
	const currentMode = $derived(mode.current);

	const languageLabels: Record<string, {
    name: string;
    icon: string
  }> = {
		en: { name: "English", icon: "🇺🇲" },
		pl: { name: "Polski", icon: "🇵🇱" },
	};

	function handleLanguageChange(locale: string) {
		setLocale(locale as "en" | "pl");
	}
</script>

<Dialog.Root bind:open>
	<Dialog.Content class="max-w-md">
		<Dialog.Header>
			<Dialog.Title class="text-xl font-semibold">Settings</Dialog.Title>
			<Dialog.Description class="text-muted-foreground">
				Customize your experience
			</Dialog.Description>
		</Dialog.Header>

		<div class="space-y-6 py-4">
			<div class="space-y-3">
				<h3 class="text-sm font-medium text-foreground">Theme</h3>
				<div class="flex gap-2">
					<Button
						variant={currentMode === "light" ? "default" : "outline"}
						size="sm"
						class="flex-1 gap-2"
						onclick={() => setMode("light")}
					>
						<HugeiconsIcon icon={Sun02Icon} size={16} />
						Light
					</Button>
					<Button
						variant={currentMode === "dark" ? "default" : "outline"}
						size="sm"
						class="flex-1 gap-2"
						onclick={() => setMode("dark")}
					>
						<HugeiconsIcon icon={Moon02Icon} size={16} />
						Dark
					</Button>
					<Button
						variant={currentMode === undefined ? "default" : "outline"}
						size="sm"
						class="flex-1 gap-2"
						onclick={() => setMode("system")}
					>
						<HugeiconsIcon icon={ComputerIcon} size={16} />
						System
					</Button>
				</div>
			</div>

			<div class="space-y-3">
				<h3 class="text-sm font-medium text-foreground">Language</h3>
				<div class="flex gap-2">
					{#each locales as locale}
						<Button
							variant={currentLocale === locale ? "default" : "outline"}
							size="sm"
							class="flex-1 gap-2"
							onclick={() => handleLanguageChange(locale)}
						>
              {languageLabels[locale]?.icon} {languageLabels[locale]?.name ?? locale}
						</Button>
					{/each}
				</div>
			</div>
		</div>

		<Dialog.Footer>
			<Dialog.Close>
				<Button variant="outline" class="gap-2">
					<HugeiconsIcon icon={Cancel01Icon} size={16} />
					Close
				</Button>
			</Dialog.Close>
		</Dialog.Footer>
	</Dialog.Content>
</Dialog.Root>
