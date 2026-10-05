<script setup lang="ts">
withDefaults(
  defineProps<{
    /** Any element is allowed; block-level usage is the common case. */
    as?: string
    width?: string
    height?: string
    rounded?: 'sm' | 'md' | 'full'
  }>(),
  { as: 'div', width: '100%', height: '1rem', rounded: 'sm' },
)

const ROUNDED = {
  sm: 'rounded-sm',
  md: 'rounded-md',
  full: 'rounded-full',
} as const
</script>

<template>
  <component
    :is="as"
    class="animate-pulse bg-surface motion-reduce:animate-none"
    :class="ROUNDED[rounded]"
    :style="{ width, height }"
    aria-hidden="true"
  />
</template>
