<script lang="ts">
	import { getLayoutContext } from "$lib/context/layout.svelte";
	import { themeApplier } from "$lib/material-gen";
	import { m } from "$lib/paraglide/messages";

	let color = $state(themeApplier.sourceColor.current);

	function updateColor(newColor: string) {
		color = newColor;
		themeApplier.setSourceColor(newColor);
	}
</script>

<div class="flex items-center gap-3">
	<label for="theme-color-picker" class="text-sm font-medium">{m.theme_color_label()}</label>
	<input
		id="theme-color-picker"
		type="color"
		value={color}
		oninput={(e) => updateColor(e.currentTarget.value)}
		class="h-10 w-16 cursor-pointer rounded border border-input"
	/>
	<input
		type="text"
		value={color}
		oninput={(e) => updateColor(e.currentTarget.value)}
		class="w-28 rounded border border-input bg-background px-3 py-2 text-sm"
		placeholder={m.color_picker_placeholder()}
	/>
</div>
