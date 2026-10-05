<script setup lang="ts">
import { ref, computed } from "vue";
import { events, neighborhoods, type Event } from "../data/demo.ts";
import MapPinpoint from "../components/MapPinpoint.vue";
import { useI18n } from "../i18n/index";

const { t, locale } = useI18n();

const props = defineProps<{
  search?: string;
  selectedDate?: string | null;
}>();

// Neighborhood-to-SVG coordinate mapping (px, within bucharest.svg viewBox 0 0 210 297)
const neighborhoodCoords: Record<string, { cx: number; cy: number }> = {
  romexpo: { cx: 90, cy: 68 },
  floreasca: { cx: 118, cy: 82 },
  "drumul-taberei": { cx: 58, cy: 148 },
  "old-town": { cx: 102, cy: 124 },
};

const selectedNeighborhood = ref<string | null>(null);
const selectedEvent = ref<Event | null>(events[0] || null);

// Zoom and Pan state
const zoomLevel = ref(1.0);
const panX = ref(0);
const panY = ref(0);
const isPanning = ref(false);
const startMouse = ref({ x: 0, y: 0 });
const startPan = ref({ x: 0, y: 0 });

const filteredEvents = computed(() => {
  return events.filter((evt) => {
    if (selectedNeighborhood.value && evt.neighborhood !== selectedNeighborhood.value) {
      return false;
    }
    if (props.search) {
      const q = props.search.toLowerCase();
      const matchTitle = (evt.localizedTitle || evt.title).toLowerCase().includes(q);
      const matchDesc = (evt.localizedDescription || evt.description).toLowerCase().includes(q);
      const matchNeighbourhood = evt.neighborhood.toLowerCase().includes(q);
      if (!matchTitle && !matchDesc && !matchNeighbourhood) return false;
    }
    return true;
  });
});

function handleSelectEvent(evt: Event) {
  selectedEvent.value = evt;
}

function selectNeighborhood(id: string | null) {
  selectedNeighborhood.value = selectedNeighborhood.value === id ? null : id;
  if (id && neighborhoodCoords[id]) {
    // Zoom in on selected neighborhood
    const coord = neighborhoodCoords[id];
    zoomLevel.value = 2.0;
    // Offset relative to center (105, 148)
    panX.value = Math.round((105 - coord.cx) * 1.5);
    panY.value = Math.round((148 - coord.cy) * 1.5);
  }
}

function zoomIn() {
  if (zoomLevel.value < 4.0) {
    zoomLevel.value = Math.min(4.0, Number((zoomLevel.value + 0.3).toFixed(2)));
  }
}

function zoomOut() {
  if (zoomLevel.value > 1.0) {
    zoomLevel.value = Math.max(1.0, Number((zoomLevel.value - 0.3).toFixed(2)));
    if (zoomLevel.value === 1.0) {
      panX.value = 0;
      panY.value = 0;
    }
  }
}

function resetZoom() {
  zoomLevel.value = 1.0;
  panX.value = 0;
  panY.value = 0;
}

function handleWheel(e: WheelEvent) {
  if (e.deltaY < 0) {
    zoomIn();
  } else {
    zoomOut();
  }
}

function onMouseDown(e: MouseEvent) {
  if (zoomLevel.value > 1.0) {
    isPanning.value = true;
    startMouse.value = { x: e.clientX, y: e.clientY };
    startPan.value = { x: panX.value, y: panY.value };
  }
}

function onMouseMove(e: MouseEvent) {
  if (!isPanning.value) return;
  const dx = (e.clientX - startMouse.value.x) / zoomLevel.value;
  const dy = (e.clientY - startMouse.value.y) / zoomLevel.value;
  panX.value = Math.round(startPan.value.x + dx);
  panY.value = Math.round(startPan.value.y + dy);
}

function onMouseUp() {
  isPanning.value = false;
}
</script>

