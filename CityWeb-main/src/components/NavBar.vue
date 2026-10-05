<script setup lang="ts">
import { useRoute } from "vue-router";
import { useI18n, type Locale } from "../i18n";

const { t, locale, setLocale } = useI18n();
const route = useRoute();

const links = [
  { to: "/evenimente", key: "nav.events" },
  { to: "/calendar", key: "nav.calendar" },
  { to: "/harta", key: "nav.harta" },
];

const locales: Locale[] = ["ro", "en"];

const isActive = (to: string) => route.path === to;
</script>

<template>
  <nav
    :aria-label="t('nav.label')"
    class="flex items-center gap-1 overflow-x-auto"
  >
    <RouterLink
      v-for="link in links"
      :key="link.to"
      :to="link.to"
      :aria-current="isActive(link.to) ? 'page' : undefined"
      class="shrink-0 rounded-sm border-b-2 px-3 py-1.5 text-sm font-medium transition-colors duration-150 ease-standard"
      :class="isActive(link.to)
        ? 'border-action text-action'
        : 'border-transparent text-text-muted hover:text-text'"
    >
      {{ t(link.key) }}
    </RouterLink>

    <span class="ml-auto flex shrink-0 items-center gap-1 pl-2">
      <button
        v-for="option in locales"
        :key="option"
        type="button"
        class="rounded-sm border px-2 py-1 text-xs font-semibold uppercase transition-colors duration-150 ease-standard"
        :class="locale === option
          ? 'border-control-border bg-surface text-text'
          : 'border-transparent text-text-muted hover:text-text'"
        :aria-pressed="locale === option"
        :aria-label="t('nav.locale')"
        @click="setLocale(option)"
      >
        {{ option }}
      </button>
    </span>
  </nav>
</template>
