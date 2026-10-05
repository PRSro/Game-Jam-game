<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useI18n } from '../i18n'

type Theme = 'light' | 'dark'

const STORAGE_KEY = 'piata.theme'

const { t } = useI18n()

/** null means "no explicit choice yet" - the OS preference decides. */
const chosen = ref<Theme | null>(null)
const systemDark = ref(false)

let media: MediaQueryList | null = null

const isDark = computed(() =>
  chosen.value !== null ? chosen.value === 'dark' : systemDark.value,
)

// The label states the ACTION, so it must not say "dark theme" while dark is active.
const label = computed(() =>
  isDark.value ? t('theme.toLight') : t('theme.toDark'),
)

function readStored(): Theme | null {
  try {
    const value = localStorage.getItem(STORAGE_KEY)
    return value === 'light' || value === 'dark' ? value : null
  } catch {
    return null
  }
}

function toggle() {
  const next: Theme = isDark.value ? 'light' : 'dark'
  chosen.value = next
  document.documentElement.dataset.theme = next
  try {
    localStorage.setItem(STORAGE_KEY, next)
  } catch {
    // Storage blocked: the theme still applies for this session.
  }
}

function onSystemChange(event: MediaQueryListEvent) {
  systemDark.value = event.matches
}

onMounted(() => {
  chosen.value = readStored()
  media = window.matchMedia('(prefers-color-scheme: dark)')
  systemDark.value = media.matches
  media.addEventListener('change', onSystemChange)
})

onUnmounted(() => {
  media?.removeEventListener('change', onSystemChange)
})
</script>

<template>
  <button
    type="button"
    class="rounded-sm border border-control-border bg-surface p-2 text-text transition-colors duration-150 ease-standard hover:bg-action-hover hover:text-action-text active:bg-action-active"
    :aria-label="label"
    :title="label"
    @click="toggle"
  >
    <svg
      v-if="isDark"
      width="24"
      height="24"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="1.5"
      stroke-linecap="round"
      aria-hidden="true"
    >
      <circle
        cx="12"
        cy="12"
        r="4"
      />
      <path d="M12 2v2M12 20v2M2 12h2M20 12h2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M19.1 4.9l-1.4 1.4M6.3 17.7l-1.4 1.4" />
    </svg>
    <svg
      v-else
      width="24"
      height="24"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="1.5"
      stroke-linecap="round"
      stroke-linejoin="round"
      aria-hidden="true"
    >
      <path d="M21 12.8A9 9 0 1111.2 3a7 7 0 009.8 9.8z" />
    </svg>
  </button>
</template>
