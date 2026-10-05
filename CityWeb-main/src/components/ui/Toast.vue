<script setup lang="ts">
import { computed, onBeforeUnmount, watch } from 'vue'
import Alert from './Alert.vue'
import type { Tone } from './types'

const props = withDefaults(
  defineProps<{
    /** `false` hides the toast entirely. */
    open: boolean
    tone?: Tone
    title?: string
    /** Auto-dismiss delay in ms. `0` keeps it until dismissed. */
    duration?: number
  }>(),
  { tone: 'info', title: undefined, duration: 0 },
)

const emit = defineEmits<{ (e: 'dismiss'): void }>()

let timer: ReturnType<typeof setTimeout> | undefined

function clear() {
  if (timer !== undefined) {
    clearTimeout(timer)
    timer = undefined
  }
}

watch(
  () => [props.open, props.duration] as const,
  ([open, duration]) => {
    clear()
    if (open && duration > 0) {
      timer = setTimeout(() => emit('dismiss'), duration)
    }
  },
  { immediate: true },
)

onBeforeUnmount(clear)

const live = computed(() => (props.tone === 'danger' ? 'assertive' : 'polite'))
</script>

<template>
  <div
    v-if="props.open"
    class="pointer-events-none fixed inset-x-0 bottom-0 z-[70] flex justify-center p-4"
  >
    <div
      class="pointer-events-auto w-full max-w-sm"
      :aria-live="live"
      :aria-atomic="true"
    >
      <Alert
        :tone="props.tone"
        :title="props.title"
        dismissible
        class="bg-bg"
        @dismiss="emit('dismiss')"
      >
        <slot />
      </Alert>
    </div>
  </div>
</template>
