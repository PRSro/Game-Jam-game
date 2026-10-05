<script setup lang="ts">
import { IconInfo } from './icons'
import type { IconLike } from './types'

withDefaults(
  defineProps<{
    title: string
    body?: string
    /** Optional decorative 24px line icon in currentColor. */
    icon?: IconLike
  }>(),
  { body: undefined, icon: IconInfo },
)
</script>

<template>
  <div
    class="flex flex-col items-center gap-2 rounded-sm border border-border bg-surface px-6 py-10 text-center"
    role="status"
  >
    <component
      :is="icon"
      :size="24"
      class="text-text-muted"
    />

    <p class="font-display text-heading-sm text-text">
      {{ title }}
    </p>

    <p
      v-if="body"
      class="max-w-prose font-body text-body text-text-muted"
    >
      {{ body }}
    </p>

    <div
      v-if="$slots.default"
      class="mt-2 flex flex-wrap items-center justify-center gap-2"
    >
      <slot />
    </div>
  </div>
</template>
