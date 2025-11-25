<script lang="ts">
	import * as Form from "$lib/components/ui/form";
	import { Input } from "$lib/components/ui/input";
	import { loginSchema } from "./schema";
	import type { PageData } from "./$types";
	import { superForm } from "sveltekit-superforms";
	import { zod4Client } from "sveltekit-superforms/adapters";
	import {
		createPostAccountApiLogin,
		getAccountApiMe,
		type InvalidCredentialsError,
	} from "$lib/api";
	import { goto } from "$app/navigation";
	import { authStore } from "$lib/stores/auth.svelte";

	let { data }: { data: PageData } = $props();

	console.log("Login Page Mounted, data.form:", data?.form);

	let errorMessage = $state<string | null>(null);

	const loginMutation = createPostAccountApiLogin({
		mutation: {
			onSuccess: async () => {
				console.log("Login successful, fetching user data");
				try {
					const userData = await getAccountApiMe();
					authStore.setUser(userData);
					errorMessage = null;
					goto("/app");
				} catch (error) {
					console.error("Failed to fetch user data:", error);
					errorMessage = "Login successful but failed to fetch user data";
				}
			},
			onError: (error: InvalidCredentialsError) => {
				console.error("Login failed:", error);
				errorMessage = error.message || "Invalid email or password";
			},
		},
	});

	const form = superForm(data.form, {
		validators: zod4Client(loginSchema),
		onSubmit: async ({ formData }) => {
			errorMessage = null;
			const email = formData.get("email") as string;
			const password = formData.get("password") as string;

			loginMutation.mutate({
				data: { email, password },
			});
		},
		SPA: true,
	});

	const { form: formData, enhance } = form;
</script>

<div class="flex min-h-screen items-center justify-center bg-background p-4">
	<div class="w-full max-w-md space-y-6">
		<div class="space-y-2 text-center">
			<h1 class="text-3xl font-semibold tracking-tight">Sign in</h1>
			<p class="text-sm text-muted-foreground">Enter your credentials to continue</p>
		</div>

		<form use:enhance class="space-y-4">
			{#if errorMessage}
				<div
					class="rounded-md border border-destructive/50 bg-destructive/10 px-4 py-3 text-sm text-destructive"
				>
					{errorMessage}
				</div>
			{/if}

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

			<Form.Button type="submit" class="w-full" disabled={loginMutation.isPending}>
				{loginMutation.isPending ? "Signing in..." : "Sign in"}
			</Form.Button>
		</form>

		<div class="text-center text-sm text-muted-foreground">
			Don't have an account?
			<a href="/register" class="font-medium text-primary underline-offset-4 hover:underline">
				Sign up
			</a>
		</div>
	</div>
</div>
