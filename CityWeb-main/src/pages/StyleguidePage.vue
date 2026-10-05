<script setup lang="ts">
import ComponentStates from '../components/designguide/ComponentStates.vue'
import SwatchGrid from '../components/designguide/SwatchGrid.vue'
import ThemeToggle from '../components/ThemeToggle.vue'
import { DIACRITICS, FATADA, LINIE_ON, LINII, SEARA, SEMANTIC } from '../components/designguide/palette'
import * as Icons from '../components/ui/icons'

const iconList = Object.entries(Icons)
</script>

<template>
  <div class="mx-auto max-w-5xl px-4 py-8 sm:px-6">
    <header class="flex flex-wrap items-start justify-between gap-4">
      <div>
        <p class="font-ui text-caption uppercase tracking-wide text-text-muted">
          Piața · living style guide
        </p>
        <h1 class="mt-1 font-display text-heading-lg text-text">
          Sistem de design
        </h1>
        <p class="mt-2 max-w-prose font-body text-body text-text-muted">
          Every colour, type style and component below is rendered from design
          tokens. Contrast ratios are measured by
          <code class="font-mono">npm run check:contrast</code>, not typed by hand.
        </p>
      </div>
      <ThemeToggle />
    </header>

    <div class="mt-10 flex flex-col gap-10">
      <SwatchGrid
        title="Fațada — cl clar"
        note="Limestone, ochre and tile taken from the street. Cerneală carries all light-mode text."
        :swatches="FATADA"
      />

      <SwatchGrid
        title="Liniile de metrou — marcaje de categorie"
        note="The five Metrorex line colours. They mark category, never background fill for large areas. Route hexes are UNVERIFIED approximations pending official Metrorex/TPBI confirmation."
        :swatches="LINII"
      />

      <SwatchGrid
        title="Seară — întuneric"
        note="Sodium streetlight and linden replace terracotta and shutter green once the sun is down."
        :swatches="SEARA"
      />

      <section class="border-t border-border pt-6">
        <h3 class="font-display text-heading-sm text-text">
          Text pe marcaj de categorie
        </h3>
        <p class="mt-1 max-w-prose font-body text-body text-text-muted">
          Ink on the yellow and orange lines, white on blue, red and green.
        </p>
        <ul class="mt-4 flex flex-wrap gap-2">
          <li
            v-for="item in LINIE_ON"
            :key="item.name"
            class="flex items-center gap-2 rounded-sm px-3 py-2 font-ui text-body font-medium"
            :class="[item.fill, item.on]"
          >
            {{ item.name }}
            <span class="font-mono text-caption">{{ item.ratio.toFixed(2) }}:1</span>
          </li>
        </ul>
      </section>

      <section class="border-t border-border pt-6">
        <h3 class="font-display text-heading-sm text-text">
          Tokeni semantici
        </h3>
        <p class="mt-1 max-w-prose font-body text-body text-text-muted">
          What each semantic token is for, and its measured contrast against the
          page background in each theme.
        </p>

        <div class="mt-4 overflow-x-auto">
          <table class="w-full border-collapse text-left">
            <thead>
              <tr class="border-b border-border">
                <th
                  scope="col"
                  class="py-2 pr-4 font-ui text-caption uppercase tracking-wide text-text-muted"
                >
                  Token
                </th>
                <th
                  scope="col"
                  class="py-2 pr-4 font-ui text-caption uppercase tracking-wide text-text-muted"
                >
                  Folosește pentru
                </th>
                <th
                  scope="col"
                  class="py-2 pr-4 font-ui text-caption uppercase tracking-wide text-text-muted"
                >
                  Clar
                </th>
                <th
                  scope="col"
                  class="py-2 pr-4 font-ui text-caption uppercase tracking-wide text-text-muted"
                >
                  Întuneric
                </th>
                <th
                  scope="col"
                  class="py-2 font-ui text-caption uppercase tracking-wide text-text-muted"
                >
                  Minim
                </th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="row in SEMANTIC"
                :key="row.token"
                class="border-b border-border"
              >
                <td class="py-2 pr-4 font-mono text-caption text-text">
                  {{ row.token }}
                </td>
                <td class="py-2 pr-4 font-body text-caption text-text-muted">
                  {{ row.use }}
                </td>
                <td class="py-2 pr-4 font-mono text-caption text-text">
                  {{ row.light.toFixed(2) }}:1
                </td>
                <td class="py-2 pr-4 font-mono text-caption text-text">
                  {{ row.dark.toFixed(2) }}:1
                </td>
                <td class="py-2 font-mono text-caption text-text-muted">
                  {{ row.min.toFixed(1) }}:1
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <section class="border-t border-border pt-6">
        <h3 class="font-display text-heading-sm text-text">
          Tipografie
        </h3>
        <p class="mt-1 max-w-prose font-body text-body text-text-muted">
          Bodoni Moda for display, Lora for body, Work Sans for UI. Every size
          below keeps its published line height.
        </p>

        <div class="mt-4 flex flex-col gap-4 rounded-sm border border-border bg-surface p-4">
          <p class="font-ui text-caption uppercase tracking-wide text-text-muted">
            Test diacritice — mărime titlu
          </p>
          <p class="font-display text-heading-lg leading-[1.15] text-text">
            {{ DIACRITICS }}
          </p>

          <p class="mt-2 font-ui text-caption uppercase tracking-wide text-text-muted">
            Test diacritice — mărime text
          </p>
          <p class="max-w-prose font-body text-body-lg text-text">
            {{ DIACRITICS }}
          </p>

          <div class="mt-2 flex flex-col gap-3 border-t border-border pt-4">
            <p class="font-display text-heading text-text">
              Display · titlu mare · {{ DIACRITICS }}
            </p>
            <p class="font-display text-heading-sm text-text">
              Display · titlu mic · {{ DIACRITICS }}
            </p>
            <p class="font-display text-subheading text-text">
              Display · subtitlu · {{ DIACRITICS }}
            </p>
            <p class="font-ui text-body text-text">
              Interfață · text · {{ DIACRITICS }}
            </p>
            <p class="font-body text-body text-text">
              Body · text · {{ DIACRITICS }}
            </p>
            <p class="font-body text-caption text-text-muted">
              Body · legendă · {{ DIACRITICS }}
            </p>
          </div>
        </div>
      </section>

      <section class="border-t border-border pt-6">
        <h3 class="font-display text-heading-sm text-text">
          Icoane
        </h3>
        <p class="mt-1 max-w-prose font-body text-body text-text-muted">
          24px line icons, 1.5px stroke, currentColor. No emoji anywhere in the product.
        </p>
        <ul class="mt-4 grid grid-cols-2 gap-3 sm:grid-cols-4">
          <li
            v-for="[name, icon] in iconList"
            :key="name"
            class="flex flex-col items-center gap-2 rounded-sm border border-border bg-surface p-3"
          >
            <component
              :is="icon"
              :size="24"
              class="text-text"
              aria-hidden="true"
            />
            <span class="font-mono text-caption text-text-muted">{{ name }}</span>
          </li>
        </ul>
      </section>

      <ComponentStates />
    </div>
  </div>
</template>
