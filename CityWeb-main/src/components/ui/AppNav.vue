<script setup lang="ts">
withDefaults(
  defineProps<{
    /** Visible name of the navigation landmark. */
    label: string
  }>(),
  {},
)
</script>

<template>
  <nav
    :aria-label="label"
    class="sticky top-0 z-50 w-full border-b border-border bg-bg"
  >
    <div class="mx-auto flex w-full max-w-7xl items-center gap-3 px-4 py-3 md:gap-4 md:px-6">
      <div class="flex shrink-0 items-center gap-3">
        <slot name="brand" />
      </div>

      <div class="min-w-0 flex-1">
        <slot name="search" />
      </div>

      <div class="flex shrink-0 items-center gap-2">
        <slot name="actions" />
      </div>
    </div>

    <div
      v-if="$slots.default"
      class="mx-auto w-full max-w-7xl px-4 md:px-6"
    >
      <slot />
    </div>
  </nav>
</template>
