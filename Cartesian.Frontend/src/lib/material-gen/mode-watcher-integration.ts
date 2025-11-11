import { generateColorScheme, generateCSSVariables } from "./material-gen";
import { mode } from "mode-watcher";

interface ApplyThemeOptions {
	sourceColor: string;
}

export function applyThemeFromColor({ sourceColor }: ApplyThemeOptions) {
	const schemes = generateColorScheme(sourceColor);
	const currentMode = mode.current;

	const isDark = currentMode === "dark";

	const scheme = isDark ? schemes.dark : schemes.light;
	const cssVars = generateCSSVariables(scheme);

	if (typeof document !== "undefined") {
		const root = document.documentElement;
		Object.entries(cssVars).forEach(([key, value]) => {
			root.style.setProperty(key, value);
		});
	}

	return { light: schemes.light, dark: schemes.dark };
}

export function watchAndApplyTheme(sourceColor: string) {
	const schemes = generateColorScheme(sourceColor);

	const applyScheme = () => {
		const currentMode = mode.current;
		const isDark = currentMode === "dark";

		const scheme = isDark ? schemes.dark : schemes.light;
		const cssVars = generateCSSVariables(scheme);

		if (typeof document !== "undefined") {
			const root = document.documentElement;
			Object.entries(cssVars).forEach(([key, value]) => {
				console.log("applying", key, value);
				root.style.setProperty(key, value);
			});
		}
	};

	return {
		schemes,
		apply: applyScheme,
	};
}
