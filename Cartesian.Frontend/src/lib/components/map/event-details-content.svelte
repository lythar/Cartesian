<script lang="ts">
    import { createGetEventApiEventIdImages, createGetEventApiEventId } from "$lib/api/cartesian-client";
    import { createReverseGeocodeQuery } from "$lib/api/queries/forward-geocode.query";
    import type { MapEventProperties } from "./map-state.svelte";
    import * as Carousel from "$lib/components/ui/carousel";
    import { Button } from "$lib/components/ui/button";
    import { Cancel01Icon, Calendar03Icon, Clock01Icon, Location01Icon } from "@hugeicons/core-free-icons";
    import { HugeiconsIcon } from "@hugeicons/svelte";
    import { Avatar, AvatarFallback, AvatarImage } from "$lib/components/ui/avatar";
    import { format } from "date-fns";
	  import { getAvatarUrl, getMediaUrl } from "$lib/utils";

    interface Props {
        event: MapEventProperties;
        onClose: () => void;
    }

    let { event, onClose }: Props = $props();

    const imagesQuery = createGetEventApiEventIdImages(event.eventId);
    let images = $derived(imagesQuery.data || []);

    const eventDetailsQuery = createGetEventApiEventId(event.eventId);
    let eventDetails = $derived(eventDetailsQuery.data);
    let hasMultipleSchedules = $derived((eventDetails?.windows?.length ?? 0) > 1);

    let currentWindow = $derived(eventDetails?.windows?.find((w) => w.id === event.windowId));
    let eventLocation = $derived(currentWindow?.location as unknown as { coordinates: [number, number] } | undefined);
    let longitude = $derived(eventLocation?.coordinates?.[0]);
    let latitude = $derived(eventLocation?.coordinates?.[1]);

    const reverseGeocodeQuery = createReverseGeocodeQuery(() =>
        longitude !== undefined && latitude !== undefined ? { longitude: longitude!, latitude: latitude! } : undefined,
    );

    let address = $derived(
        reverseGeocodeQuery.data?.features?.[0]?.properties?.place_formatted ||
            reverseGeocodeQuery.data?.features?.[0]?.properties?.full_address ||
            "Unknown location",
    );

    let tags = $derived(typeof event.tags === 'string' ? JSON.parse(event.tags) : event.tags || []);
    let displayTags = $derived(tags.slice(0, 6));
    let remainingTags = $derived(tags.length - 6);
    let showAllTags = $state(false);
</script>

