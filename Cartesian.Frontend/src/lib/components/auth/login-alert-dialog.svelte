<script lang="ts">
	import * as AlertDialog from "$lib/components/ui/alert-dialog";
	import { Button } from "$lib/components/ui/button";
	import { goto } from "$app/navigation";
	import * as m from "$lib/paraglide/messages";

	let {
		open = $bindable(false),
		title = m.account_required(),
		description = m.account_required_description(),
	}: { open: boolean; title?: string; description?: string } = $props();
</script>

<AlertDialog.Root bind:open>
	<AlertDialog.Content>
		<AlertDialog.Header>
			<AlertDialog.Title>{title}</AlertDialog.Title>
			<AlertDialog.Description>
				{description}
			</AlertDialog.Description>
		</AlertDialog.Header>
		<AlertDialog.Footer>
			<AlertDialog.Cancel>{m.cancel()}</AlertDialog.Cancel>
			<Button
				variant="outline"
				onclick={() => {
					open = false;
					goto("/login");
				}}
			>
				{m.log_in()}
			</Button>
			<Button
				onclick={() => {
					open = false;
					goto("/register");
				}}
			>
				{m.sign_up()}
			</Button>
		</AlertDialog.Footer>
	</AlertDialog.Content>
</AlertDialog.Root>
