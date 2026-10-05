<script setup lang="ts">
import Card from './Card.vue'

type Result = {
  label: string
  /** Whole-number percentage 0-100. */
  percent: number
}

const props = withDefaults(
  defineProps<{
    question: string
    scope?: string
    results: Result[]
    totalVotes?: number
  }>(),
  { scope: undefined, totalVotes: 0 },
)

/** Clamp so a bad percentage can never overflow the track. */
function width(percent: number) {
  const value = Number.isFinite(percent) ? percent : 0
  return `${Math.min(100, Math.max(0, value))}%`
}
</script>

<template>
  <Card
    category="sondaje"
    :interactive="false"
  >
    <div class="flex flex-col gap-3">
      <div>
        <h3 class="font-display text-heading-sm text-text">
          {{ props.question }}
        </h3>
        <p
          v-if="props.scope"
          class="mt-1 font-body text-caption text-text-muted"
        >
          {{ props.scope }}
        </p>
      </div>

      <ul class="flex flex-col gap-2">
        <li
          v-for="result in props.results"
          :key="result.label"
          class="flex flex-col gap-1"
        >
          <div class="flex items-baseline justify-between gap-2">
            <span class="font-ui text-caption text-text">{{ result.label }}</span>
            <span class="font-ui text-caption text-text-muted">{{ result.percent }}%</span>
          </div>
          <div
            class="h-2 w-full overflow-hidden rounded-full bg-surface"
            role="img"
            :aria-label="`${result.label}: ${result.percent}%`"
          >
            <div
              class="h-full rounded-full bg-linie-sondaje"
              :style="{ width: width(result.percent) }"
            />
          </div>
        </li>
      </ul>

      <p
        v-if="props.totalVotes > 0"
        class="border-t border-border pt-2 font-body text-caption text-text-muted"
      >
        {{ props.totalVotes }}
      </p>
    </div>
  </Card>
</template>
