<script setup lang="ts">
import { ref, computed } from 'vue'
import { useI18n } from '../i18n/index'

type CalEvent = {
  id: string
  title: string
  localizedTitle?: string
  when: string
  neighborhood?: string
  tags?: string[]
  rsvpCount?: number
}

const props = defineProps<{
  events: CalEvent[]
  selectedDate: string | null
}>()

const emit = defineEmits<{
  (e: 'select-date', value: string | null): void
}>()

const { t, tm, intlLocale } = useI18n()

const weekdays = computed(() => tm('cal.weekdays'))
const months = computed(() => tm('cal.months'))

const today = new Date()
const firstEvent = props.events.length ? new Date(props.events[0].when) : today

const viewYear = ref(firstEvent.getFullYear())
const viewMonth = ref(firstEvent.getMonth())

const monthLabel = computed(() => `${months.value[viewMonth.value]} ${viewYear.value}`)

const pad = (n: number) => String(n).padStart(2, '0')

const inView = (d: Date) =>
  d.getFullYear() === viewYear.value && d.getMonth() === viewMonth.value

const eventsByDay = computed(() => {
  const map = new Map<string, CalEvent[]>()
  for (const ev of props.events) {
    const d = new Date(ev.when)
    if (!inView(d)) continue
    const iso = `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`
    const bucket = map.get(iso)
    if (bucket) bucket.push(ev)
    else map.set(iso, [ev])
  }
  for (const list of map.values()) {
    list.sort((a, b) => new Date(a.when).getTime() - new Date(b.when).getTime())
  }
  return map
})

const cells = computed(() => {
  const firstWeekday = (new Date(viewYear.value, viewMonth.value, 1).getDay() + 6) % 7
  const totalDays = new Date(viewYear.value, viewMonth.value + 1, 0).getDate()
  const out: Array<number | null> = Array.from({ length: firstWeekday }, () => null)
  for (let day = 1; day <= totalDays; day++) out.push(day)
  while (out.length % 7 !== 0) out.push(null)
  return out
})

const isoFor = (day: number) =>
  `${viewYear.value}-${pad(viewMonth.value + 1)}-${pad(day)}`

const isToday = (day: number) =>
  today.getFullYear() === viewYear.value &&
  today.getMonth() === viewMonth.value &&
  today.getDate() === day

const shiftMonth = (delta: number) => {
  const next = new Date(viewYear.value, viewMonth.value + delta, 1)
  viewYear.value = next.getFullYear()
  viewMonth.value = next.getMonth()
}

const pickDay = (day: number) => {
  const iso = isoFor(day)
  emit('select-date', props.selectedDate === iso ? null : iso)
}

const goToday = () => {
  viewYear.value = today.getFullYear()
  viewMonth.value = today.getMonth()
}

const clearDay = () => emit('select-date', null)

const monthEventCount = computed(() =>
  [...eventsByDay.value.values()].reduce((sum, list) => sum + list.length, 0),
)

const dayLabel = (iso: string) =>
  new Date(`${iso}T00:00:00`).toLocaleDateString(intlLocale.value, {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
  })

const formatTime = (iso: string) =>
  new Date(iso).toLocaleTimeString(intlLocale.value, { hour: '2-digit', minute: '2-digit' })

const titleOf = (ev: CalEvent) => ev.localizedTitle || ev.title

const agenda = computed(() => {
  const byDay = eventsByDay.value
  if (props.selectedDate) {
    const list = byDay.get(props.selectedDate)
    return list?.length ? [{ iso: props.selectedDate, label: dayLabel(props.selectedDate), list }] : []
  }
  return [...byDay.entries()]
    .sort((a, b) => a[0].localeCompare(b[0]))
    .map(([iso, list]) => ({ iso, label: dayLabel(iso), list }))
})
</script>

