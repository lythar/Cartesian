import { browser } from "$app/environment";
import { generateColorScheme, generateCSSVariables, type ShadcnColorScheme } from "./material-gen";
import { mode } from "mode-watcher";
import { PersistedState } from "runed";
import { untrack } from "svelte";

interface ThemeState {
	sourceColor: string;
	light: ShadcnColorScheme;
	dark: ShadcnColorScheme;
}

class ThemeApplier {
	private state = $state<ThemeState | null>(null);
	sourceColor = new PersistedState<string>("theme-source-color", "#009FE7");
	private initialized = false;

	initialize() {
		if (this.initialized || !browser) return;
		this.initialized = true;

		$effect(() => {
			const color = this.sourceColor.current;
			const currentMode = mode.current;

			const schemes = generateColorScheme(color);

			untrack(() => {
				this.state = {
					sourceColor: color,
					light: schemes.light,
					dark: schemes.dark,
				};
			});

			if (browser) {
				const isDark = currentMode === "dark";
				const scheme = isDark ? schemes.dark : schemes.light;
				const cssVars = generateCSSVariables(scheme);

				const root = document.documentElement;
				Object.entries(cssVars).forEach(([key, value]) => {
					root.style.setProperty(key, value);
				});
			}
		});
	}

	get currentTheme() {
		return this.state;
	}

	setSourceColor(color: string) {
		this.sourceColor.current = color;
	}

	getScheme(mode: "light" | "dark"): ShadcnColorScheme | null {
		if (!this.state) return null;
		return mode === "dark" ? this.state.dark : this.state.light;
	}

	getCSSVariables(mode: "light" | "dark"): Record<string, string> | null {
		const scheme = this.getScheme(mode);
		if (!scheme) return null;
		return generateCSSVariables(scheme);
	}
}

export const themeApplier = new ThemeApplier();
