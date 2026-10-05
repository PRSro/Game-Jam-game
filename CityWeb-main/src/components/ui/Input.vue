<script setup lang="ts">
import { computed, useId } from 'vue'
import type { IconLike } from './types'

const props = withDefaults(
  defineProps<{
    modelValue?: string
    label: string
    type?: 'text' | 'email' | 'tel' | 'url' | 'search' | 'password'
    placeholder?: string
    description?: string
    error?: string
    disabled?: boolean
    required?: boolean
    /** Leading 24px line icon, decorative and hidden from assistive tech. */
    icon?: IconLike
    autocomplete?: string
  }>(),
  {
    modelValue: '',
    type: 'text',
    placeholder: undefined,
    description: undefined,
    error: undefined,
    disabled: false,
    required: false,
    icon: undefined,
    autocomplete: undefined,
  },
)

const emit = defineEmits<{ (e: 'update:modelValue', value: string): void }>()

const uid = useId()
const inputId = computed(() => `input-${uid}`)
const descId = computed(() =>
  props.description || props.error ? `${inputId.value}-desc` : undefined,
)
const errorId = computed(() => (props.error ? `${inputId.value}-error` : undefined))
const describedBy = computed(() =>
  [descId.value, errorId.value].filter(Boolean).join(' ') || undefined,
)
</script>

<template>
  <div class="flex flex-col">
    <label
      :for="inputId"
      class="mb-1 font-ui text-caption font-medium uppercase tracking-wide text-text"
    >
      {{ props.label }}
      <span
        v-if="props.required"
        class="text-danger"
        aria-hidden="true"
      >*</span>
    </label>

    <div class="relative flex items-center">
      <component
        :is="props.icon"
        v-if="props.icon"
        :size="20"
        class="pointer-events-none absolute left-3 text-text-muted"
        aria-hidden="true"
      />
      <input
        :id="inputId"
        :value="props.modelValue"
        :type="props.type"
        :placeholder="props.placeholder"
        :disabled="props.disabled"
        :required="props.required"
        :autocomplete="props.autocomplete"
        :aria-describedby="describedBy"
        :aria-invalid="props.error ? 'true' : undefined"
        class="w-full rounded-sm border bg-surface px-3 py-2 font-body text-body text-text transition-colors duration-150 ease-standard placeholder:text-text-muted focus:outline-none focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-focus-ring disabled:cursor-not-allowed disabled:opacity-50"
        :class="[
          props.icon ? 'pl-10' : '',
          props.error
            ? 'border-danger focus-visible:outline-danger'
            : 'border-control-border focus-visible:outline-action',
        ]"
        @input="emit('update:modelValue', ($event.target as HTMLInputElement).value)"
      >
    </div>

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
