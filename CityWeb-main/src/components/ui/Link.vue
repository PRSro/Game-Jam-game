<script setup lang="ts">
import { computed, useId } from 'vue'
import type { IconLike } from './types'

const props = withDefaults(
  defineProps<{
    to: string
    /** Visible text. Keep it descriptive: this is the only accessible name. */
    label: string
    /** Muted alternative wording announced alongside the label. */
    context?: string
    /** Trailing 24px line icon, decorative and hidden from assistive tech. */
    icon?: IconLike
    /** Underline on hover/focus. Reserve for links inside body copy. */
    underline?: boolean
    external?: boolean
  }>(),
  { context: undefined, icon: undefined, underline: true, external: false },
)

const uid = useId()
const contextId = computed(() => (props.context ? `link-${uid}-ctx` : undefined))
</script>

<template>
  <a
    :href="props.to"
    :aria-describedby="contextId"
    :target="props.external ? '_blank' : undefined"
    :rel="props.external ? 'noopener noreferrer' : undefined"
    class="inline-flex items-baseline gap-1 rounded-sm font-body font-medium text-action transition-colors duration-150 ease-standard hover:text-action-hover focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-focus-ring"
    :class="props.underline ? 'underline decoration-1 underline-offset-2' : 'no-underline hover:underline'"
  >
    <span>{{ props.label }}</span>
    <component
      :is="props.icon"
      v-if="props.icon"
      :size="18"
      class="self-center"
      aria-hidden="true"
    />
    <span
      v-if="props.external"
      class="sr-only"
    >(opens in a new tab)</span>
    <span
      v-if="props.context"
      :id="contextId"
      class="font-ui text-caption font-normal text-text-muted"
    >{{ props.context }}</span>
  </a>
</template>
