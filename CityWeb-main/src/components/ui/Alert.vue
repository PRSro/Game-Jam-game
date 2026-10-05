<script setup lang="ts">
import { computed } from 'vue'
import { IconAlert, IconCheck, IconClose, IconInfo } from './icons'
import type { Tone } from './types'


const props = withDefaults(
  defineProps<{
    tone?: Tone
    title?: string
    dismissible?: boolean
  }>(),
  { tone: 'info', title: undefined, dismissible: false },
)

const emit = defineEmits<{ (e: 'dismiss'): void }>()

/**
 * The 4px left stripe carries the tone. The body text stays on --text so it
 * always clears 4.5:1, and every alert has a visible label, so the colour is
 * never the only signal. Traffic disruptions use linie-trafic.
 */
const STRIPE: Record<Tone, string> = {
  info: 'border-l-linie-joburi',
  success: 'border-l-accent',
  warning: 'border-l-ocru',
  danger: 'border-l-linie-trafic',
}

const ICON: Record<Tone, typeof IconInfo> = {
  info: IconInfo,
  success: IconCheck,
  warning: IconAlert,
  danger: IconAlert,
}

const icon = computed(() => ICON[props.tone])
</script>

<template>
  <div
    :class="[
      'flex items-start gap-3 rounded-sm border border-l-4 border-border bg-surface p-4',
      STRIPE[props.tone],
    ]"
    :role="props.tone === 'danger' ? 'alert' : 'status'"
  >
    <component
      :is="icon"
      :size="20"
      class="mt-0.5 shrink-0"
      :class="props.tone === 'danger' ? 'text-danger' : 'text-accent'"
    />

    <div class="min-w-0 flex-1">
      <p
        v-if="props.title"
        class="font-ui text-body font-semibold text-text"
      >
        {{ props.title }}
      </p>
      <div class="font-body text-body text-text-muted">
        <slot />
      </div>
    </div>

    <button
      v-if="props.dismissible"
      type="button"
      class="shrink-0 rounded-sm p-1 text-text-muted transition-colors duration-150 ease-standard hover:bg-action-hover hover:text-action-text active:bg-action-active"
      aria-label="Închide"
      @click="emit('dismiss')"
    >
      <IconClose :size="18" />
    </button>
  </div>
</template>
