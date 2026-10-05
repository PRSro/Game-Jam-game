<script setup lang="ts">
import { computed, useId } from 'vue'

const props = withDefaults(
  defineProps<{
    modelValue?: boolean
    label: string
    description?: string
    disabled?: boolean
  }>(),
  { modelValue: false, description: undefined, disabled: false },
)

const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>()

const uid = useId()
const switchId = computed(() => `switch-${uid}`)
const descId = computed(() => (props.description ? `${switchId.value}-desc` : undefined))

function toggle() {
  if (props.disabled) return
  emit('update:modelValue', !props.modelValue)
}
</script>

<template>
  <div class="flex items-start gap-3">
    <button
      :id="switchId"
      type="button"
      role="switch"
      :aria-checked="props.modelValue"
      :aria-describedby="descId"
      :disabled="props.disabled"
      class="relative mt-0.5 inline-flex h-6 w-11 shrink-0 cursor-pointer items-center rounded-full border transition-colors duration-150 ease-standard disabled:cursor-not-allowed disabled:opacity-50"
      :class="props.modelValue
        ? 'border-action bg-action'
        : 'border-control-border bg-surface hover:border-text-muted'"
      @click="toggle"
    >
      <span
        class="inline-block h-4 w-4 rounded-full bg-surface transition-transform duration-150 ease-standard"
        :class="props.modelValue ? 'translate-x-6' : 'translate-x-1'"
        aria-hidden="true"
      />
    </button>

    <div class="min-w-0">
      <label
        :for="switchId"
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
    </div>
  </div>
</template>
