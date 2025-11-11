<script lang="ts">
	import { themeApplier } from "$lib/material-gen";

	let color = $state(themeApplier.sourceColor.current);

	function updateColor(newColor: string) {
		color = newColor;
		themeApplier.setSourceColor(newColor);
	}
</script>

<div class="flex items-center gap-3">
	<label for="theme-color-picker" class="text-sm font-medium">Theme Color:</label>
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
		placeholder="#009FE7"
	/>
</div>
