<script setup lang="ts">
import { computed, useId } from 'vue'

const props = withDefaults(
  defineProps<{
    modelValue?: string
    value: string
    name: string
    label: string
    description?: string
    disabled?: boolean
  }>(),
  { modelValue: undefined, description: undefined, disabled: false },
)

const emit = defineEmits<{ (e: 'update:modelValue', value: string): void }>()

const uid = useId()
const inputId = computed(() => `radio-${uid}`)
const descId = computed(() => (props.description ? `${inputId.value}-desc` : undefined))

function select() {
  if (props.disabled) return
  emit('update:modelValue', props.value)
}
</script>

<template>
  <div class="flex items-start gap-3">
    <input
      :id="inputId"
      :name="props.name"
      :value="props.value"
      type="radio"
      class="mt-0.5 h-[18px] w-[18px] shrink-0 cursor-pointer appearance-none rounded-full border border-control-border bg-surface accent-action transition-colors duration-150 checked:border-action checked:bg-action hover:border-action active:bg-action-active disabled:cursor-not-allowed disabled:opacity-50 disabled:hover:border-control-border"
      :checked="props.modelValue === props.value"
      :disabled="props.disabled"
      :aria-describedby="descId"
      @change="select"
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
    </div>
  </div>
</template>
