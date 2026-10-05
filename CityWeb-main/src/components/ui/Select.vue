<script setup lang="ts">
import { computed, useId } from 'vue'
import { IconChevronDown } from './icons'
import type { SelectOption } from './types'


const props = withDefaults(
  defineProps<{
    modelValue?: string
    label: string
    options: SelectOption[]
    placeholder?: string
    description?: string
    error?: string
    disabled?: boolean
    required?: boolean
  }>(),
  {
    modelValue: '',
    placeholder: undefined,
    description: undefined,
    error: undefined,
    disabled: false,
    required: false,
  },
)

const emit = defineEmits<{ (e: 'update:modelValue', value: string): void }>()

const uid = useId()
const fieldId = computed(() => `select-${uid}`)
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

    <div class="relative flex items-center">
      <select
        :id="fieldId"
        :value="props.modelValue"
        :disabled="props.disabled"
        :required="props.required"
        :aria-describedby="describedBy"
        :aria-invalid="props.error ? 'true' : undefined"
        class="w-full appearance-none rounded-sm border bg-surface py-2 pl-3 pr-10 font-body text-body text-text transition-colors duration-150 ease-standard focus:outline-none focus-visible:outline-2 focus-visible:outline-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
        :class="[
          props.error
            ? 'border-danger focus-visible:outline-danger'
            : 'border-control-border focus-visible:outline-action',
          props.modelValue ? '' : 'text-text-muted',
        ]"
        @change="emit('update:modelValue', ($event.target as HTMLSelectElement).value)"
      >
        <option
          v-if="props.placeholder"
          value=""
          disabled
        >
          {{ props.placeholder }}
        </option>
        <option
          v-for="option in props.options"
          :key="option.value"
          :value="option.value"
          :disabled="option.disabled"
        >
          {{ option.label }}
        </option>
      </select>

      <IconChevronDown
        :size="20"
        class="pointer-events-none absolute right-3 text-text-muted"
        aria-hidden="true"
      />
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
