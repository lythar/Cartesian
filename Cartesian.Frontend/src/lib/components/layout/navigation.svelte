<script lang="ts">
	import { getLayoutContext } from "$lib/context/layout.svelte";
  import * as Sidebar from "$lib/components/ui/sidebar";
	import { Home07Icon, MapsLocation02Icon, Settings01Icon } from "@hugeicons/core-free-icons";
  import { m } from "$lib/paraglide/messages";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import NavigationHeader from "./navigation-header.svelte";
	import MobileNavigation from "./mobile-navigation.svelte";
	import { page } from "$app/stores";

  const navigationElements = [
    { name: "nav_home", href: "/app/home", icon: Home07Icon },
    { name: "nav_map", href: "/app/map", icon: MapsLocation02Icon },
    { name: "nav_settings", href: "/app/settings", icon: Settings01Icon },
  ]

  const layout = getLayoutContext();

  const { children } = $props();

	function getTranslation(key: string) {
		return m[key as keyof typeof m]();
	}

	function isActive(href: string) {
		return $page.url.pathname === href;
	}
</script>

{#snippet MainAppNavigation()}
  {#each navigationElements as element (element.href)}
    <Sidebar.MenuItem>
      <Sidebar.MenuButton isActive={isActive(element.href)}>
        {#snippet child({props})}
          <a href={element.href} {...props}>
            <HugeiconsIcon icon={element.icon} strokeWidth={2} className="fill-current/20" />
            <span>{m[element.name as keyof typeof m]()}</span>
          </a>
        {/snippet}
      </Sidebar.MenuButton>
    </Sidebar.MenuItem>
  {/each}
{/snippet}

<Sidebar.SidebarProvider
    open={false}
>
  <Sidebar.Root
    variant="inset"
    collapsible="offcanvas"
    class="border-none"

  >
    <!-- <NavigationHeader /> -->
    <!-- <Sidebar.Content>
      {#if layout.isMobile}

      {:else}
        <Sidebar.Group>
          <Sidebar.GroupLabel>{m.nav_group_main_app()}</Sidebar.GroupLabel>
          <Sidebar.GroupContent>
            <Sidebar.Menu>
              {@render MainAppNavigation()}
            </Sidebar.Menu>
          </Sidebar.GroupContent>
        </Sidebar.Group>
      {/if}
    </Sidebar.Content> -->
  </Sidebar.Root>
  <Sidebar.Inset>
    <main class="flex flex-1 relative flex-col overflow-hidden {layout.isMobile ? 'pb-[72px]' : ''}">
      {#if !layout.isMobile}
        <!-- <div class="absolute left-2 top-2 z-10">
          <Sidebar.Trigger />
        </div> -->
      {/if}
      {@render children?.()}
    </main>
  </Sidebar.Inset>
  {#if layout.isMobile}
    <MobileNavigation {navigationElements} translationFunction={getTranslation} />
  {/if}
</Sidebar.SidebarProvider>
