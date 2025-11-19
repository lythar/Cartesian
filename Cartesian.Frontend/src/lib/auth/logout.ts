import { authStore } from "$lib/stores/auth.svelte";
import { goto } from "$app/navigation";

export async function logout() {
	try {
		const response = await fetch("/account/api/logout", {
			method: "POST",
			credentials: "include",
		});

		if (response.ok) {
			authStore.clearUser();
			goto("/login");
		}
	} catch (error) {
		console.error("Logout failed:", error);
	}
}
