<script setup lang="ts">
import { computed } from 'vue'
import type { Business } from '../../data/demo'
import { useI18n } from '../../i18n'

const props = defineProps<{
  business: Business
}>()

const { locale } = useI18n()

const name = computed(() => {
  if (locale.value === 'en' && props.business.localizedName) {
    return props.business.localizedName
  }
  return props.business.name
})

const description = computed(() => {
  if (locale.value === 'en' && props.business.localizedDescription) {
    return props.business.localizedDescription
  }
  return props.business.description
})

const address = computed(() => {
  if (locale.value === 'en' && props.business.localizedAddress) {
    return props.business.localizedAddress
  }
  return props.business.address
})

const categoryLabel = computed(() => {
  const map: Record<string, { ro: string; en: string }> = {
    cafe: { ro: 'Cafenea', en: 'Coffee Shop' },
    bookstore: { ro: 'Librărie', en: 'Bookstore' },
    bakery: { ro: 'Brutărie', en: 'Bakery' },
    'tech-hub': { ro: 'Tech Hub', en: 'Tech Hub' },
    bistro: { ro: 'Bistro', en: 'Bistro' },
    services: { ro: 'Servicii', en: 'Services' }
  }
  const cat = map[props.business.category]
  if (!cat) return props.business.category
  return locale.value === 'en' ? cat.en : cat.ro
})
</script>

<template>
  <article 
    :id="`business-card-${business.id}`"
    class="flex flex-col justify-between rounded-lg border border-control-border bg-surface p-5 shadow-xs transition-all hover:border-action/40 hover:shadow-md"
  >
    <div class="flex flex-col gap-3">
      <!-- Header: Category, Neighborhood & Verification -->
      <div class="flex items-center justify-between gap-2">
        <span class="rounded-full bg-action/10 px-2.5 py-0.5 text-xs font-semibold text-action">
          {{ categoryLabel }}
        </span>

        <div class="flex items-center gap-2">
          <span 
            v-if="business.verified" 
            class="flex items-center gap-1 rounded-full bg-emerald-500/10 px-2 py-0.5 text-[11px] font-semibold text-emerald-600 dark:text-emerald-400"
            title="Afacere verificată Piața"
          >
            <svg
              class="h-3 w-3 fill-current"
              viewBox="0 0 24 24"
            >
              <path d="M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z" />
            </svg>
            <span>Verificat</span>
          </span>

          <span class="text-xs font-medium text-text-muted capitalize">
            {{ business.neighborhood }}
          </span>
        </div>
      </div>

      <!-- Title & Address -->
      <div>
        <h3 class="text-lg font-bold text-text">
          <RouterLink
            :to="`/afaceri/${business.id}`"
            class="hover:text-action hover:underline"
          >
            {{ name }}
          </RouterLink>
        </h3>
        <p class="mt-1 flex items-center gap-1 text-xs text-text-muted">
          <svg
            class="h-3.5 w-3.5 shrink-0"
            viewBox="0 0 24 24"
            fill="currentColor"
          >
            <path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z" />
          </svg>
          <span>{{ address }}</span>
        </p>
      </div>

      <!-- Description -->
      <p class="text-sm text-text-muted line-clamp-3">
        {{ description }}
      </p>

      <!-- Tags -->
      <div class="flex flex-wrap gap-1.5 pt-1">
        <span 
          v-for="tag in business.tags" 
          :key="tag"
          class="rounded-sm border border-control-border bg-bg/60 px-2 py-0.5 text-[11px] text-text-muted"
        >
          #{{ tag }}
        </span>
      </div>
    </div>

    <!-- Graph Outgoing Connections Footer -->
    <div class="mt-4 border-t border-control-border/60 pt-3 flex items-center justify-between text-xs text-text-muted">
      <div class="flex items-center gap-3">
        <span 
          v-if="business.connectedEvents.length > 0" 
          class="font-medium text-action"
        >
          {{ business.connectedEvents.length }} {{ locale === 'en' ? 'events' : 'evenimente' }}
        </span>
        <span 
          v-if="business.connectedJobs.length > 0" 
          class="font-medium text-emerald-600 dark:text-emerald-400"
        >
          {{ business.connectedJobs.length }} {{ locale === 'en' ? 'jobs' : 'joburi' }}
        </span>
        <span 
          v-if="business.connectedPolls.length > 0" 
          class="font-medium text-amber-600 dark:text-amber-400"
        >
          {{ business.connectedPolls.length }} {{ locale === 'en' ? 'polls' : 'sondaje' }}
        </span>
      </div>

      <RouterLink
        id="business-card-details-link"
        :to="`/afaceri/${business.id}`"
        class="font-semibold text-action hover:underline"
      >
        {{ locale === 'en' ? 'View details →' : 'Vezi detalii →' }}
      </RouterLink>
    </div>
  </article>
</template>
