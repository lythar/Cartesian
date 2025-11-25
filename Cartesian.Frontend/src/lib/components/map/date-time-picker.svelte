<script lang="ts">
	import { Calendar } from "$lib/components/ui/calendar";
	import * as Popover from "$lib/components/ui/popover";
	import Button from "$lib/components/ui/button/button.svelte";
	import { Input } from "$lib/components/ui/input";
	import { cn } from "$lib/utils";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { Calendar03Icon, Clock01Icon } from "@hugeicons/core-free-icons";
	import { m } from "$lib/paraglide/messages";
	import {
		DateFormatter,
		type DateValue,
		getLocalTimeZone,
		parseDate,
		CalendarDate,
		today,
	} from "@internationalized/date";

	let {
		value = $bindable(),
		placeholder,
		class: className,
	} = $props<{
		value: string;
		placeholder?: string;
		class?: string;
	}>();

	const actualPlaceholder = $derived(placeholder ?? m.date_picker_placeholder());

	const df = new DateFormatter("en-US", {
		dateStyle: "medium",
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
						"h-10 w-full justify-start border-border/50 bg-muted/30 text-left font-normal shadow-none",
						!date && "text-muted-foreground",
					)}
					{...props}
				>
					<HugeiconsIcon
						icon={Calendar03Icon}
						size={16}
						strokeWidth={2}
						className="mr-2 shrink-0"
					/>
					<span class="truncate">
						{#if date}
							{df.format(date.toDate(getLocalTimeZone()))} at {time}
						{:else}
							{actualPlaceholder}
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
				<div
					class="flex min-w-[150px] flex-col gap-2 border-t p-3 sm:border-t-0 sm:border-l"
				>
					<div
						class="mb-1 flex items-center gap-2 text-sm font-medium text-muted-foreground"
					>
						<HugeiconsIcon icon={Clock01Icon} size={16} strokeWidth={2} />
						Time
					</div>
					<Input type="time" bind:value={time} class="h-9" />
					<div class="mt-auto pt-2 text-xs text-muted-foreground">
						Select time for the event window.
					</div>
				</div>
			</div>
		</Popover.Content>
	</Popover.Root>
</div>
