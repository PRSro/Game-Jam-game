<script setup lang="ts">
import { useRadio } from '../composables/useRadio'
import { useSoundEffects } from '../composables/useSoundEffects'

const {
  RADIO_STATIONS,
  currentStationId,
  currentStation,
  isPlaying,
  isLoading,
  isMuted,
  volume,
  errorMessage,
  togglePlay,
  setStation,
  toggleMute
} = useRadio()

const { isSoundMuted, toggleSoundMute } = useSoundEffects()

const handleStationChange = (id: string) => {
  setStation(id)
}
</script>

<template>
  <div 
    id="radio-player"
    class="w-full border-t border-control-border bg-surface/95 backdrop-blur-md transition-all duration-200"
    role="region"
    aria-label="Piața Live Radio Bucharest"
  >
    <div class="mx-auto flex max-w-7xl flex-col gap-2 px-4 py-2.5 md:px-6">
      <!-- Main Radio Controls Bar -->
      <div class="flex flex-wrap items-center justify-between gap-3">
        <!-- Live Radio Label & Current Station Info -->
        <div class="flex items-center gap-2.5 min-w-0">
          <div class="relative flex items-center justify-center">
            <span 
              class="inline-block h-3 w-3 rounded-full"
              :class="isPlaying ? 'bg-red-500 animate-pulse' : (isLoading ? 'bg-amber-400 animate-ping' : 'bg-text-muted/40')"
              aria-hidden="true"
            />
          </div>
          
          <div class="flex flex-col min-w-0">
            <div class="flex items-center gap-2">
              <span class="text-xs font-semibold uppercase tracking-wider text-text-muted">
                Piața Radio
              </span>
              <span 
                class="rounded-xs px-1.5 py-0.5 text-[10px] font-bold text-white uppercase"
                :style="{ backgroundColor: currentStation.accentColor }"
              >
                {{ currentStation.frequency }}
              </span>
            </div>
            <span class="truncate text-xs font-medium text-text">
              {{ currentStation.name }} – <span class="italic text-text-muted">{{ currentStation.tagline }}</span>
            </span>
          </div>
        </div>

        <!-- Station Selection Pills -->
        <div
          class="hidden items-center gap-1.5 sm:flex"
          role="radiogroup"
          aria-label="Select Radio Station"
        >
          <button
            v-for="station in RADIO_STATIONS"
            :id="`radio-station-${station.id}`"
            :key="station.id"
            type="button"
            class="rounded-full px-2.5 py-1 text-xs font-medium transition-colors focus-visible:outline-2"
            :class="currentStationId === station.id 
              ? 'bg-action text-white shadow-xs' 
              : 'border border-control-border bg-bg text-text-muted hover:border-text-muted hover:text-text'"
            :aria-checked="currentStationId === station.id"
            role="radio"
            @click="handleStationChange(station.id)"
          >
            {{ station.name }}
          </button>
        </div>

        <!-- Audio Action Buttons -->
        <div class="flex items-center gap-2">
          <!-- Play / Stop Button -->
          <button
            id="radio-play-btn"
            type="button"
            class="flex h-9 items-center justify-center gap-2 rounded-sm bg-action px-3.5 text-xs font-semibold text-white transition-opacity hover:opacity-90 disabled:opacity-50"
            :aria-label="isPlaying ? 'Stop radio stream' : 'Play radio stream'"
            @click="togglePlay"
          >
            <template v-if="isLoading">
              <svg
                class="h-4 w-4 animate-spin text-white"
                viewBox="0 0 24 24"
                fill="none"
              >
                <circle
                  class="opacity-25"
                  cx="12"
                  cy="12"
                  r="10"
                  stroke="currentColor"
                  stroke-width="4"
                />
                <path
                  class="opacity-75"
                  fill="currentColor"
                  d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"
                />
              </svg>
              <span>Conectare...</span>
            </template>
            <template v-else-if="isPlaying">
              <svg
                class="h-4 w-4 fill-current"
                viewBox="0 0 24 24"
              >
                <rect
                  x="6"
                  y="6"
                  width="12"
                  height="12"
                  rx="1"
                />
              </svg>
              <span>Stop</span>
            </template>
            <template v-else>
              <svg
                class="h-4 w-4 fill-current"
                viewBox="0 0 24 24"
              >
                <path d="M8 5v14l11-7z" />
              </svg>
              <span>Ascultă Live</span>
            </template>
          </button>

          <!-- Volume & Sound Effects Controls -->
          <div class="flex items-center gap-1">
            <!-- Mute Stream Toggle -->
            <button
              id="radio-mute-btn"
              type="button"
              class="rounded-sm border border-control-border bg-bg p-2 text-text-muted transition-colors hover:text-text"
              :aria-label="isMuted ? 'Unmute radio' : 'Mute radio'"
              :title="isMuted ? 'Unmute' : 'Mute'"
              @click="toggleMute"
            >
              <svg
                v-if="isMuted || volume === 0"
                class="h-4 w-4"
                viewBox="0 0 24 24"
                fill="currentColor"
              >
                <path d="M16.5 12c0-1.77-1.02-3.29-2.5-4.03v2.21l2.45 2.45c.03-.2.05-.41.05-.63zm2.5 0c0 .94-.2 1.82-.54 2.64l1.51 1.51C20.63 14.91 21 13.5 21 12c0-4.28-2.99-7.86-7-8.77v2.06c2.89.86 5 3.54 5 6.71zM4.27 3L3 4.27 7.73 9H3v6h4l5 5v-6.73l4.25 4.25c-.67.52-1.42.93-2.25 1.18v2.06c1.38-.31 2.63-.95 3.69-1.81L19.73 21 21 19.73 4.27 3zM12 4L9.91 6.09 12 8.18V4z" />
              </svg>
              <svg
                v-else
                class="h-4 w-4"
                viewBox="0 0 24 24"
                fill="currentColor"
              >
                <path d="M3 9v6h4l5 5V4L7 9H3zm13.5 3c0-1.77-1.02-3.29-2.5-4.03v8.05c1.48-.73 2.5-2.25 2.5-4.02zM14 3.23v2.06c2.89.86 5 3.54 5 6.71s-2.11 5.85-5 6.71v2.06c4.01-.91 7-4.49 7-8.77s-2.99-7.86-7-8.77z" />
              </svg>
            </button>

            <!-- SFX UI Sound Effects Toggle -->
            <button
              id="sfx-toggle-btn"
              type="button"
              class="rounded-sm border p-2 text-xs transition-colors"
              :class="!isSoundMuted ? 'border-action bg-action/10 text-action' : 'border-control-border bg-bg text-text-muted'"
              :aria-label="isSoundMuted ? 'Enable UI sound effects' : 'Disable UI sound effects'"
              :title="isSoundMuted ? 'Enable sound effects' : 'Disable sound effects'"
              @click="toggleSoundMute"
            >
              <span class="font-bold">SFX</span>
            </button>
          </div>
        </div>
      </div>

      <!-- Mobile Station Selection Pills -->
      <div
        class="flex items-center gap-1.5 overflow-x-auto pb-1 sm:hidden"
        role="radiogroup"
        aria-label="Select Radio Station Mobile"
      >
        <button
          v-for="station in RADIO_STATIONS"
          :key="station.id"
          type="button"
          class="shrink-0 rounded-full px-2.5 py-1 text-xs font-medium transition-colors"
          :class="currentStationId === station.id 
            ? 'bg-action text-white' 
            : 'border border-control-border bg-bg text-text-muted'"
          @click="handleStationChange(station.id)"
        >
          {{ station.name }}
        </button>
      </div>

      <!-- Error Toast Notification -->
      <div 
        v-if="errorMessage"
        id="radio-error-banner"
        class="flex items-center justify-between rounded-sm border border-red-500/30 bg-red-500/10 px-3 py-1.5 text-xs text-red-600 dark:text-red-400"
        role="alert"
      >
        <div class="flex items-center gap-2">
          <svg
            class="h-4 w-4 shrink-0"
            viewBox="0 0 24 24"
            fill="currentColor"
          >
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z" />
          </svg>
          <span class="font-medium">{{ errorMessage }}</span>
        </div>
        <button
          type="button"
          class="text-red-600 hover:text-red-800 dark:text-red-400 dark:hover:text-red-200"
          aria-label="Dismiss error"
          @click="errorMessage = null"
        >
          &times;
        </button>
      </div>
    </div>
  </div>
</template>
