<script setup lang="ts">
import { computed } from 'vue'
import type { Category } from './types'


const props = withDefaults(
  defineProps<{
    category: Category
    label: string
    /** A tag is small by definition - never let it become a block. */
    size?: 'sm' | 'md'
  }>(),
  { size: 'sm' },
)

/**
 * Metro-line colours as category markers only. The text colour on top is
 * fixed per category: ink on the light fills (yellow, orange), white on the
 * intense ones (blue, red, green). Never white on yellow or orange.
 */
const CATEGORY_CLASS: Record<Category, string> = {
  evenimente: 'bg-linie-evenimente text-linie-on-clar',
  joburi: 'bg-linie-joburi text-linie-on-intens',
  trafic: 'bg-linie-trafic text-linie-on-intens',
  oameni: 'bg-linie-oameni text-linie-on-intens',
  sondaje: 'bg-linie-sondaje text-linie-on-clar',
}

const SIZE_CLASS = {
  sm: 'px-2 py-0.5 text-caption',
  md: 'px-2.5 py-1 text-body',
} as const

const classes = computed(() =>
  [
    'inline-flex max-w-full items-center gap-1.5 rounded-sm font-ui font-medium',
    SIZE_CLASS[props.size],
    CATEGORY_CLASS[props.category],
  ].join(' '),
)
</script>

<template>
  <span :class="classes">
    <slot name="leading" />
    <span class="truncate">{{ props.label }}</span>
  </span>
</template>
