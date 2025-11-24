<script lang="ts">
	import * as AlertDialog from "$lib/components/ui/alert-dialog";
	import { Button } from "$lib/components/ui/button";
	import { goto } from "$app/navigation";

	let {
		open = $bindable(false),
		title = "Account Required",
		description = "You need to sign up before you can create an event."
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
			<AlertDialog.Cancel>Cancel</AlertDialog.Cancel>
			<Button
				variant="outline"
				onclick={() => {
					open = false;
					goto("/login");
				}}
			>
				Log In
			</Button>
			<Button
				onclick={() => {
					open = false;
					goto("/register");
				}}
			>
				Sign Up
			</Button>
		</AlertDialog.Footer>
	</AlertDialog.Content>
</AlertDialog.Root>
