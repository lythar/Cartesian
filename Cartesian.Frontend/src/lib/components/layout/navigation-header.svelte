<script lang="ts">
	import * as Sidebar from "$lib/components/ui/sidebar";
	import { cn } from "$lib/utils";
</script>

<Sidebar.Header>
	<Sidebar.Menu>
		<Sidebar.MenuItem>
			<Sidebar.MenuButton size="lg">
				{#snippet child({ props })}
					<a href="/" {...props}>
						<img src="/lythar.svg" alt="Lythar Logo" class="size-8 shrink-0" />
						<span class="grid flex-1 text-left text-base tracking-wide uppercase">
							Lythar
						</span>
					</a>
				{/snippet}
			</Sidebar.MenuButton>
		</Sidebar.MenuItem>
	</Sidebar.Menu>
</Sidebar.Header>
