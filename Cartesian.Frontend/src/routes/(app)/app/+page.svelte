<script lang="ts">
	import MapRenderer from "$lib/components/map/map-renderer.svelte";
	import { page } from "$app/stores";
	import { goto } from "$app/navigation";
	import { getEventApiEventId } from "$lib/api/cartesian-client";
	import { mapInteractionState } from "$lib/components/map/map-state.svelte";
	import { getMediaUrl } from "$lib/utils";

	let { data } = $props();

	$effect(() => {
		const eventId = $page.url.searchParams.get("event");
		if (eventId) {
			getEventApiEventId(eventId)
				.then((event) => {
					const window = event.windows?.[0];

					mapInteractionState.selectedEvent = {
						eventId: event.id,
						eventName: event.name,
						eventDescription: event.description,
						windowId: window?.id ?? "",
						windowTitle: window?.title ?? "",
						windowDescription: window?.description ?? "",
						authorId: event.author.id,
						authorName: event.author.name,
						authorAvatar: event.author.avatar?.id ?? "",
						communityId: event.community?.id ?? null,
						communityName: event.community?.name ?? null,
						communityAvatar: event.community?.avatar?.id ?? null,
						visibility: event.visibility,
						timing: event.timing,
						tags: event.tags as unknown as string[],
						startTime: (window?.startTime as string) ?? "",
						endTime: (window?.endTime as string) ?? "",
						createdAt: new Date().toISOString(),
					};
					mapInteractionState.eventDetailsOpen = true;

					const newUrl = new URL($page.url);
					newUrl.searchParams.delete("event");
					goto(newUrl.toString(), { replaceState: true });
				})
				.catch((err) => {
					console.error("Failed to load event from URL", err);
				});
		}
	});
</script>

<svelte:head>
	{#if data.metaEvent}
		<title>{data.metaEvent.name} | Lythar</title>
		<meta name="description" content={data.metaEvent.description} />
		<meta property="og:title" content={data.metaEvent.name} />
		<meta property="og:description" content={data.metaEvent.description} />
		<meta property="og:site_name" content={`Hosted by ${data.metaEvent.author.name}`} />
		<meta property="og:url" content={$page.url.href} />
		<meta property="og:type" content="website" />

		{#if data.metaEvent.image}
			<meta
				property="og:image"
				content={new URL(getMediaUrl(data.metaEvent.image.id), $page.url).href}
			/>
			<meta
				name="twitter:image"
				content={new URL(getMediaUrl(data.metaEvent.image.id), $page.url).href}
			/>
		{/if}

		<meta name="twitter:card" content="summary_large_image" />
		<meta name="twitter:title" content={data.metaEvent.name} />
		<meta name="twitter:description" content={data.metaEvent.description} />
	{/if}
</svelte:head>

<MapRenderer ipGeo={data.ipGeo} />
