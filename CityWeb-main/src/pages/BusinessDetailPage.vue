<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, RouterLink } from 'vue-router'
import { businesses, events, jobs, polls } from '../data/demo'
import { useI18n } from '../i18n'
import EventCard from '../components/ui/EventCard.vue'
import JobCard from '../components/ui/JobCard.vue'
import PollCard from '../components/ui/PollCard.vue'

const route = useRoute()
const { locale } = useI18n()

const business = computed(() => {
  const id = String(route.params.id)
  return businesses.find(b => b.id === id)
})

const name = computed(() => {
  if (!business.value) return ''
  return locale.value === 'en' && business.value.localizedName
    ? business.value.localizedName
    : business.value.name
})

const description = computed(() => {
  if (!business.value) return ''
  return locale.value === 'en' && business.value.localizedDescription
    ? business.value.localizedDescription
    : business.value.description
})

const address = computed(() => {
  if (!business.value) return ''
  return locale.value === 'en' && business.value.localizedAddress
    ? business.value.localizedAddress
    : business.value.address
})

const categoryLabel = computed(() => {
  if (!business.value) return ''
  const map: Record<string, { ro: string; en: string }> = {
    cafe: { ro: 'Cafenea', en: 'Coffee Shop' },
    bookstore: { ro: 'Librărie', en: 'Bookstore' },
    bakery: { ro: 'Brutărie', en: 'Bakery' },
    'tech-hub': { ro: 'Tech Hub', en: 'Tech Hub' },
    bistro: { ro: 'Bistro', en: 'Bistro' },
    services: { ro: 'Servicii', en: 'Services' }
  }
  const cat = map[business.value.category]
  if (!cat) return business.value.category
  return locale.value === 'en' ? cat.en : cat.ro
})

const connectedEventsList = computed(() => {
  if (!business.value) return []
  return events.filter(e => business.value!.connectedEvents.includes(e.id))
})

const connectedJobsList = computed(() => {
  if (!business.value) return []
  return jobs.filter(j => business.value!.connectedJobs.includes(j.id))
})

const connectedPollsList = computed(() => {
  if (!business.value) return []
  return polls.filter(p => business.value!.connectedPolls.includes(p.id))
})
</script>

