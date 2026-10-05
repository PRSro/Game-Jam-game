<script setup lang="ts">
import Card from './Card.vue'
import { IconJoburi, IconPin } from './icons'

const props = withDefaults(
  defineProps<{
    title: string
    company: string
    where: string
    /** Plain text only - never a promise about pay that we cannot verify. */
    salaryLabel?: string
    commute?: string
    interactive?: boolean
  }>(),
  { salaryLabel: undefined, commute: undefined, interactive: true },
)
</script>

<template>
  <Card
    category="joburi"
    :interactive="props.interactive"
  >
    <div class="flex flex-col gap-2">
      <h3 class="font-display text-heading-sm text-text">
        {{ props.title }}
      </h3>

      <p class="flex items-start gap-2 font-ui text-body text-text">
        <IconJoburi
          :size="16"
          class="mt-1 shrink-0 text-accent"
        />
        <span>{{ props.company }}</span>
      </p>

      <p class="flex items-start gap-2 font-body text-caption text-text-muted">
        <IconPin
          :size="16"
          class="mt-px shrink-0"
        />
        <span>{{ props.where }}</span>
      </p>

      <p
        v-if="props.salaryLabel"
        class="font-body text-caption text-text-muted"
      >
        {{ props.salaryLabel }}
      </p>

      <p
        v-if="props.commute"
        class="border-t border-border pt-2 font-body text-caption text-text-muted"
      >
        {{ props.commute }}
      </p>
    </div>
  </Card>
</template>
