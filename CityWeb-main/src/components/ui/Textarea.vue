<script setup lang="ts">
import { computed, useId } from 'vue'

const props = withDefaults(
  defineProps<{
    modelValue?: string
    label: string
    placeholder?: string
    description?: string
    error?: string
    disabled?: boolean
    required?: boolean
    rows?: number
  }>(),
  {
    modelValue: '',
    placeholder: undefined,
    description: undefined,
    error: undefined,
    disabled: false,
    required: false,
    rows: 4,
  },
)

const emit = defineEmits<{ (e: 'update:modelValue', value: string): void }>()

const uid = useId()
const fieldId = computed(() => `textarea-${uid}`)
const descId = computed(() =>
  props.description || props.error ? `${fieldId.value}-desc` : undefined,
)
const errorId = computed(() => (props.error ? `${fieldId.value}-error` : undefined))
const describedBy = computed(() =>
  [descId.value, errorId.value].filter(Boolean).join(' ') || undefined,
)
</script>

<template>
  <div class="flex flex-col">
    <label
      :for="fieldId"
      class="mb-1 font-ui text-caption font-medium uppercase tracking-wide text-text"
    >
      {{ props.label }}
      <span
        v-if="props.required"
        class="text-danger"
        aria-hidden="true"
      >*</span>
    </label>

    <textarea
      :id="fieldId"
      :value="props.modelValue"
      :rows="props.rows"
      :placeholder="props.placeholder"
      :disabled="props.disabled"
      :required="props.required"
      :aria-describedby="describedBy"
      :aria-invalid="props.error ? 'true' : undefined"
      class="w-full resize-y rounded-sm border bg-surface px-3 py-2 font-body text-body text-text transition-colors duration-150 ease-standard placeholder:text-text-muted focus:outline-none focus-visible:outline-2 focus-visible:outline-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
      :class="
        props.error
          ? 'border-danger focus-visible:outline-danger'
          : 'border-control-border focus-visible:outline-action'
      "
      @input="emit('update:modelValue', ($event.target as HTMLTextAreaElement).value)"
    />

    <p
      v-if="props.description"
      :id="descId"
      class="mt-1 font-body text-caption text-text-muted"
    >
      {{ props.description }}
    </p>
    <p
      v-if="props.error"
      :id="errorId"
      class="mt-1 font-body text-caption text-danger"
      aria-live="polite"
    >
      {{ props.error }}
    </p>
  </div>
</template>
