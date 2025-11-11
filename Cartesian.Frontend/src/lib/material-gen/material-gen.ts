import {
	argbFromHex,
	hexFromArgb,
	Hct,
	SchemeTonalSpot,
	MaterialDynamicColors,
} from "@material/material-color-utilities";

export interface ShadcnColorScheme {
	background: string;
	foreground: string;
	card: string;
	cardForeground: string;
	popover: string;
	popoverForeground: string;
	primary: string;
	primaryForeground: string;
	secondary: string;
	secondaryForeground: string;
	muted: string;
	mutedForeground: string;
	accent: string;
	accentForeground: string;
	destructive: string;
	destructiveForeground: string;
	border: string;
	input: string;
	ring: string;
	sidebar: string;
	sidebarForeground: string;
	sidebarPrimary: string;
	sidebarPrimaryForeground: string;
	sidebarAccent: string;
	sidebarAccentForeground: string;
	sidebarBorder: string;
	sidebarRing: string;
}

function argbToOklch(argb: number): string {
	const hex = hexFromArgb(argb);
	const r = parseInt(hex.slice(1, 3), 16) / 255;
	const g = parseInt(hex.slice(3, 5), 16) / 255;
	const b = parseInt(hex.slice(5, 7), 16) / 255;

	const toLinear = (c: number) => {
		return c <= 0.04045 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4);
	};

	const lr = toLinear(r);
	const lg = toLinear(g);
	const lb = toLinear(b);

	const l = 0.4122214708 * lr + 0.5363325363 * lg + 0.0514459929 * lb;
	const m = 0.2119034982 * lr + 0.6806995451 * lg + 0.1073969566 * lb;
	const s = 0.0883024619 * lr + 0.2817188376 * lg + 0.6299787005 * lb;

	const l_ = Math.cbrt(l);
	const m_ = Math.cbrt(m);
	const s_ = Math.cbrt(s);

	const L = 0.2104542553 * l_ + 0.793617785 * m_ - 0.0040720468 * s_;
	const a = 1.9779984951 * l_ - 2.428592205 * m_ + 0.4505937099 * s_;
	const b_lab = 0.0259040371 * l_ + 0.7827717662 * m_ - 0.808675766 * s_;

	const C = Math.sqrt(a * a + b_lab * b_lab);
	let h = (Math.atan2(b_lab, a) * 180) / Math.PI;
	if (h < 0) h += 360;

	return `oklch(${L.toFixed(3)} ${C.toFixed(3)} ${h.toFixed(3)})`;
}

function materialToShadcn(scheme: SchemeTonalSpot): ShadcnColorScheme {
	const isDark = scheme.isDark;

	return {
		background: argbToOklch(MaterialDynamicColors.background.getArgb(scheme)),
		foreground: argbToOklch(MaterialDynamicColors.onBackground.getArgb(scheme)),
		card: argbToOklch(MaterialDynamicColors.surface.getArgb(scheme)),
		cardForeground: argbToOklch(MaterialDynamicColors.onSurface.getArgb(scheme)),
		popover: argbToOklch(MaterialDynamicColors.surfaceContainer.getArgb(scheme)),
		popoverForeground: argbToOklch(MaterialDynamicColors.onSurface.getArgb(scheme)),
		primary: argbToOklch(MaterialDynamicColors.primary.getArgb(scheme)),
		primaryForeground: argbToOklch(MaterialDynamicColors.onPrimary.getArgb(scheme)),
		secondary: argbToOklch(MaterialDynamicColors.secondary.getArgb(scheme)),
		secondaryForeground: argbToOklch(MaterialDynamicColors.onSecondary.getArgb(scheme)),
		muted: argbToOklch(MaterialDynamicColors.surfaceContainerHighest.getArgb(scheme)),
		mutedForeground: argbToOklch(MaterialDynamicColors.onSurfaceVariant.getArgb(scheme)),
		accent: argbToOklch(MaterialDynamicColors.tertiary.getArgb(scheme)),
		accentForeground: argbToOklch(MaterialDynamicColors.onTertiary.getArgb(scheme)),
		destructive: argbToOklch(MaterialDynamicColors.error.getArgb(scheme)),
		destructiveForeground: argbToOklch(MaterialDynamicColors.onError.getArgb(scheme)),
		border: argbToOklch(MaterialDynamicColors.outline.getArgb(scheme)),
		input: argbToOklch(MaterialDynamicColors.outlineVariant.getArgb(scheme)),
		ring: argbToOklch(MaterialDynamicColors.primary.getArgb(scheme)),
		sidebar: argbToOklch(
			isDark
				? MaterialDynamicColors.surfaceContainerLow.getArgb(scheme)
				: MaterialDynamicColors.surfaceContainerLowest.getArgb(scheme),
		),
		sidebarForeground: argbToOklch(MaterialDynamicColors.onSurface.getArgb(scheme)),
		sidebarPrimary: argbToOklch(MaterialDynamicColors.primary.getArgb(scheme)),
		sidebarPrimaryForeground: argbToOklch(MaterialDynamicColors.onPrimary.getArgb(scheme)),
		sidebarAccent: argbToOklch(
			isDark
				? MaterialDynamicColors.surfaceContainerHigh.getArgb(scheme)
				: MaterialDynamicColors.surfaceContainerHighest.getArgb(scheme),
		),
		sidebarAccentForeground: argbToOklch(MaterialDynamicColors.onSurface.getArgb(scheme)),
		sidebarBorder: argbToOklch(MaterialDynamicColors.outline.getArgb(scheme)),
		sidebarRing: argbToOklch(MaterialDynamicColors.primary.getArgb(scheme)),
	};
}

export function generateColorScheme(sourceColor: string): {
	light: ShadcnColorScheme;
	dark: ShadcnColorScheme;
} {
	const sourceArgb = argbFromHex(sourceColor);
	const sourceHct = Hct.fromInt(sourceArgb);

	const lightScheme = new SchemeTonalSpot(sourceHct, false, 0.0);
	const darkScheme = new SchemeTonalSpot(sourceHct, true, 0.0);

	return {
		light: materialToShadcn(lightScheme),
		dark: materialToShadcn(darkScheme),
	};
}

export function generateCSSVariables(scheme: ShadcnColorScheme): Record<string, string> {
	return {
		"--background": scheme.background,
		"--foreground": scheme.foreground,
		"--card": scheme.card,
		"--card-foreground": scheme.cardForeground,
		"--popover": scheme.popover,
		"--popover-foreground": scheme.popoverForeground,
		"--primary": scheme.primary,
		"--primary-foreground": scheme.primaryForeground,
		"--secondary": scheme.secondary,
		"--secondary-foreground": scheme.secondaryForeground,
		"--muted": scheme.muted,
		"--muted-foreground": scheme.mutedForeground,
		"--accent": scheme.accent,
		"--accent-foreground": scheme.accentForeground,
		"--destructive": scheme.destructive,
		"--destructive-foreground": scheme.destructiveForeground,
		"--border": scheme.border,
		"--input": scheme.input,
		"--ring": scheme.ring,
		"--sidebar": scheme.sidebar,
		"--sidebar-foreground": scheme.sidebarForeground,
		"--sidebar-primary": scheme.sidebarPrimary,
		"--sidebar-primary-foreground": scheme.sidebarPrimaryForeground,
		"--sidebar-accent": scheme.sidebarAccent,
		"--sidebar-accent-foreground": scheme.sidebarAccentForeground,
		"--sidebar-border": scheme.sidebarBorder,
		"--sidebar-ring": scheme.sidebarRing,
	};
}
