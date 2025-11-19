<script lang="ts">
	import { goto } from "$app/navigation";
	import { query } from "$app/server";
	import {
		createPostAccountApiRegister,
		type CartesianIdentityError,
		type RegisterBody,
		type RegisterSuccess,
	} from "$lib/api";
	import { type CreateMutationResult } from "@tanstack/svelte-query";
	import { onMount } from "svelte";
	import { schema, type RegisterSchema } from "./schema";

	let formData: RegisterSchema = {
		username: "",
		email: "",
		password: "",
		confirmPassword: "",
	};

	let errors: Partial<Record<keyof RegisterSchema, string>> = {};

	let mutation: CreateMutationResult<
		RegisterSuccess,
		CartesianIdentityError,
		{
			data: RegisterBody;
		},
		unknown
	>;

	onMount(() => {
		mutation = createPostAccountApiRegister({
			mutation: {
				onSuccess: (data) => {
					console.log("success: ", data.me);
					goto("/login");
				},
				onError: (error) => {
					console.error("Registration error:", error);
					if (error.identityErrors) {
						// Display identity validation errors
						const errorMessages = Object.entries(error.identityErrors)
							.map(([key, value]) => `${key}: ${JSON.stringify(value)}`)
							.join("\n");
						alert(`Registration failed:\n${errorMessages}`);
					} else {
						alert(`Error creating user: ${error.message}`);
					}
				},
			},
		});
	});

	function handleSubmit(event: SubmitEvent) {
		errors = {};
		const result = schema.safeParse(formData);
		if (!result.success) {
			result.error.issues.forEach((issue) => {
				const path = issue.path[0] as keyof RegisterSchema;
				errors[path] = issue.message;
			});
			return;
		}

		mutation.mutate({
			data: {
				username: formData.username,
				email: formData.email,
				password: formData.password,
			},
		});
	}
</script>

<div class="flex flex-col gap-4">
	<h1>Register</h1>
	<form action="" on:submit|preventDefault={handleSubmit} class="flex flex-col gap-4">
		<label for="username">Username</label>
		<input type="text" bind:value={formData.username} id="username" name="username" />
		{#if errors.username}
			<p class="text-red-500">{errors.username}</p>
		{/if}

		<label for="email">Email</label>
		<input type="email" bind:value={formData.email} id="email" name="email" />
		{#if errors.email}
			<p class="text-red-500">{errors.email}</p>
		{/if}
		<label for="password">Password</label>
		<input type="password" bind:value={formData.password} id="password" name="password" />
		{#if errors.password}
			<p class="text-red-500">{errors.password}</p>
		{/if}

		<label for="confirmPassword">Confirm password</label>
		<input
			type="password"
			bind:value={formData.confirmPassword}
			id="confirmPassword"
			name="confirmPassword"
		/>
		{#if errors.confirmPassword}
			<p class="text-red-500">{errors.confirmPassword}</p>
		{/if}

		<button type="submit">Register</button>
	</form>
</div>