<div class="relative h-48 w-full bg-muted shrink-0">
    {#if images.length > 0}
            <Carousel.Root class="h-full w-full">
            <Carousel.Content>
                {#each images as image}
                    <Carousel.Item>
                        <div class="h-48 w-full bg-cover bg-center" style="background-image: url({getMediaUrl(image.id)})"></div>
                    </Carousel.Item>
                {/each}
            </Carousel.Content>
            {#if images.length > 1}
              <Carousel.Previous class="left-2" />
              <Carousel.Next class="right-2" />
            {/if}
        </Carousel.Root>
    {:else}
        <div class="flex h-full w-full items-center justify-center text-muted-foreground">
            <HugeiconsIcon icon={Calendar03Icon} size={48} />
        </div>
    {/if}

    <Button
        variant="ghost"
        size="icon"
        class="absolute right-4 top-4 h-8 w-8 rounded-full bg-background/50 backdrop-blur hover:bg-background/80"
        onclick={onClose}
    >
        <HugeiconsIcon icon={Cancel01Icon} size={16} strokeWidth={2} />
        <span class="sr-only">Close</span>
    </Button>
</div>

<div class="flex-1 overflow-y-auto p-6">
    <div class="mb-6">
        <h2 class="mb-2 text-2xl font-bold tracking-tight">{event.eventName}</h2>
        
        {#if !hasMultipleSchedules}
            <div class="mb-3 flex flex-wrap items-center gap-x-4 gap-y-2 text-sm text-muted-foreground">
                <div class="flex items-center gap-1.5">
                    <HugeiconsIcon icon={Calendar03Icon} size={16} />
                    <span>{format(new Date(event.startTime), "MMM d, yyyy")}</span>
                </div>
                <div class="flex items-center gap-1.5">
                    <HugeiconsIcon icon={Clock01Icon} size={16} />
                    <span>{format(new Date(event.startTime), "h:mm a")} - {format(new Date(event.endTime), "h:mm a")}</span>
                </div>
            </div>

            {#if eventLocation}
                <div class="mb-3 flex items-start gap-1.5 text-sm text-muted-foreground">
                    <HugeiconsIcon icon={Location01Icon} size={16} className="mt-0.5 shrink-0" />
                    <div class="flex flex-col">
                        <span class="font-medium text-foreground">{address}</span>
                        <span class="text-xs text-muted-foreground">
                            {latitude?.toFixed(6)}, {longitude?.toFixed(6)}
                        </span>
                    </div>
                </div>
            {/if}
        {/if}

        <div class="flex items-center gap-2">
            <Avatar class="h-6 w-6">
                <AvatarImage src={event.communityId ? getAvatarUrl({ id: event.communityAvatar! }) : getAvatarUrl({ id: event.authorAvatar! })} alt={event.communityId ? event.communityName : event.authorName} />
                <AvatarFallback>{(event.communityId ? event.communityName : event.authorName)?.substring(0, 2).toUpperCase()}</AvatarFallback>
            </Avatar>
            <span class="text-sm font-medium text-muted-foreground">
                Hosted by <span class="text-foreground">{event.communityId ? event.communityName : event.authorName}</span>
            </span>
        </div>
    </div>

    <!-- Tags -->
    <div class="mb-6 flex flex-wrap gap-2">
        {#each (showAllTags ? tags : displayTags) as tag}
            <span class="rounded-full bg-secondary px-3 py-1 text-xs font-medium text-secondary-foreground">
                {tag}
            </span>
        {/each}
        {#if remainingTags > 0 && !showAllTags}
            <button
                class="rounded-full bg-muted px-3 py-1 text-xs font-medium text-muted-foreground hover:bg-muted/80"
                onclick={() => showAllTags = true}
            >
                +{remainingTags} more
            </button>
        {/if}
    </div>

    <!-- Description -->
    <div class="mb-8">
        <h3 class="mb-2 text-sm font-semibold uppercase tracking-wider text-muted-foreground">About</h3>
        <p class="text-sm leading-relaxed text-foreground/90 whitespace-pre-wrap">
            {event.eventDescription}
        </p>
    </div>

    <!-- Timeline -->
    {#if hasMultipleSchedules}
        <div>
            <h3 class="mb-4 text-sm font-semibold uppercase tracking-wider text-muted-foreground">Schedule</h3>
            <div class="relative border-l border-border pl-4 ml-2 space-y-6">
                <div class="relative">
                    <div class="absolute -left-[21px] top-1 h-2.5 w-2.5 rounded-full bg-primary ring-4 ring-background"></div>
                    <div class="rounded-lg border bg-card p-3 shadow-sm">
                        <h4 class="font-medium">{event.windowTitle || "Event Occurrence"}</h4>
                        {#if event.windowDescription}
                            <p class="mt-1 text-xs text-muted-foreground">{event.windowDescription}</p>
                        {/if}
                        <div class="mt-2 flex items-center gap-3 text-xs text-muted-foreground">
                            <div class="flex items-center gap-1">
                                <HugeiconsIcon icon={Calendar03Icon} size={14} />
                                <span>{format(new Date(event.startTime), "MMM d, yyyy")}</span>
                            </div>
                            <div class="flex items-center gap-1">
                                <HugeiconsIcon icon={Clock01Icon} size={14} />
                                <span>{format(new Date(event.startTime), "h:mm a")} - {format(new Date(event.endTime), "h:mm a")}</span>
                            </div>
                        </div>
                        {#if eventLocation}
                            <div class="mt-2 flex items-start gap-1 text-xs text-muted-foreground">
                                <HugeiconsIcon icon={Location01Icon} size={14} className="mt-0.5 shrink-0" />
                                <div>
                                    <span class="font-medium text-foreground">{address}</span>
                                    <span class="ml-1 text-muted-foreground">({latitude?.toFixed(6)}, {longitude?.toFixed(6)})</span>
                                </div>
                            </div>
                        {/if}
                    </div>
                </div>
            </div>
        </div>
    {/if}
</div>