<template>
  <main class="min-h-[calc(100vh-64px)] bg-bg p-4 md:p-8 text-text select-none">
    <div class="mx-auto max-w-7xl flex flex-col gap-6">
      <!-- Top Header & Filter Bar -->
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 border-b border-border pb-4">
        <div>
          <h1 class="font-display text-heading text-text">
            {{ locale === 'ro' ? 'Harta Interactivă a Bucureștiului' : 'Interactive Map of Bucharest' }}
          </h1>
          <p class="font-body text-caption text-text-muted mt-1">
            {{ locale === 'ro' ? 'Explorează evenimentele și cartierele active în timp real' : 'Explore events and active neighborhoods in real time' }}
          </p>
        </div>

        <!-- Neighborhood Filters -->
        <div class="flex flex-wrap items-center gap-2">
          <button
            class="px-3 py-1.5 rounded-sm font-ui text-caption font-medium transition-colors border"
            :class="selectedNeighborhood === null 
              ? 'bg-action border-action text-action-text' 
              : 'bg-surface border-border text-text-muted hover:border-control-border hover:text-text'"
            @click="selectNeighborhood(null)"
          >
            {{ locale === 'ro' ? 'Toate Cartierele' : 'All Neighborhoods' }}
          </button>
          <button
            v-for="n in neighborhoods"
            :key="n.id"
            class="px-3 py-1.5 rounded-sm font-ui text-caption font-medium transition-colors border"
            :class="selectedNeighborhood === n.id
              ? 'bg-action border-action text-action-text'
              : 'bg-surface border-border text-text-muted hover:border-control-border hover:text-text'"
            @click="selectNeighborhood(n.id)"
          >
            {{ n.localizedName || n.name }}
          </button>
        </div>
      </div>

      <!-- Main Layout: Map + Event Details Side Panel -->
      <div class="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
        <!-- Map Canvas (8 cols) -->
        <div class="lg:col-span-8 flex flex-col gap-3">
          <div 
            class="relative w-full rounded-sm border border-border bg-surface p-4 md:p-6  overflow-hidden min-h-[500px] flex items-center justify-center cursor-grab active:cursor-grabbing"
            @wheel.prevent="handleWheel"
            @mousedown="onMouseDown"
            @mousemove="onMouseMove"
            @mouseup="onMouseUp"
            @mouseleave="onMouseUp"
          >
            <!-- Map Decorative Grid overlay -->
            

            <!-- Floating Zoom Controls Widget -->
            <div class="absolute top-4 right-4 z-20 flex flex-col items-center gap-1 bg-bg border border-border rounded-sm p-1.5  ">
              <button
                class="w-8 h-8 rounded-sm bg-surface hover:bg-action border border-control-border hover:border-action text-text hover:text-action-text flex items-center justify-center font-ui font-bold text-body-lg transition-colors"
                title="Zoom In (+)"
                @click.stop="zoomIn"
              >
                +
              </button>
              
              <span class="text-[10px] font-semibold text-action py-1 px-1">
                {{ Math.round(zoomLevel * 100) }}%
              </span>

              <button
                class="w-8 h-8 rounded-sm bg-surface hover:bg-action border border-control-border hover:border-action text-text hover:text-action-text flex items-center justify-center font-ui font-bold text-body-lg transition-colors"
                title="Zoom Out (-)"
                @click.stop="zoomOut"
              >
                −
              </button>

              <button
                v-if="zoomLevel > 1 || panX !== 0 || panY !== 0"
                class="w-8 h-8 rounded-sm bg-surface hover:bg-action-hover border border-control-border hover:border-action text-text-muted hover:text-action-text flex items-center justify-center font-ui text-caption transition-colors mt-1"
                title="Reset Zoom"
                @click.stop="resetZoom"
              >
                ↺
              </button>
            </div>

            <!-- SVG Map Graphics Wrapper with Smooth Zoom Transform -->
            <div 
              class="relative aspect-[210/297] w-full max-w-[500px] flex items-center justify-center transition-transform duration-200 ease-out"
              :style="{
                transform: `scale(${zoomLevel}) translate(${panX}px, ${panY}px)`,
                transformOrigin: 'center center'
              }"
            >
              <!-- Bucharest SVG Background Image -->
              <img
                src="/bucharest.svg"
                alt="Harta Bucuresti"
                class="absolute inset-0 w-full h-full object-contain pointer-events-none opacity-60 transition-opacity"
              >

              <!-- SVG Overlay for Pinpoints -->
              <svg
                viewBox="0 0 210 297"
                class="absolute inset-0 w-full h-full object-contain overflow-visible"
              >
                <MapPinpoint
                  v-for="event in filteredEvents"
                  :key="event.id"
                  :event="event"
                  :neighborhood="event.neighborhood"
                  :address="event.address ?? 'N/A'"
                  :localized-address="event.localizedAddress ?? (event.localizedTitle || event.title)"
                  :cx="neighborhoodCoords[event.neighborhood]?.cx"
                  :cy="neighborhoodCoords[event.neighborhood]?.cy"
                  @select="handleSelectEvent"
                />
              </svg>
            </div>

            <!-- Map Legend & Hint Badge -->
            <div class="absolute bottom-4 left-4 bg-bg border border-control-border rounded-sm p-2.5 font-ui text-caption flex items-center gap-3">
              <span class="flex items-center gap-1.5 text-text-muted">
                <span class="w-2.5 h-2.5 rounded-full bg-accent" />
                {{ locale === 'ro' ? 'Evenimente active' : 'Active Events' }}
              </span>
              <span class="text-text-muted">|</span>
              <span class="text-text-muted">{{ filteredEvents.length }} {{ locale === 'ro' ? 'pe hartă' : 'on map' }}</span>
              <span class="text-text-muted hidden sm:inline">|</span>
              <span class="font-ui text-caption text-text-muted hidden sm:inline">{{ locale === 'ro' ? 'Scroll / click +/- pentru zoom' : 'Scroll / click +/- to zoom' }}</span>
            </div>
          </div>
        </div>

        <!-- Sidebar / Active Event Info Card (4 cols) -->
        <div class="lg:col-span-4 flex flex-col gap-4">
          <div
            v-if="selectedEvent"
            class="rounded-sm border border-border bg-surface p-5  flex flex-col gap-4 "
          >
            <div class="flex items-center justify-between">
              <span class="px-2.5 py-1 rounded-sm bg-action/10 border border-action/30 text-action text-xs font-semibold uppercase tracking-wider">
                {{ selectedEvent.neighborhood }}
              </span>
              <span class="font-ui text-caption text-text-muted">
                {{ selectedEvent.whenLocalized }}
              </span>
            </div>

            <div>
              <h2 class="font-display text-subheading font-semibold text-text">
                {{ selectedEvent.localizedTitle || selectedEvent.title }}
              </h2>
              <p class="font-body text-caption text-text-muted mt-2 leading-relaxed">
                {{ selectedEvent.localizedDescription || selectedEvent.description }}
              </p>
            </div>

            <div class="flex flex-col gap-2 pt-2 border-t border-border font-body text-caption text-text-muted">
              <div
                v-if="selectedEvent.organizers?.length"
                class="flex items-center gap-2"
              >
                <span class="text-text-muted">{{ t('event.organizers') }}:</span>
                <span class="font-medium text-text">{{ selectedEvent.organizers.join(', ') }}</span>
              </div>
              <div class="flex items-center gap-2">
                <span class="text-text-muted">{{ t('event.interest') }}:</span>
                <span class="font-medium text-action">{{ selectedEvent.rsvpCount }} {{ locale === 'ro' ? 'persoane interesate' : 'people interested' }}</span>
              </div>
            </div>

            <div class="flex items-center gap-2 pt-2">
              <button class="w-full py-2.5 rounded-sm bg-action hover:bg-action-hover text-action-text font-ui font-medium text-caption transition-colors">
                {{ t('event.rsvpCta') }}
              </button>
            </div>
          </div>

          <!-- List of events on map -->
          <div class="rounded-sm border border-border bg-surface p-4 flex flex-col gap-3">
            <h3 class="font-ui text-caption font-bold uppercase tracking-wider text-text-muted px-1">
              {{ locale === 'ro' ? 'Lista evenimentelor' : 'Event List' }}
            </h3>
            <div class="flex flex-col gap-2 max-h-[260px] overflow-y-auto pr-1">
              <div
                v-for="evt in filteredEvents"
                :key="evt.id"
                class="p-2 rounded-sm border transition-all cursor-pointer flex flex-col gap-0.5"
                :class="selectedEvent?.id === evt.id 
                  ? 'border-action/80 bg-action/10' 
                  : 'border-border/60 bg-bg hover:border-control-border hover:bg-surface'"
                @click="handleSelectEvent(evt)"
              >
                <div class="flex items-center justify-between gap-2">
                  <span class="min-w-0 flex-1 font-body text-caption font-semibold text-text truncate">
                    {{ evt.localizedTitle || evt.title }}
                  </span>
                  <span class="shrink-0 text-[10px] text-action font-medium">
                    {{ evt.whenLocalized }}
                  </span>
                </div>
                <span class="text-[11px] text-text-muted">
                  {{ evt.neighborhood }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </main>
</template>
