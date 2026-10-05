<script setup lang="ts">
import Card from './Card.vue'
import { IconCalendar, IconPin, IconOameni } from './icons'

const props = withDefaults(
  defineProps<{
    title: string
    when: string
    where: string
    neighborhood?: string
    rsvpCount?: number
    interactive?: boolean
  }>(),
  { neighborhood: undefined, rsvpCount: 0, interactive: true },
)
</script>

<template>
  <Card
    category="evenimente"
    :interactive="props.interactive"
  >
    <div class="flex flex-col gap-2">
      <h3 class="font-display text-heading-sm text-text">
        {{ props.title }}
      </h3>

      <p class="flex items-start gap-2 font-body text-caption text-text-muted">
        <IconCalendar
          :size="16"
          class="mt-px shrink-0"
        />
        <span>{{ props.when }}</span>
      </p>

      <p class="flex items-start gap-2 font-body text-caption text-text-muted">
        <IconPin
          :size="16"
          class="mt-px shrink-0"
        />
        <span>{{ props.where }}</span>
      </p>

      <p
        v-if="props.neighborhood"
        class="font-body text-caption text-text-muted"
      >
        {{ props.neighborhood }}
      </p>

      <p
        v-if="props.rsvpCount > 0"
        class="flex items-center gap-2 border-t border-border pt-2 font-ui text-caption text-text"
      >
        <IconOameni
          :size="16"
          class="shrink-0 text-accent"
        />
        <span>{{ props.rsvpCount }}</span>
      </p>
    </div>
  </Card>
</template>
