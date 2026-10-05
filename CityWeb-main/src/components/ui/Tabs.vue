<script setup lang="ts">
import { computed, ref, useId } from 'vue'

type Tab = {
  id: string
  label: string
  /** Optional 24px line icon rendered before the label. */
  icon?: unknown
  disabled?: boolean
}

const props = defineProps<{
  tabs: Tab[]
  modelValue: string
  label: string
}>()

const emit = defineEmits<{ (e: 'update:modelValue', value: string): void }>()

const uid = useId()
const buttons = ref<(HTMLButtonElement | null)[]>([])

const enabledIndexes = computed(() =>
  props.tabs.map((tab, index) => (tab.disabled ? -1 : index)).filter((i) => i >= 0),
)

function isActive(id: string) {
  return props.modelValue === id
}

function panelId(id: string) {
  return `${uid}-panel-${id}`
}

function tabId(id: string) {
  return `${uid}-tab-${id}`
}

function focusTab(index: number) {
  const target = props.tabs[index]
  if (!target || target.disabled) return
  emit('update:modelValue', target.id)
  buttons.value[index]?.focus()
}

function onKeydown(event: KeyboardEvent) {
  const current = props.tabs.findIndex((tab) => tab.id === props.modelValue)
  if (current < 0) return

  const enabled = enabledIndexes.value
  const position = enabled.indexOf(current)
  if (position < 0) return

  let next: number | undefined
  if (event.key === 'ArrowRight' || event.key === 'ArrowDown') {
    next = enabled[(position + 1) % enabled.length]
  } else if (event.key === 'ArrowLeft' || event.key === 'ArrowUp') {
    next = enabled[(position - 1 + enabled.length) % enabled.length]
  } else if (event.key === 'Home') {
    next = enabled[0]
  } else if (event.key === 'End') {
    next = enabled[enabled.length - 1]
  }

  if (next === undefined) return
  event.preventDefault()
  focusTab(next)
}
</script>

<template>
  <div class="flex flex-col gap-4">
    <div
      role="tablist"
      :aria-label="props.label"
      class="flex flex-wrap gap-1 border-b border-border"
      @keydown="onKeydown"
    >
      <button
        v-for="(tab, index) in props.tabs"
        :id="tabId(tab.id)"
        :key="tab.id"
        :ref="(el) => { buttons[index] = el as HTMLButtonElement | null }"
        type="button"
        role="tab"
        :aria-selected="isActive(tab.id)"
        :aria-controls="panelId(tab.id)"
        :tabindex="isActive(tab.id) ? 0 : -1"
        :disabled="tab.disabled"
        class="-mb-px inline-flex items-center gap-2 rounded-t-sm border-b-2 px-3 py-2 font-ui text-body transition-colors duration-150 ease-standard disabled:cursor-not-allowed disabled:opacity-50"
        :class="isActive(tab.id)
          ? 'border-b-action text-action'
          : 'border-b-transparent text-text-muted hover:border-b-control-border hover:text-text'"
        @click="focusTab(index)"
      >
        <component
          :is="tab.icon"
          v-if="tab.icon"
          :size="20"
        />
        <span>{{ tab.label }}</span>
      </button>
    </div>

    <div
      v-for="tab in props.tabs"
      v-show="isActive(tab.id)"
      :id="panelId(tab.id)"
      :key="`panel-${tab.id}`"
      role="tabpanel"
      :aria-labelledby="tabId(tab.id)"
      tabindex="0"
      class="focus-visible:rounded-sm"
    >
      <slot :name="tab.id">
        {{ tab.label }}
      </slot>
    </div>
  </div>
</template>
