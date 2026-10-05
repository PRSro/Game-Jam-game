<script setup lang="ts">
import { useRoute } from 'vue-router'
import NavBar from './NavBar.vue'
import ThemeToggle from './ThemeToggle.vue'
import { useI18n } from '../i18n'

defineProps<{
  search: string
}>()

const emit = defineEmits<{
  (e: 'update:search', value: string): void
}>()

const { t } = useI18n()
const route = useRoute()
const calendarActive = () => route.path === '/calendar'
</script>

<template>
  <header class="sticky top-0 z-50 w-full border-b border-border bg-bg">
    <div class="mx-auto flex w-full max-w-7xl flex-col gap-2 px-4 py-3 md:px-6">
      <div class="flex items-center gap-3 md:gap-4">
        <RouterLink
          to="/evenimente"
          class="flex shrink-0 flex-col leading-none focus-visible:rounded-sm"
        >
          <span class="font-display text-heading-sm text-text">
            {{ t('brand.name') }}
          </span>
          <span class="mt-1 hidden text-xs text-text-muted lg:block">
            {{ t('brand.tagline') }}
          </span>
        </RouterLink>

        <div class="relative min-w-0 flex-1">
          <svg
            class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-text-muted"
            width="18"
            height="18"
            viewBox="0 0 24 24"
            fill="currentColor"
            aria-hidden="true"
          >
            <path d="M10 2a8 8 0 105.3 14l4.4 4.4 1.4-1.4-4.4-4.4A8 8 0 0010 2zm0 2a6 6 0 110 12 6 6 0 010-12z" />
          </svg>
          <input
            :value="search"
            type="search"
            :placeholder="t('search.placeholder')"
            :aria-label="t('search.label')"
            class="w-full rounded-sm border border-control-border bg-surface py-2.5 pl-10 pr-3 text-sm text-text placeholder:text-text-muted focus:border-text-muted"
            @input="emit('update:search', ($event.target as HTMLInputElement).value)"
          >
        </div>

        <RouterLink
          to="/calendar"
          class="shrink-0 rounded-sm border p-2.5 transition-colors duration-150 ease-standard"
          :class="calendarActive()
            ? 'border-action bg-surface text-action'
            : 'border-control-border bg-surface text-text-muted hover:border-text-muted hover:text-text'"
          :aria-current="calendarActive() ? 'page' : undefined"
          :aria-label="t('calendar.toggle')"
          :title="t('calendar.toggle')"
        >
          <svg
            width="20"
            height="20"
            viewBox="0 0 24 24"
            fill="currentColor"
            aria-hidden="true"
          >
            <path d="M7 2v2H5a2 2 0 00-2 2v14a2 2 0 002 2h14a2 2 0 002-2V6a2 2 0 00-2-2h-2V2h-2v2H9V2H7zm12 8v10H5V10h14zM5 8V6h14v2H5z" />
          </svg>
        </RouterLink>

        <ThemeToggle />
      </div>

      <NavBar />
    </div>
  </header>
</template>