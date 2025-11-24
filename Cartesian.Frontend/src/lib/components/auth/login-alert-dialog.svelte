<script lang="ts">
	import * as AlertDialog from "$lib/components/ui/alert-dialog";
	import { Button } from "$lib/components/ui/button";
	import { goto } from "$app/navigation";
	import { m } from "$lib/paraglide/messages";

	let { open = $bindable(false) }: { open: boolean } = $props();
</script>

<AlertDialog.Root bind:open>
	<AlertDialog.Content>
		<AlertDialog.Header>
			<AlertDialog.Title>{m.dialog_account_required_title()}</AlertDialog.Title>
			<AlertDialog.Description>
				{m.dialog_account_required_description()}
			</AlertDialog.Description>
		</AlertDialog.Header>
		<AlertDialog.Footer>
			<AlertDialog.Cancel>{m.common_cancel()}</AlertDialog.Cancel>
			<Button
				variant="outline"
				onclick={() => {
					open = false;
					goto("/login");
				}}
			>
				{m.dialog_account_required_login()}
			</Button>
			<Button
				onclick={() => {
					open = false;
					goto("/register");
				}}
			>
				{m.dialog_account_required_signup()}
			</Button>
		</AlertDialog.Footer>
	</AlertDialog.Content>
</AlertDialog.Root>
