# UI/UX Plan for Social Map-Based Events App

## Core Concept

Map-first design where the map is ALWAYS visible. All UI elements float/overlay on top. Think Airbnb meets Google Maps meets Meetup.

---

## Component Architecture

### **1. Always Visible (Level 1)**

**Top Bar:**

- **Search Input** (left side, expandable)
    - Regular search + AI search toggle
    - Autocomplete with event suggestions
    - Recent searches
    - Quick filters dropdown (date, category, distance)
- **User Menu** (right side)
    - Avatar/profile pic
    - Dropdown: Profile, Saved Events, Settings, Logout

**Floating Action Button (FAB):**

- Bottom right corner
- Primary action: "+ Add Event"
- Secondary actions (expandable): Add Gathering, Add Meetup, etc.

**Map Controls:**

- Zoom in/out
- Recenter to user location
- Map style toggle (dark/light/satellite)

---

### **2. Context Panels (Level 2)**

**Event Discovery Panel:**

_Mobile:_ Bottom sheet

- Starts at ~80px peek (showing event count)
- Swipe up to half screen → Event cards list
- Swipe up to full screen → Selected event details
- Swipeable horizontal carousel of nearby events

_Desktop:_ Left sidebar (400px)

- Collapsible panel
- List of event cards
- Scroll to browse
- Hover preview on map

**Quick Filters Popover:**

- Categories (concerts, sports, social, etc.)
- Date range picker
- Distance slider
- Event type (free/paid, public/private)
- Apply/Clear buttons

**Notifications Tray:**

- Slide from top right
- Event invites
- RSVP confirmations
- Friend activities
- New events in saved areas

---

### **3. Full Overlays (Level 3)**

**Event Details Modal:**

- Full screen on mobile, centered modal on desktop
- Hero image/banner
- Event info (title, date, time, location)
- Description
- Host info
- Attendees list
- Comments/Discussion
- RSVP/Join button
- Share button

**Add/Edit Event Form:**

- Full screen overlay
- Step-by-step wizard:
    1. Click on map to set location
    2. Event details (title, description)
    3. Date/time picker
    4. Category & tags
    5. Cover image upload
    6. Visibility settings
    7. Preview & Submit

**AI Search Interface:**

- Full screen takeover
- Conversational UI
- "Find me outdoor events this weekend"
- Shows AI processing
- Results as event cards + map markers
- Save/refine search

**User Profile:**

- Modal on desktop, full screen on mobile
- Profile header
- Events created
- Events attending
- Reviews/ratings
- Social connections

---

## Map Interaction Patterns

\*\*Event Markers

- Cluster when zoomed out (with count badge)
- Individual markers when zoomed in
- Color-coded by category
- Different icons for event types
- Pulse animation for happening now
- Click marker → Small preview card pops up
- Click preview → Full event details

**Map States:**

- **Browse Mode** (default): Shows all events, bottom sheet with event list
- **Search Mode**: Shows search results, dims non-matching markers
- **Event Focus**: Selected event highlighted, others faded
- **Add Event Mode**: Crosshair cursor, click to place

---

## Mobile-Specific Features

**Gestures:**

- Swipe up on bottom sheet: Expand event list
- Swipe down: Collapse to map
- Swipe left/right on event cards: Next/previous event
- Long press on map: Quick add event here
- Pull down from top: Refresh events

**Bottom Navigation (Optional):**

- 3-4 items max:
    - Explore (home)
    - My Events
    - Add Event
    - Profile

---

## Desktop-Specific Features

**Layout:**

- Resizable left sidebar (event list)
- Optional right sidebar (event details when viewing)
- Keyboard shortcuts (ESC to close, / for search, etc.)
- Multi-select events (shift+click markers)
- Hover states on markers show mini preview

---

## State Management Strategy

Replace generic pane system with:

```typescript
// UI State Manager
{
  bottomSheet: 'collapsed' | 'peek' | 'half' | 'full',
  activeEventId: string | null,
  searchMode: 'regular' | 'ai' | null,
  addEventMode: boolean,
  filters: FilterState,
  notifications: NotificationState,
  userMenu: boolean,
}
```

---

## Component Breakdown

### To Build:

1. `search-bar.svelte` - Top search with AI toggle
2. `event-bottom-sheet.svelte` - Mobile event browser
3. `event-sidebar.svelte` - Desktop event list
4. `event-card.svelte` - Preview card component
5. `event-details-modal.svelte` - Full event view
6. `add-event-wizard.svelte` - Multi-step form
7. `fab-menu.svelte` - Floating action button
8. `filters-popover.svelte` - Filter controls
9. `map-marker.svelte` - Custom event markers
10. `ai-search-overlay.svelte` - AI search interface
11. `user-menu.svelte` - Profile dropdown
12. `notifications-tray.svelte` - Notification center

### State/Context:

- `map-ui.svelte.ts` - UI state management
- `events.svelte.ts` - Event data & queries
- `filters.svelte.ts` - Filter state
- `search.svelte.ts` - Search state & history

---

## Inspiration References

- **Airbnb**: Bottom sheet pattern, map + cards
- **Google Maps**: Search, markers, info windows
- **Meetup**: Event categories, RSVP flow
- **Instagram**: Add content flow, stories UI
- **Linear**: AI command bar, keyboard shortcuts