<template>
  <section class="mx-auto w-full max-w-7xl px-4 py-6 md:px-6">
    <div class="rounded-sm border border-border bg-surface p-4 md:p-6">
      <div class="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div class="flex items-center gap-2">
          <button
            type="button"
            class="rounded-sm border border-border p-2 text-text-muted transition-colors hover:bg-surface hover:text-text"
            :aria-label="t('cal.prev')"
            @click="shiftMonth(-1)"
          >
            <svg
              width="16"
              height="16"
              viewBox="0 0 24 24"
              fill="currentColor"
            ><path d="M15 6l-6 6 6 6" /></svg>
          </button>
          <h2 class="min-w-40 text-center text-lg font-semibold capitalize text-text">
            {{ monthLabel }}
          </h2>
          <button
            type="button"
            class="rounded-sm border border-border p-2 text-text-muted transition-colors hover:bg-surface hover:text-text"
            :aria-label="t('cal.next')"
            @click="shiftMonth(1)"
          >
            <svg
              width="16"
              height="16"
              viewBox="0 0 24 24"
              fill="currentColor"
            ><path d="M9 6l6 6-6 6" /></svg>
          </button>
        </div>

        <div class="flex items-center gap-2 text-xs text-text-muted">
          <span>{{ t('cal.count', { n: monthEventCount }) }}</span>
          <button
            type="button"
            class="rounded-sm border border-border px-3 py-1.5 text-text transition-colors hover:bg-surface hover:text-text"
            @click="goToday"
          >
            {{ t('cal.today') }}
          </button>
          <button
            v-if="selectedDate"
            type="button"
            class="rounded-sm border border-border px-3 py-1.5 text-text transition-colors hover:bg-surface hover:text-text"
            @click="clearDay"
          >
            {{ t('cal.allDays') }}
          </button>
        </div>
      </div>

      <div class="grid grid-cols-7 gap-1 text-center">
        <span
          v-for="(day, i) in weekdays"
          :key="i"
          class="py-1 text-[11px] font-semibold uppercase text-text-muted"
        >{{ day }}</span>

        <template
          v-for="(cell, i) in cells"
          :key="i"
        >
          <span
            v-if="cell === null"
            class="h-11 md:h-14"
          />
          <button
            v-else
            type="button"
            class="relative flex h-11 flex-col items-center justify-center rounded-sm text-sm transition-colors md:h-14"
            :class="[
              selectedDate === isoFor(cell)
                ? 'bg-action font-semibold text-action-text'
                : eventsByDay.has(isoFor(cell))
                  ? 'bg-surface font-medium text-action hover:bg-action-hover hover:text-action-text'
                  : 'text-text-muted hover:bg-surface hover:text-text',
              isToday(cell) ? 'ring-1 ring-text' : '',
            ]"
            :aria-pressed="selectedDate === isoFor(cell)"
            :aria-label="`${cell} ${months[viewMonth]}`"
            @click="pickDay(cell)"
          >
            <span>{{ cell }}</span>
            <span
              v-if="eventsByDay.has(isoFor(cell))"
              class="text-[10px] font-normal text-action"
            >{{ eventsByDay.get(isoFor(cell))!.length }}</span>
          </button>
        </template>
      </div>
    </div>

    <div class="mt-6 space-y-6">
      <div
        v-if="!agenda.length"
        class="rounded-sm border border-dashed border-border p-8 text-center text-sm text-text-muted"
      >
        {{ t('cal.empty', { month: monthLabel }) }}
      </div>

      <div
        v-for="group in agenda"
        :key="group.iso"
      >
        <h3 class="mb-2 text-sm font-semibold capitalize text-text-muted">
          {{ group.label }}
          <span
            v-if="selectedDate === group.iso"
            class="ml-2 text-xs text-action"
          >{{ t('cal.filterActive') }}</span>
        </h3>
        <ul class="space-y-2">
          <li
            v-for="ev in group.list"
            :key="ev.id"
            class="flex items-start gap-4 rounded-sm border border-control-border bg-surface p-3"
          >
            <span class="w-14 shrink-0 text-sm font-semibold text-text">
              {{ formatTime(ev.when) }}
            </span>
            <span class="min-w-0 flex-1">
              <span class="block truncate text-sm text-text">{{ titleOf(ev) }}</span>
              <span class="block truncate text-xs text-text-muted">
                {{ ev.neighborhood }}
                <template v-if="ev.rsvpCount"> · {{ t('cal.interested', { n: ev.rsvpCount }) }}</template>
              </span>
            </span>
          </li>
        </ul>
      </div>
    </div>
  </section>
</template>