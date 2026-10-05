import type { Component } from 'vue'

/**
 * The five Metrorex line colours, used as category markers only.
 * Kept out of the components because <script setup> cannot export types.
 */
export type Category = 'evenimente' | 'joburi' | 'trafic' | 'oameni' | 'sondaje'

/** Alert / toast tone. `danger` is reserved for traffic disruptions. */
export type Tone = 'info' | 'success' | 'warning' | 'danger'

/** A 24px line icon from ./icons, or any component with no colour baked in. */
export type IconLike = Component

export type SelectOption = {
  value: string
  label: string
  disabled?: boolean
}
