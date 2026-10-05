<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(
  defineProps<{
    name: string
    neighborhood?: string
    /** Rendered as the avatar tile; falls back to initials from the name. */
    initials?: string
    interactive?: boolean
  }>(),
  { neighborhood: undefined, initials: undefined, interactive: false },
)

function initialsOf(name: string) {
  return name
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part.charAt(0).toUpperCase())
    .join('')
}

const tile = computed(() => props.initials ?? initialsOf(props.name))
</script>

<template>
  <component
    :is="props.interactive ? 'button' : 'div'"
    :type="props.interactive ? 'button' : undefined"
    class="inline-flex max-w-full items-center gap-2 rounded-sm border border-border bg-surface text-left transition-colors duration-150 ease-standard"
    :class="props.interactive ? 'cursor-pointer px-2 py-1 hover:border-action active:bg-action-active' : 'py-1'"
  >
    <span
      class="flex h-8 w-8 shrink-0 items-center justify-center rounded-sm border border-control-border bg-surface font-display text-caption text-action"
      aria-hidden="true"
    >
      {{ tile }}
    </span>

    <span class="flex min-w-0 flex-col">
      <span class="truncate font-ui text-body text-text">{{ props.name }}</span>
      <span
        v-if="props.neighborhood"
        class="truncate font-body text-caption text-text-muted"
      >
        {{ props.neighborhood }}
      </span>
    </span>
  </component>
</template>
