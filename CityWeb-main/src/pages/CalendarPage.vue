<script setup lang="ts">
import CalendarView from '../components/CalendarView.vue'

defineProps<{
  events: Array<{
    id: string
    title: string
    localizedTitle?: string
    when: string
    neighborhood?: string
    tags?: string[]
    rsvpCount?: number
  }>
  selectedDate: string | null
}>()
</script>

<template>
  <CalendarView
    :events="events"
    :selected-date="selectedDate"
  />
</template>