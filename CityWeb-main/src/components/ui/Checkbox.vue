<script setup lang="ts">
import { computed, useId } from 'vue'

const props = withDefaults(
  defineProps<{
    modelValue?: boolean
    label: string
    description?: string
    disabled?: boolean
    error?: string
  }>(),
  { modelValue: false, description: undefined, disabled: false, error: undefined },
)

const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>()

const uid = useId()
const inputId = computed(() => `checkbox-${uid}`)
const descId = computed(() => (props.description || props.error ? `${inputId.value}-desc` : undefined))
const errorId = computed(() => (props.error ? `${inputId.value}-error` : undefined))
const describedBy = computed(() =>
  [descId.value, errorId.value].filter(Boolean).join(' ') || undefined,
)

function toggle(event: Event) {
  emit('update:modelValue', (event.target as HTMLInputElement).checked)
}
</script>

<template>
  <div class="flex items-start gap-3">
    <input
      :id="inputId"
      :checked="props.modelValue"
      type="checkbox"
      class="mt-0.5 h-[18px] w-[18px] shrink-0 cursor-pointer appearance-none rounded-sm border border-control-border bg-surface accent-action transition-colors duration-150 checked:border-action checked:bg-action hover:border-action active:bg-action-active disabled:cursor-not-allowed disabled:opacity-50 disabled:hover:border-control-border"
      :disabled="props.disabled"
      :aria-describedby="describedBy"
      :aria-invalid="props.error ? 'true' : undefined"
      @change="toggle"
    >

    <div class="min-w-0">
      <label
        :for="inputId"
        class="block cursor-pointer font-ui text-body text-text"
        :class="props.disabled ? 'cursor-not-allowed opacity-50' : ''"
      >
        {{ props.label }}
      </label>
      <p
        v-if="props.description"
        :id="descId"
        class="mt-0.5 font-body text-caption text-text-muted"
      >
        {{ props.description }}
      </p>
      <p
        v-if="props.error"
        :id="errorId"
        class="mt-0.5 font-body text-caption text-danger"
        aria-live="polite"
      >
        {{ props.error }}
      </p>
    </div>
  </div>
</template>
