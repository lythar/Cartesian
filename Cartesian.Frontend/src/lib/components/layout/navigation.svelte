<script lang="ts">
	import { getLayoutContext } from "$lib/context/layout.svelte";
	import { createPaneContext, getPaneContext } from "$lib/context/pane.svelte";
  import * as Sidebar from "$lib/components/ui/sidebar";
	import { Home07Icon, MapsLocation02Icon, Settings01Icon } from "@hugeicons/core-free-icons";
  import { m } from "$lib/paraglide/messages";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import NavigationHeader from "./navigation-header.svelte";
	import MobileNavigation from "./mobile-navigation.svelte";
	import HomePane from "$lib/components/panes/home-pane.svelte";
	import MapPane from "$lib/components/panes/map-pane.svelte";
	import SettingsPane from "$lib/components/panes/settings-pane.svelte";
	import { onMount } from "svelte";

  const navigationElements = [
    { name: "nav_home", id: "home", icon: Home07Icon, side: "left" as const, component: HomePane },
    { name: "nav_map", id: "map", icon: MapsLocation02Icon, side: "left" as const, component: MapPane },
    { name: "nav_settings", id: "settings", icon: Settings01Icon, side: "right" as const, component: SettingsPane },
  ]

  const layout = getLayoutContext();
	const paneManager = createPaneContext();

  const { children } = $props();

	function getTranslation(key: string) {
		return m[key as keyof typeof m]();
	}

	function isActive(id: string) {
		return paneManager.activeLeftPane === id || paneManager.activeRightPane === id;
	}

	function handleNavigation(id: string) {
		paneManager.activatePane(id);
	}

	onMount(() => {
		navigationElements.forEach(element => {
			paneManager.registerPane({
				id: element.id,
				component: element.component,
				side: element.side,
			});
		});
	});
</script>

{#snippet MainAppNavigation()}
  {#each navigationElements as element (element.id)}
    <Sidebar.MenuItem>
      <Sidebar.MenuButton isActive={isActive(element.id)}>
        {#snippet child({props})}
          <button type="button" onclick={() => handleNavigation(element.id)} {...props}>
            <HugeiconsIcon icon={element.icon} strokeWidth={2} className="fill-current/20" />
            <span>{m[element.name as keyof typeof m]()}</span>
          </button>
        {/snippet}
      </Sidebar.MenuButton>
    </Sidebar.MenuItem>
  {/each}
{/snippet}

<Sidebar.SidebarProvider>
  <Sidebar.Root
    variant="inset"
    collapsible="icon"
    class="border-none"
  >
    <NavigationHeader />
    <Sidebar.Content>
      <Sidebar.Group>
        <Sidebar.GroupLabel>{m.nav_group_main_app()}</Sidebar.GroupLabel>
        <Sidebar.GroupContent>
          <Sidebar.Menu>
            {@render MainAppNavigation()}
          </Sidebar.Menu>
        </Sidebar.GroupContent>
      </Sidebar.Group>
    </Sidebar.Content>
  </Sidebar.Root>
  <Sidebar.Inset>
    <main class="flex flex-1 relative flex-col overflow-hidden {layout.isMobile ? 'pb-[72px]' : ''}">
      {#if !layout.isMobile}
        <div class="absolute left-2 top-2 z-10">
          <Sidebar.Trigger />
        </div>
      {/if}
      {@render children?.()}
    </main>
  </Sidebar.Inset>
  {#if layout.isMobile}
    <MobileNavigation {navigationElements} translationFunction={getTranslation} />
  {/if}
</Sidebar.SidebarProvider>
