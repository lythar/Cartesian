import { getContext, setContext } from "svelte";

class LayoutContext {
	private query = $state<MediaQueryList>();
	private _matches = $state(false);

	constructor() {
		if (typeof window !== "undefined") {
			this.query = window.matchMedia("(min-width: 1024px)");
			this._matches = this.query.matches;

			this.query.addEventListener("change", (e) => {
				this._matches = e.matches;
			});
		}
	}

	get isDesktop() {
		return this._matches;
	}

	get isMobile() {
		return !this._matches;
	}
}

const LAYOUT_KEY = Symbol("layout");

export function createLayoutContext() {
	const context = new LayoutContext();
	setContext(LAYOUT_KEY, context);
	return context;
}

export function getLayoutContext() {
	return getContext<LayoutContext>(LAYOUT_KEY);
}
