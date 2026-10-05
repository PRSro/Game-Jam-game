<script setup lang="ts">
import { computed } from 'vue'
import type { Category } from './types'

const props = withDefaults(
  defineProps<{
    /** When set, renders the 3-4px metro-line stripe down the left edge. */
    category?: Category
    interactive?: boolean
    padded?: boolean
  }>(),
  { category: undefined, interactive: false, padded: true },
)

const STRIPE: Record<Category, string> = {
  evenimente: 'border-l-linie-evenimente',
  joburi: 'border-l-linie-joburi',
  trafic: 'border-l-linie-trafic',
  oameni: 'border-l-linie-oameni',
  sondaje: 'border-l-linie-sondaje',
}

const classes = computed(() =>
  [
    'block rounded-sm border bg-surface transition-colors duration-150 ease-standard',
    props.category ? 'border-l-4' : 'border-l',
    props.category ? STRIPE[props.category] : 'border-border',
    props.padded ? 'p-4' : '',
    props.interactive
      ? 'cursor-pointer hover:border-action active:bg-action-active'
      : '',
  ]
    .filter(Boolean)
    .join(' '),
)
</script>

<template>
  <component
    :is="props.interactive ? 'button' : 'div'"
    :type="props.interactive ? 'button' : undefined"
    :class="[classes, 'text-left']"
  >
    <slot />
  </component>
</template>
