<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, ref, useId, watch } from 'vue'
import { IconClose } from './icons'

const props = withDefaults(
  defineProps<{
    open: boolean
    title: string
    dismissible?: boolean
  }>(),
  { dismissible: true },
)

const emit = defineEmits<{ (e: 'close'): void }>()

const uid = useId()
const titleId = computed(() => `modal-${uid}-title`)
const panel = ref<HTMLElement | null>(null)

let previouslyFocused: HTMLElement | null = null

const FOCUSABLE = [
  'a[href]',
  'button:not([disabled])',
  'input:not([disabled])',
  'select:not([disabled])',
  'textarea:not([disabled])',
  '[tabindex]:not([tabindex="-1"])',
].join(', ')

function requestClose() {
  if (props.dismissible) emit('close')
}

function visibleNodes(): HTMLElement[] {
  if (!panel.value) return []
  return Array.from(panel.value.querySelectorAll<HTMLElement>(FOCUSABLE)).filter(
    (node) => node.offsetParent !== null,
  )
}

function onKeydown(event: KeyboardEvent) {
  if (event.key === 'Escape') {
    requestClose()
    return
  }
  if (event.key !== 'Tab') return

  const nodes = visibleNodes()
  if (nodes.length === 0) {
    event.preventDefault()
    panel.value?.focus()
    return
  }

  const first = nodes[0]
  const last = nodes[nodes.length - 1]
  const active = document.activeElement

  if (event.shiftKey && (active === first || active === panel.value)) {
    event.preventDefault()
    last.focus()
  } else if (!event.shiftKey && active === last) {
    event.preventDefault()
    first.focus()
  }
}

watch(
  () => props.open,
  async (open) => {
    if (open) {
      previouslyFocused = document.activeElement as HTMLElement | null
      document.body.style.overflow = 'hidden'
      await nextTick()
      const target = visibleNodes()[0] ?? panel.value
      target?.focus()
    } else {
      document.body.style.overflow = ''
      previouslyFocused?.focus()
      previouslyFocused = null
    }
  },
)

onBeforeUnmount(() => {
  document.body.style.overflow = ''
})
</script>

<template>
  <Teleport to="body">
    <div
      v-if="props.open"
      class="fixed inset-0 z-[60] flex items-center justify-center bg-amurg/70 p-4"
      @click.self="requestClose"
    >
      <div
        ref="panel"
        role="dialog"
        aria-modal="true"
        :aria-labelledby="titleId"
        tabindex="-1"
        class="max-h-[90vh] w-full max-w-lg overflow-y-auto rounded-sm border border-control-border bg-bg"
        @keydown="onKeydown"
      >
        <div class="flex items-start justify-between gap-3 border-b border-border p-4">
          <h2
            :id="titleId"
            class="font-display text-heading-sm text-text"
          >
            {{ props.title }}
          </h2>
          <button
            v-if="props.dismissible"
            type="button"
            class="shrink-0 rounded-sm p-1 text-text-muted transition-colors duration-150 ease-standard hover:bg-action-hover hover:text-action-text active:bg-action-active"
            aria-label="Închide"
            @click="requestClose"
          >
            <IconClose :size="20" />
          </button>
        </div>

        <div class="p-4">
          <slot />
        </div>

        <div
          v-if="$slots.footer"
          class="flex flex-wrap items-center justify-end gap-2 border-t border-border p-4"
        >
          <slot name="footer" />
        </div>
      </div>
    </div>
  </Teleport>
</template>
