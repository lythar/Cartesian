<script lang="ts">
	import { onMount, tick } from "svelte";
  import mapboxgl from "mapbox-gl";
	import { mapMarkers, mapState, newEventMarkerLocation, newEventOverlayState } from "./map-state.svelte";
	import { authStore } from "$lib/stores/auth.svelte";
	import LoginAlertDialog from "$lib/components/auth/login-alert-dialog.svelte";
	import { animate } from "motion";
	import { createReverseGeocodeQuery, type ReverseGeocode } from "$lib/api/queries/reverse-geocode.query";
	import Button from "../ui/button/button.svelte";
	import { HugeiconsIcon } from "@hugeicons/svelte";
	import { Add01Icon } from "@hugeicons/core-free-icons";

  interface Props {
    map: mapboxgl.Map;
    selectedLocation: { lng: number; lat: number } | null;
  }

  let { map, selectedLocation = $bindable(null) }: Props = $props();

  let loginAlertOpen = $state(false);

  let queryLocation = $state<{ lng: number; lat: number } | null>(null);

  const geocodeQuery = $derived(
    queryLocation
      ? createReverseGeocodeQuery(queryLocation.lng, queryLocation.lat, {
          query: { enabled: true }
        })
      : null
  );

  let marker: mapboxgl.Marker | null = $state(null);

  let markerWrapperElement: HTMLDivElement | null = $state(null);
  let markerElement: HTMLDivElement | null = $state(null);
  let locationElement: HTMLDivElement | null = $state(null);


  // This might cook one day, even calc-size() if it would be in firefox, but I don't have time to figure this out.
  // $effect.pre(() => {
  //   if (!locationElement) return;

  //   const currentWidth = locationElement.offsetWidth;

  //   // oxlint-disable-next-line no-unused-expressions
  //   geocodeQuery; // mark it as a dependency

  //   tick().then(() => {
  //     if (!locationElement) return;

  //     const newWidth = locationElement.offsetWidth;

  //     if (newWidth !== currentWidth) {
  //       animate(locationElement!, {
  //         maxWidth: [currentWidth + "px", newWidth + "px"]
  //       }, { duration: 0.3, ease: "easeOut" });
  //     }
  //   });
  // })

  const ANIMATION_LOCATION_DELTA = 0.001;

  const handleUpdateNewEventMarker = (e: mapboxgl.MapMouseEvent) => {
    const features = mapState.instance!.queryRenderedFeatures(e.point, {
      layers: ["clusters", "unclustered-point"]
    });

    if (features.length > 0) {
      return;
    }

    const prev = selectedLocation;
    const { lng, lat } = e.lngLat;

    if (prev && Math.abs(prev.lng - lng) < ANIMATION_LOCATION_DELTA && Math.abs(prev.lat - lat) < ANIMATION_LOCATION_DELTA) {
      selectedLocation = { lng: e.lngLat.lng, lat: e.lngLat.lat };
      marker!.setLngLat([lng, lat]);
    } else {
      markerElement!.style.opacity = "0";

      selectedLocation = { lng: e.lngLat.lng, lat: e.lngLat.lat };
      marker!.setLngLat([lng, lat]);

      animate(markerElement!, {
        opacity: [0, 1],
        scale: [0.8, 1]
      }, { duration: 0.3, type: "spring", mass: 1, stiffness: 250, damping: 18 });
    }

    queryLocation = { lng, lat };
  };


  onMount(() => {
		mapState.instance!.on("click", handleUpdateNewEventMarker);

    marker = new mapboxgl.Marker(markerWrapperElement!, {
      anchor: "bottom",
    })
      .setLngLat([0, 0])
      .addTo(map);
  })
</script>

<div bind:this={markerWrapperElement} class="w-8 h-8">
  <img bind:this={markerElement} src="/lythar.svg" alt="Map Marker" class="w-full h-full" style="pointer-events: none;" />
</div>

<LoginAlertDialog bind:open={loginAlertOpen} />

<div class="absolute bottom-4 left-1/2 transform -translate-x-1/2 z-10">
  {#if selectedLocation}
    <div bind:this={locationElement} class="bg-background p-4 rounded-lg shadow-lg inline-flex flex-row gap-6 justify-between min-w-50">
      <div class="flex flex-col">
        {#if geocodeQuery?.isPending}
          <p class="text-sm font-semibold bg-linear-to-r from-primary/40 via-primary to-primary/40 bg-clip-text text-transparent animate-gradient bg-size-[200%_100%]">
            Loading address...
          </p>
        {:else if geocodeQuery?.isError}
          <p class="text-sm text-destructive">Failed to load address</p>
        {:else if geocodeQuery?.data?.features?.[0]?.properties}
          {@const properties = geocodeQuery.data.features[0].properties}
          <p class="text-sm font-semibold text-foreground">
            {properties.name ?? properties.place_formatted ?? "Unknown location"}
          </p>
        {/if}

        <p class="text-sm text-muted-foreground">{selectedLocation.lng.toFixed(2)}, {selectedLocation.lat.toFixed(2)}</p>
      </div>
      <!-- CTA -->
      <div class="w-fit flex items-center gap-2">
        <Button
          variant="default"
          size="lg"
          onclick={() => {
            if (!$authStore.isAuthenticated) {
              loginAlertOpen = true;
              return;
            }
            newEventOverlayState.location = selectedLocation;
            newEventOverlayState.source = "toolbar";
            newEventOverlayState.open = true;
            console.log(newEventOverlayState);
            selectedLocation = null;
            queryLocation = null;
            marker!.setLngLat([0, 0]);
          }}
        >
          <HugeiconsIcon icon={Add01Icon} strokeWidth={2} className="size-5" />
          <span>Add event here</span>
        </Button>
        <Button
          variant="destructive"
          size="lg"
          onclick={() => {
            selectedLocation = null;
            queryLocation = null;
            marker!.setLngLat([0, 0]);
          }}
        >
          Cancel
        </Button>
      </div>
    </div>
  {/if}
</div>

<style>
  @keyframes gradient {
    0% {
      background-position: 0% 50%;
    }
    50% {
      background-position: 100% 50%;
    }
    100% {
      background-position: 0% 50%;
    }
  }

  .animate-gradient {
    animation: gradient 2s ease infinite;
  }
</style>
