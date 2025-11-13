<script lang="ts">
	import { getLayoutContext } from "$lib/context/layout.svelte";
  import * as Sidebar from "$lib/components/ui/sidebar";
	import { Home07Icon, MapsLocation02Icon, Settings01Icon } from "@hugeicons/core-free-icons";
  import { m } from "$lib/paraglide/messages";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import NavigationHeader from "./navigation-header.svelte";

  const navigationElements = [
    { name: "nav_home", href: "/app/home", icon: Home07Icon },
    { name: "nav_map", href: "/app/map", icon: MapsLocation02Icon },
    { name: "nav_settings", href: "/app/settings", icon: Settings01Icon },
  ]

  const layout = getLayoutContext();

  const { children } = $props();
</script>

{#snippet MainAppNavigation()}
  {#each navigationElements as element (element.href)}
    <Sidebar.MenuItem>
      <Sidebar.MenuButton>
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

<Sidebar.SidebarProvider>
  <Sidebar.Root
    variant="inset"
    collapsible="icon"
    class="border-none"
  >
    <NavigationHeader />
    <Sidebar.Content>
      {#if layout.isMobile}
        <div>

        </div>
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
    </Sidebar.Content>
  </Sidebar.Root>
  <Sidebar.Inset>
    <main class="flex flex-1 relative flex-col overflow-hidden">
      <div class="absolute left-2 top-2 z-10">
        <Sidebar.Trigger />
      </div>
      {@render children?.()}
    </main>
  </Sidebar.Inset>
</Sidebar.SidebarProvider>
