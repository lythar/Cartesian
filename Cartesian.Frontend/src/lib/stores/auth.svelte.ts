import { writable } from "svelte/store";
import type { MyUserDto } from "$lib/api";

interface AuthState {
	user: MyUserDto | null;
	isAuthenticated: boolean;
	isLoading: boolean;
}

function createAuthStore() {
	const { subscribe, set, update } = writable<AuthState>({
		user: null,
		isAuthenticated: false,
		isLoading: true,
	});

	return {
		subscribe,
		setUser: (user: MyUserDto) => {
			update((state) => ({
				...state,
				user,
				isAuthenticated: true,
				isLoading: false,
			}));
		},
		clearUser: () => {
			set({
				user: null,
				isAuthenticated: false,
				isLoading: false,
			});
		},
		setLoading: (isLoading: boolean) => {
			update((state) => ({ ...state, isLoading }));
		},
	};
}

export const authStore = createAuthStore();
