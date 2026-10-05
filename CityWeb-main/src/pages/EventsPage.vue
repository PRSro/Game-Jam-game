<script setup lang="ts">
import EventsGrid from '../components/EventsGrid.vue'

defineProps<{
  search: string
  selectedDate: string | null
}>()
</script>

<template>
  <main class="mx-auto w-full max-w-7xl px-4 py-6 md:px-6">
    <EventsGrid
      :search="search"
      :selected-date="selectedDate"
    />
  </main>
</template>