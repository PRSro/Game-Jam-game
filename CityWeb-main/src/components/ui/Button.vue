<script setup lang="ts">
import { computed } from 'vue'

type Variant = 'primary' | 'secondary' | 'text'
type Size = 'sm' | 'md' | 'lg'

const props = withDefaults(
  defineProps<{
    variant?: Variant
    size?: Size
    disabled?: boolean
    type?: 'button' | 'submit' | 'reset'
  }>(),
  { variant: 'primary', size: 'md', disabled: false, type: 'button' },
)

const VARIANT_CLASS: Record<Variant, string> = {
  primary:
    'bg-action text-action-text border-transparent hover:bg-action-hover active:bg-action-active',
  secondary:
    'bg-transparent text-text border-control-border hover:bg-surface active:bg-surface',
  text: 'bg-transparent text-action border-transparent hover:underline hover:underline-offset-4',
}

const SIZE_CLASS: Record<Size, string> = {
  sm: 'text-caption px-3 py-1.5 gap-1.5',
  md: 'text-body px-4 py-2 gap-2',
  lg: 'text-body-lg px-6 py-3 gap-2',
}

const classes = computed(() =>
  [
    'inline-flex items-center justify-center rounded-sm border font-ui',
    'transition-colors duration-150 ease-standard select-none',
    'disabled:opacity-50 disabled:cursor-not-allowed',
    VARIANT_CLASS[props.variant],
    SIZE_CLASS[props.size],
  ].join(' '),
)
</script>

<template>
  <button
    :type="props.type"
    :disabled="props.disabled"
    :class="classes"
  >
    <slot name="leading" />
    <slot />
    <slot name="trailing" />
  </button>
</template>
