<script lang="ts">
	import { getLayoutContext } from "$lib/context/layout.svelte";
  import * as Sidebar from "$lib/components/ui/sidebar";
	import { Home07Icon, MapsLocation02Icon, Settings01Icon } from "@hugeicons/core-free-icons";
  import { m } from "$lib/paraglide/messages";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { cn } from "$lib/utils";

  const navigationElements = [
    { name: "nav_home", href: "/app/home", icon: Home07Icon },
    { name: "nav_map", href: "/app/map", icon: MapsLocation02Icon },
    { name: "nav_settings", href: "/app/settings", icon: Settings01Icon },
  ]

  const layout = getLayoutContext();

  // const sidebar = Sidebar.useSidebar();

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
    variant="floating"
    collapsible="icon"
    class="border-none"
  >
    <Sidebar.Header class={"flex flex-row items-center gap-4"}>
      <img src="/lythar.svg" alt="Lythar Logo" class="size-8" />
      <span class={cn("text-xl tracking-widest")}>Lythar</span>
    </Sidebar.Header>
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
  <main class="flex flex-1 relative flex-col overflow-hidden">
    <div class="absolute top-4 left-2 z-10">
      <Sidebar.Trigger />
    </div>
    {@render children?.()}
  </main>
</Sidebar.SidebarProvider>
