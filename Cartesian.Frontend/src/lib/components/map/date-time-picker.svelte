<script lang="ts">
	import { Calendar } from "$lib/components/ui/calendar";
	import * as Popover from "$lib/components/ui/popover";
	import Button from "$lib/components/ui/button/button.svelte";
	import { Input } from "$lib/components/ui/input";
	import { cn } from "$lib/utils";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { Calendar03Icon, Clock01Icon } from "@hugeicons/core-free-icons";
	import {
		DateFormatter,
		type DateValue,
		getLocalTimeZone,
		parseDate,
		CalendarDate,
		today
	} from "@internationalized/date";

	let {
		value = $bindable(),
		placeholder = "Pick a date",
		class: className
	} = $props<{
		value: string;
		placeholder?: string;
		class?: string;
	}>();

	const df = new DateFormatter("en-US", {
		dateStyle: "medium"
	});

	let date = $state<DateValue | undefined>();
	let time = $state("12:00");
	let open = $state(false);

	// Parse initial value
	$effect(() => {
		if (value) {
			try {
				const [d, t] = value.split("T");
				if (d) date = parseDate(d);
				if (t) time = t.slice(0, 5); // Ensure HH:mm
			} catch (e) {
				// Invalid date format, ignore
			}
		}
	});

	// Update value when date or time changes
	$effect(() => {
		if (date && time) {
			const d = date.toString(); // YYYY-MM-DD
			value = `${d}T${time}`;
		}
	});

	function onDateSelect(v: DateValue | undefined) {
		if (v) {
			date = v;
		} else {
			date = undefined;
		}
	}
</script>

<div class={cn("grid gap-2", className)}>
	<Popover.Root bind:open>
		<Popover.Trigger>
			{#snippet child({ props })}
				<Button
					variant="outline"
					class={cn(
						"w-full justify-start text-left font-normal h-10 bg-muted/30 border-border/50 shadow-none",
						!date && "text-muted-foreground"
					)}
					{...props}
				>
					<HugeiconsIcon icon={Calendar03Icon} size={16} strokeWidth={2} className="mr-2 shrink-0" />
					<span class="truncate">
						{#if date}
							{df.format(date.toDate(getLocalTimeZone()))} at {time}
						{:else}
							{placeholder}
						{/if}
					</span>
				</Button>
			{/snippet}
		</Popover.Trigger>
		<Popover.Content class="w-auto p-0" align="start">
			<div class="flex flex-col sm:flex-row">
				<div class="p-3">
					<Calendar type="single" bind:value={date as any} />
				</div>
				<div class="border-t sm:border-l sm:border-t-0 p-3 flex flex-col gap-2 min-w-[150px]">
					<div class="flex items-center gap-2 text-sm font-medium text-muted-foreground mb-1">
						<HugeiconsIcon icon={Clock01Icon} size={16} strokeWidth={2} />
						Time
					</div>
					<Input
						type="time"
						bind:value={time}
						class="h-9"
					/>
					<div class="text-xs text-muted-foreground mt-auto pt-2">
						Select time for the event window.
					</div>
				</div>
			</div>
		</Popover.Content>
	</Popover.Root>
</div>
