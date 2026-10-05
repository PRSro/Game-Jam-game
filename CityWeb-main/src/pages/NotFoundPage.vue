<script setup lang="ts">
import { useI18n } from '../i18n/index'

const { t } = useI18n()
</script>

<template>
  <main class="mx-auto w-full max-w-7xl px-4 py-24 md:px-6">
    <div class="mx-auto max-w-md text-center">
      <h1 class="text-2xl font-bold text-text">
        {{ t('notFound.title') }}
      </h1>
      <p class="mt-2 text-sm text-text-muted">
        {{ t('notFound.body') }}
      </p>
      <RouterLink
        to="/evenimente"
        class="mt-6 inline-block rounded-sm bg-action px-4 py-2.5 text-sm font-medium text-action-text transition-colors hover:bg-action-hover"
      >
        {{ t('notFound.cta') }}
      </RouterLink>
    </div>
  </main>
</template>