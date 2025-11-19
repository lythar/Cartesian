<script lang="ts">
	import * as Form from "$lib/components/ui/form";
	import { Input } from "$lib/components/ui/input";
	import { goto } from "$app/navigation";
	import {
		createPostAccountApiRegister,
		type CartesianIdentityError,
	} from "$lib/api";
	import { superForm } from "sveltekit-superforms";
	import { zod4Client } from "sveltekit-superforms/adapters";
	import { schema } from "./schema";
	import type { PageData } from "./$types";

	let { data }: { data: PageData } = $props();

	let errorMessage = $state<string | null>(null);

	const registerMutation = createPostAccountApiRegister({
		mutation: {
			onSuccess: (data) => {
				console.log("Registration successful:", data.me);
				errorMessage = null;
				goto("/login");
			},
			onError: (error: CartesianIdentityError) => {
				console.error("Registration error:", error);
				if (error.identityErrors) {
					const errorMessages = Object.entries(error.identityErrors)
						.map(([key, value]) => `${key}: ${JSON.stringify(value)}`)
						.join(", ");
					errorMessage = errorMessages || "Registration failed";
				} else {
					errorMessage = error.message || "Registration failed";
				}
			},
		},
	});

	const form = superForm(data.form, {
		validators: zod4Client(schema),
		onSubmit: async ({ formData }) => {
			errorMessage = null;
			const username = formData.get("username") as string;
			const email = formData.get("email") as string;
			const password = formData.get("password") as string;

			registerMutation.mutate({
				data: { username, email, password },
			});
		},
		SPA: true,
	});

	const { form: formData, enhance } = form;

</script>

<div class="flex min-h-screen items-center justify-center bg-background p-4">
	<div class="w-full max-w-md space-y-6">
		<div class="space-y-2 text-center">
			<h1 class="text-3xl font-semibold tracking-tight">Create an account</h1>
			<p class="text-sm text-muted-foreground">Enter your details to get started</p>
		</div>

		<form use:enhance class="space-y-4">
			{#if errorMessage}
				<div
					class="rounded-md border border-destructive/50 bg-destructive/10 px-4 py-3 text-sm text-destructive"
				>
					{errorMessage}
				</div>
			{/if}

			<Form.Field {form} name="username">
				<Form.Control>
					{#snippet children({ props })}
						<Form.Label>Username</Form.Label>
						<Input {...props} type="text" bind:value={$formData.username} />
					{/snippet}
				</Form.Control>
				<Form.FieldErrors />
			</Form.Field>

			<Form.Field {form} name="email">
				<Form.Control>
					{#snippet children({ props })}
						<Form.Label>Email</Form.Label>
						<Input {...props} type="email" bind:value={$formData.email} />
					{/snippet}
				</Form.Control>
				<Form.FieldErrors />
			</Form.Field>

			<Form.Field {form} name="password">
				<Form.Control>
					{#snippet children({ props })}
						<Form.Label>Password</Form.Label>
						<Input {...props} type="password" bind:value={$formData.password} />
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

			<Form.Button type="submit" class="w-full" disabled={registerMutation.isPending}>
				{registerMutation.isPending ? "Creating account..." : "Create account"}
			</Form.Button>
		</form>

		<div class="text-center text-sm text-muted-foreground">
			Already have an account?
			<a href="/login" class="font-medium text-primary underline-offset-4 hover:underline">
				Sign in
			</a>
		</div>
	</div>
</div>
