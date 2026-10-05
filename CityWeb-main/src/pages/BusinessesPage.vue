<script setup lang="ts">
import { ref, computed } from 'vue'
import { businesses } from '../data/demo'
import BusinessCard from '../components/ui/BusinessCard.vue'
import { useI18n } from '../i18n'

const props = defineProps<{
  search?: string
}>()

const { locale } = useI18n()

const selectedCategory = ref<string>('all')

const categories = computed(() => [
  { id: 'all', labelRo: 'Toate afacerile', labelEn: 'All Businesses' },
  { id: 'cafe', labelRo: 'Cafenele', labelEn: 'Cafes' },
  { id: 'bookstore', labelRo: 'Librării', labelEn: 'Bookstores' },
  { id: 'bakery', labelRo: 'Brutării', labelEn: 'Bakeries' },
  { id: 'tech-hub', labelRo: 'Tech Hubs', labelEn: 'Tech Hubs' }
])

const filteredBusinesses = computed(() => {
  return businesses.filter(b => {
    // Category filter
    if (selectedCategory.value !== 'all' && b.category !== selectedCategory.value) {
      return false
    }

    // Search filter
    if (props.search && props.search.trim() !== '') {
      const q = props.search.toLowerCase()
      const matchName = b.name.toLowerCase().includes(q) || (b.localizedName && b.localizedName.toLowerCase().includes(q))
      const matchNeigh = b.neighborhood.toLowerCase().includes(q)
      const matchDesc = b.description.toLowerCase().includes(q)
      const matchTags = b.tags.some(t => t.toLowerCase().includes(q))
      return matchName || matchNeigh || matchDesc || matchTags
    }

    return true
  })
})
</script>

<template>
  <main class="mx-auto max-w-7xl px-4 py-6 md:px-6">
    <!-- Hero Header -->
    <div class="mb-6 flex flex-col gap-2 border-b border-control-border pb-5">
      <div class="flex items-center gap-2 text-xs font-semibold uppercase tracking-wider text-action">
        <span>Piața Bucharest</span>
        <span>•</span>
        <span>Afaceri Locale</span>
      </div>
      <h1 class="text-2xl font-bold text-text md:text-3xl">
        {{ locale === 'en' ? 'Local Businesses & Venues' : 'Afaceri și Spații Locale din București' }}
      </h1>
      <p class="max-w-2xl text-sm text-text-muted">
        {{ locale === 'en' 
          ? 'Discover local businesses anchored in Bucharest neighborhoods, hosting events, posting jobs, and powering the community graph.' 
          : 'Descoperă afacerile locale ancorate în cartierele din București, care găzduiesc evenimente, oferă locuri de muncă și conectează comunitatea.' 
        }}
      </p>
    </div>

    <!-- Category Filter Tabs -->
    <div
      class="mb-6 flex items-center gap-2 overflow-x-auto pb-2"
      role="tablist"
    >
      <button
        v-for="cat in categories"
        :id="`category-tab-${cat.id}`"
        :key="cat.id"
        type="button"
        class="shrink-0 rounded-full px-4 py-1.5 text-xs font-semibold transition-colors"
        :class="selectedCategory === cat.id
          ? 'bg-action text-white shadow-xs'
          : 'border border-control-border bg-surface text-text-muted hover:border-text-muted hover:text-text'"
        role="tab"
        :aria-selected="selectedCategory === cat.id"
        @click="selectedCategory = cat.id"
      >
        {{ locale === 'en' ? cat.labelEn : cat.labelRo }}
      </button>
    </div>

    <!-- Business Grid -->
    <div
      v-if="filteredBusinesses.length > 0"
      class="grid gap-6 sm:grid-cols-2 lg:grid-cols-3"
    >
      <BusinessCard
        v-for="business in filteredBusinesses"
        :key="business.id"
        :business="business"
      />
    </div>

    <!-- Empty State -->
    <div 
      v-else 
      class="flex flex-col items-center justify-center rounded-lg border border-dashed border-control-border bg-surface p-12 text-center"
    >
      <svg
        class="h-12 w-12 text-text-muted/50"
        viewBox="0 0 24 24"
        fill="currentColor"
      >
        <path d="M19 6h-2c0-2.76-2.24-5-5-5S7 3.24 7 6H5c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V8c0-1.1-.9-2-2-2zm-7-3c1.66 0 3 1.34 3 3H9c0-1.66 1.34-3 3-3zm7 17H5V8h14v12z" />
      </svg>
      <h3 class="mt-4 text-base font-semibold text-text">
        {{ locale === 'en' ? 'No businesses found' : 'Nicio afacere găsită' }}
      </h3>
      <p class="mt-1 text-xs text-text-muted">
        {{ locale === 'en' ? 'Try adjusting your category or search keywords.' : 'Încearcă să schimbi categoria sau termenii de căutare.' }}
      </p>
    </div>
  </main>
</template>
