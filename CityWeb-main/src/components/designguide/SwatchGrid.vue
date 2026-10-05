<script setup lang="ts">
import type { Swatch } from './palette'

defineProps<{
  title: string
  note: string
  swatches: Swatch[]
}>()
</script>

<template>
  <section class="border-t border-border pt-6">
    <h3 class="font-display text-heading-sm text-text">
      {{ title }}
    </h3>
    <p class="mt-1 max-w-prose font-body text-body text-text-muted">
      {{ note }}
    </p>

    <ul class="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
      <li
        v-for="swatch in swatches"
        :key="swatch.token"
        class="overflow-hidden rounded-sm border border-border bg-surface"
      >
        <div
          class="flex h-20 items-end justify-between gap-2 p-3"
          :class="[swatch.fill, swatch.on]"
        >
          <span class="font-ui text-caption font-medium">{{ swatch.name }}</span>
          <span class="font-mono text-caption">{{ swatch.hex }}</span>
        </div>

        <div class="p-3">
          <p class="font-mono text-caption text-text-muted">
            {{ swatch.token }}
          </p>
          <p class="mt-1 font-body text-caption text-text">
            {{ swatch.role }}
          </p>
          <p class="mt-1 font-ui text-caption text-text-muted">
            pe fundal:
            <span class="font-mono">
              {{ swatch.onBg.light === null ? '—' : `${swatch.onBg.light.toFixed(2)}:1` }}
            </span>
            clar ·
            <span class="font-mono">
              {{ swatch.onBg.dark === null ? '—' : `${swatch.onBg.dark.toFixed(2)}:1` }}
            </span>
            întuneric
          </p>
          <p
            v-if="swatch.decorationOnly"
            class="mt-1 font-ui text-caption font-medium text-danger"
          >
            Decorative only — never use for text.
          </p>
        </div>
      </li>
    </ul>
  </section>
</template>
