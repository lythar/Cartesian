export let userProfileState = $state<{
	open: boolean;
	userId: string | null;
}>({
	open: false,
	userId: null,
});

export function openUserProfile(userId: string) {
	userProfileState.userId = userId;
	userProfileState.open = true;
}

export function closeUserProfile() {
	userProfileState.open = false;
	userProfileState.userId = null;
}