<template>
  <main class="mx-auto max-w-7xl px-4 py-6 md:px-6">
    <!-- Back Button -->
    <div class="mb-4">
      <RouterLink
        to="/afaceri"
        class="inline-flex items-center gap-1.5 text-xs font-semibold text-action hover:underline"
      >
        <svg
          class="h-4 w-4"
          viewBox="0 0 24 24"
          fill="currentColor"
        >
          <path d="M20 11H7.83l5.59-5.59L12 4l-8 8 8 8 1.41-1.41L7.83 13H20v-2z" />
        </svg>
        <span>{{ locale === 'en' ? 'Back to Local Businesses' : 'Înapoi la Afaceri Locale' }}</span>
      </RouterLink>
    </div>

    <!-- Business Detail Container -->
    <div
      v-if="business"
      id="business-detail-content"
      class="flex flex-col gap-8"
    >
      <!-- Business Header Card -->
      <section class="rounded-xl border border-control-border bg-surface p-6 shadow-xs">
        <div class="flex flex-wrap items-start justify-between gap-4">
          <div class="flex flex-col gap-2 min-w-0">
            <div class="flex items-center gap-2">
              <span class="rounded-full bg-action/10 px-3 py-1 text-xs font-bold text-action">
                {{ categoryLabel }}
              </span>
              <span 
                v-if="business.verified"
                class="flex items-center gap-1 rounded-full bg-emerald-500/10 px-2.5 py-0.5 text-xs font-semibold text-emerald-600 dark:text-emerald-400"
              >
                <svg
                  class="h-3.5 w-3.5 fill-current"
                  viewBox="0 0 24 24"
                >
                  <path d="M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z" />
                </svg>
                <span>Afacere Verificată</span>
              </span>
              <span class="text-xs font-semibold uppercase tracking-wider text-text-muted">
                {{ business.neighborhood }}
              </span>
            </div>

            <h1 class="text-2xl font-extrabold text-text md:text-3xl">
              {{ name }}
            </h1>

            <p class="flex items-center gap-1.5 text-sm text-text-muted">
              <svg
                class="h-4 w-4 shrink-0 text-action"
                viewBox="0 0 24 24"
                fill="currentColor"
              >
                <path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z" />
              </svg>
              <span>{{ address }}</span>
            </p>
          </div>

          <!-- Contact & Social Buttons -->
          <div class="flex flex-wrap items-center gap-2">
            <!-- Facebook Profile Button -->
            <a
              v-if="business.facebookUrl"
              id="business-facebook-btn"
              :href="business.facebookUrl"
              target="_blank"
              rel="noopener noreferrer"
              class="inline-flex items-center gap-2 rounded-md bg-[#1877F2] px-4 py-2 text-xs font-bold text-white transition-opacity hover:opacity-90 shadow-xs"
            >
              <svg
                class="h-4 w-4 fill-current"
                viewBox="0 0 24 24"
              >
                <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z" />
              </svg>
              <span>Pagina Facebook</span>
            </a>

            <!-- Website Link Button -->
            <a
              v-if="business.websiteUrl"
              :href="business.websiteUrl"
              target="_blank"
              rel="noopener noreferrer"
              class="inline-flex items-center gap-2 rounded-md border border-control-border bg-surface px-4 py-2 text-xs font-bold text-text hover:border-text-muted shadow-xs"
            >
              <svg
                class="h-4 w-4 text-text-muted"
                viewBox="0 0 24 24"
                fill="currentColor"
              >
                <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-1 17.93c-3.95-.49-7-3.85-7-7.93 0-.62.08-1.21.21-1.79L9 15v1c0 1.1.9 2 2 2v1.93zm6.9-2.54c-.26-.81-1-1.39-1.9-1.39h-1v-3c0-.55-.45-1-1-1H8v-2h2c.55 0 1-.45 1-1V7h2c1.1 0 2-.9 2-2v-.41c2.93 1.19 5 4.06 5 7.41 0 2.08-.8 3.97-2.1 5.39z" />
              </svg>
              <span>Website</span>
            </a>

            <!-- Phone Button -->
            <a
              v-if="business.phone"
              :href="`tel:${business.phone}`"
              class="inline-flex items-center gap-2 rounded-md border border-control-border bg-surface px-4 py-2 text-xs font-bold text-text hover:border-text-muted shadow-xs"
            >
              <svg
                class="h-4 w-4 text-text-muted"
                viewBox="0 0 24 24"
                fill="currentColor"
              >
                <path d="M6.62 10.79c1.44 2.83 3.76 5.14 6.59 6.59l2.2-2.2c.27-.27.67-.36 1.02-.24 1.12.37 2.33.57 3.57.57.55 0 1 .45 1 1V20c0 .55-.45 1-1 1-9.39 0-17-7.61-17-17 0-.55.45-1 1-1h3.5c.55 0 1 .45 1 1 0 1.25.2 2.45.57 3.57.11.35.03.74-.25 1.02l-2.2 2.2z" />
              </svg>
              <span>{{ business.phone }}</span>
            </a>
          </div>
        </div>

        <hr class="my-5 border-control-border/60">

        <!-- Description & Tags -->
        <div class="flex flex-col gap-3">
          <h2 class="text-sm font-bold uppercase tracking-wider text-text-muted">
            {{ locale === 'en' ? 'About this Business' : 'Despre această Afacere' }}
          </h2>
          <p class="text-base text-text leading-relaxed">
            {{ description }}
          </p>

          <div class="flex flex-wrap gap-2 pt-2">
            <span 
              v-for="tag in business.tags" 
              :key="tag"
              class="rounded-md border border-control-border bg-bg/80 px-2.5 py-1 text-xs font-medium text-text-muted"
            >
              #{{ tag }}
            </span>
          </div>
        </div>
      </section>

      <!-- Graph Section: Hosted Events -->
      <section class="flex flex-col gap-4">
        <div class="flex items-center justify-between border-b border-control-border pb-2">
          <h2 class="text-lg font-bold text-text flex items-center gap-2">
            <span>{{ locale === 'en' ? 'Hosted Events' : 'Evenimente Găzduite' }}</span>
            <span class="rounded-full bg-action/10 px-2.5 py-0.5 text-xs text-action">
              {{ connectedEventsList.length }}
            </span>
          </h2>
        </div>

        <div
          v-if="connectedEventsList.length > 0"
          class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3"
        >
          <EventCard
            v-for="evt in connectedEventsList"
            :key="evt.id"
            :title="locale === 'en' && evt.localizedTitle ? evt.localizedTitle : evt.title"
            :when="evt.whenLocalized"
            :where="evt.neighborhood"
            :neighborhood="evt.neighborhood"
            :rsvp-count="evt.rsvpCount"
          />
        </div>
        <p
          v-else
          class="text-sm italic text-text-muted"
        >
          {{ locale === 'en' ? 'No active events currently hosted.' : 'Niciun eveniment activ programat în prezent.' }}
        </p>
      </section>

      <!-- Graph Section: Connected Job Openings -->
      <section class="flex flex-col gap-4">
        <div class="flex items-center justify-between border-b border-control-border pb-2">
          <h2 class="flex items-center gap-2 text-lg font-bold text-text">
            <span>{{ locale === 'en' ? 'Job Openings' : 'Locuri de Muncă Disponibile' }}</span>
            <span class="rounded-full bg-emerald-500/10 px-2.5 py-0.5 text-xs text-emerald-600 dark:text-emerald-400">
              {{ connectedJobsList.length }}
            </span>
          </h2>
        </div>

        <div
          v-if="connectedJobsList.length > 0"
          class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3"
        >
          <JobCard
            v-for="job in connectedJobsList"
            :key="job.id"
            :title="job.title"
            :company="job.company"
            :where="job.neighborhood"
            :salary-label="job.salaryPlaceholder"
            :commute="job.commuteByMetro"
          />
        </div>
        <p
          v-else
          class="text-sm italic text-text-muted"
        >
          {{ locale === 'en' ? 'No active job openings at the moment.' : 'Niciun loc de muncă disponibil în prezent.' }}
        </p>
      </section>

      <!-- Graph Section: Connected Polls -->
      <section
        v-if="connectedPollsList.length > 0"
        class="flex flex-col gap-4"
      >
        <div class="flex items-center justify-between border-b border-control-border pb-2">
          <h2 class="flex items-center gap-2 text-lg font-bold text-text">
            <span>{{ locale === 'en' ? 'Community Polls' : 'Sondaje Comunitare' }}</span>
          </h2>
        </div>

        <div class="grid gap-4 sm:grid-cols-2">
          <PollCard
            v-for="poll in connectedPollsList"
            :key="poll.id"
            :question="locale === 'en' && poll.localizedQuestion ? poll.localizedQuestion : poll.question"
            :scope="poll.place"
            :results="poll.optionLabels.map((label, idx) => ({ label, percent: poll.results[idx] || 0 }))"
            :total-votes="poll.votes"
          />
        </div>
      </section>
    </div>

    <!-- Fallback State: Business Not Found -->
    <div 
      v-else 
      id="business-not-found"
      class="flex flex-col items-center justify-center rounded-xl border border-dashed border-control-border bg-surface p-12 text-center"
    >
      <h2 class="text-xl font-bold text-text">
        {{ locale === 'en' ? 'Business Not Found' : 'Afacerea nu a fost găsită' }}
      </h2>
      <p class="mt-2 text-sm text-text-muted">
        {{ locale === 'en' ? 'The business listing you are looking for does not exist or has been removed.' : 'Afacerea pe care o cauți nu există sau a fost eliminată.' }}
      </p>
      <RouterLink
        to="/afaceri"
        class="mt-4 rounded-md bg-action px-4 py-2 text-xs font-semibold text-white hover:opacity-90"
      >
        {{ locale === 'en' ? 'Return to Business Directory' : 'Înapoi la Catalogul de Afaceri' }}
      </RouterLink>
    </div>
  </main>
</template>
