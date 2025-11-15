export const lightPreset = {
	light: {
		middle: "day",
		endings: "dawn",
	},
	dark: {
		middle: "night",
		endings: "dusk",
	},
} as const;

export const getLightingPreset = (themeMode: "light" | "dark"): string => {
	const hour = new Date().getHours();

	// oxlint-disable-next-line const-comparisons
	const isMiddleOfDay = (hour >= 9 && hour < 17) || (hour >= 23 && hour < 5);

	if (themeMode === "dark") {
		return isMiddleOfDay ? lightPreset.dark.middle : lightPreset.dark.endings;
	}

	return isMiddleOfDay ? lightPreset.light.middle : lightPreset.light.endings;
};
