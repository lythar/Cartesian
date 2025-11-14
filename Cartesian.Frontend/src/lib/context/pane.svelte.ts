import { getContext, setContext } from "svelte";
import type { Component } from "svelte";

const PANE_CONTEXT_KEY = Symbol("pane");

export interface PaneConfig {
	id: string;
	component: Component;
	side: "left" | "right";
	props?: Record<string, unknown>;
}

class PaneManager {
	activeLeftPane = $state<string | null>(null);
	activeRightPane = $state<string | null>(null);

	private registeredPanes = $state<Map<string, PaneConfig>>(new Map());

	registerPane(config: PaneConfig) {
		this.registeredPanes.set(config.id, config);
	}

	unregisterPane(id: string) {
		this.registeredPanes.delete(id);
		if (this.activeLeftPane === id) this.activeLeftPane = null;
		if (this.activeRightPane === id) this.activeRightPane = null;
	}

	activatePane(id: string) {
		const pane = this.registeredPanes.get(id);
		if (!pane) return;

		if (pane.side === "left") {
			this.activeLeftPane = this.activeLeftPane === id ? null : id;
		} else {
			this.activeRightPane = this.activeRightPane === id ? null : id;
		}
	}

	getActiveLeftPane() {
		if (!this.activeLeftPane) return null;
		return this.registeredPanes.get(this.activeLeftPane);
	}

	getActiveRightPane() {
		if (!this.activeRightPane) return null;
		return this.registeredPanes.get(this.activeRightPane);
	}

	closeLeftPane() {
		this.activeLeftPane = null;
	}

	closeRightPane() {
		this.activeRightPane = null;
	}

	closeAllPanes() {
		this.activeLeftPane = null;
		this.activeRightPane = null;
	}
}

export function createPaneContext() {
	const manager = new PaneManager();
	setContext(PANE_CONTEXT_KEY, manager);
	return manager;
}

export function getPaneContext(): PaneManager {
	const context = getContext<PaneManager>(PANE_CONTEXT_KEY);
	if (!context) {
		throw new Error("Pane context not found. Did you forget to call createPaneContext()?");
	}
	return context;
}
