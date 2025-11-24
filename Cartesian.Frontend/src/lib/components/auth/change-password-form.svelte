<script lang="ts">
	import * as Form from "$lib/components/ui/form";
	import { Input } from "$lib/components/ui/input";
	import { Button } from "$lib/components/ui/button";
	import { toast } from "svelte-sonner";
	import { superForm } from "sveltekit-superforms";
	import { zod4 } from "sveltekit-superforms/adapters";
	import { z } from "zod";
	import { putAccountApiMePassword } from "$lib/api";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { Loading03Icon, CheckmarkCircle02Icon } from "@hugeicons/core-free-icons";

	let errorMessage = $state<string | null>(null);

	const changePasswordSchema = z
		.object({
			currentPassword: z.string().min(1, "Current password is required"),
			newPassword: z.string().min(8, "Password must be at least 8 characters"),
			confirmPassword: z.string().min(1, "Confirm password is required"),
		})
		.refine((data) => data.newPassword === data.confirmPassword, {
			message: "Passwords do not match",
			path: ["confirmPassword"],
		});

	const form = superForm(
		{
			currentPassword: "",
			newPassword: "",
			confirmPassword: "",
		},
		{
			SPA: true,
			validators: zod4(changePasswordSchema as any),
			onUpdate: async ({ form }) => {
				errorMessage = null;
				if (form.valid) {
					try {
						await putAccountApiMePassword({
							currentPassword: form.data.currentPassword,
							newPassword: form.data.newPassword,
						});
						toast.success("Password updated successfully");
						form.data.currentPassword = "";
						form.data.newPassword = "";
						form.data.confirmPassword = "";
					} catch (error) {
						console.error(error);
						errorMessage =
							error instanceof Error ? error.message : "Failed to update password";
					}
				}
			},
		},
	);

	const { form: formData, enhance, submitting } = form;
</script>

<form method="POST" use:enhance class="space-y-4">
	<Form.Field {form} name="currentPassword">
		<Form.Control>
			{#snippet children({ props })}
				<Form.Label>Current Password</Form.Label>
				<Input {...props} type="password" bind:value={$formData.currentPassword} />
			{/snippet}
		</Form.Control>
		<Form.FieldErrors />
	</Form.Field>

	<div class="grid gap-4 sm:grid-cols-2">
		<Form.Field {form} name="newPassword">
			<Form.Control>
				{#snippet children({ props })}
					<Form.Label>New Password</Form.Label>
					<Input {...props} type="password" bind:value={$formData.newPassword} />
				{/snippet}
			</Form.Control>
			<Form.FieldErrors />
		</Form.Field>

		<Form.Field {form} name="confirmPassword">
			<Form.Control>
				{#snippet children({ props })}
					<Form.Label>Confirm Password</Form.Label>
					<Input {...props} type="password" bind:value={$formData.confirmPassword} />
				{/snippet}
			</Form.Control>
			<Form.FieldErrors />
		</Form.Field>
	</div>

	{#if errorMessage}
		<div class="text-sm text-destructive">{errorMessage}</div>
	{/if}

	<div class="flex justify-end pt-2">
		<Button
			type="submit"
			disabled={$submitting}
			class="rounded-xl bg-primary font-medium shadow-lg shadow-primary/20 transition-all hover:shadow-primary/30 active:scale-[0.98]"
		>
			{#if $submitting}
				<HugeiconsIcon icon={Loading03Icon} className="mr-2 size-4 animate-spin" />
				Updating...
			{:else}
				<HugeiconsIcon icon={CheckmarkCircle02Icon} className="mr-2 size-4" />
				Update Password
			{/if}
		</Button>
	</div>
</form>
