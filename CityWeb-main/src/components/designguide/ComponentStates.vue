<script setup lang="ts">
import { ref } from 'vue'
import Alert from '../ui/Alert.vue'
import AppNav from '../ui/AppNav.vue'
import Button from '../ui/Button.vue'
import Card from '../ui/Card.vue'
import Checkbox from '../ui/Checkbox.vue'
import EmptyState from '../ui/EmptyState.vue'
import EventCard from '../ui/EventCard.vue'
import Input from '../ui/Input.vue'
import JobCard from '../ui/JobCard.vue'
import Link from '../ui/Link.vue'
import Modal from '../ui/Modal.vue'
import PersonChip from '../ui/PersonChip.vue'
import PollCard from '../ui/PollCard.vue'
import Radio from '../ui/Radio.vue'
import Select from '../ui/Select.vue'
import Skeleton from '../ui/Skeleton.vue'
import Switch from '../ui/Switch.vue'
import Tabs from '../ui/Tabs.vue'
import Tag from '../ui/Tag.vue'
import Textarea from '../ui/Textarea.vue'
import Toast from '../ui/Toast.vue'
import { IconInfo, IconPin, IconSearch } from '../ui/icons'

const check = ref(false)
const checkError = ref(true)
const radio = ref('flori')
const sw = ref(true)
const swOff = ref(false)
const text = ref('Floreasca')
const textErr = ref('')
const area = ref('')
const sel = ref('')
const selFilled = ref('bucuresti')
const tab = ref('azi')
const modalOpen = ref(false)
const toastOpen = ref(false)
const toastTone = ref<'info' | 'success' | 'warning' | 'danger'>('info')

const TABS = [
  { id: 'azi', label: 'Azi' },
  { id: 'saptamana', label: 'Săptămâna' },
  { id: 'luna', label: 'Luna' },
  { id: 'dezactivat', label: 'Dezactivat', disabled: true },
]

const OPTIONS = [
  { value: 'bucuresti', label: 'București' },
  { value: 'floresti', label: 'Florești' },
  { value: 'vanatori', label: 'Vânători', disabled: true },
]

const NAV_LINKS = [
  { label: 'Acum', to: '/evenimente' },
  { label: 'Hartă', to: '/evenimente' },
  { label: 'Calendar', to: '/calendar' },
  { label: 'Design', to: '/design' },
]
</script>

<template>
  <div class="flex flex-col gap-10">
    <section class="border-t border-border pt-6">
      <h3 class="font-display text-heading-sm text-text">
        Button
      </h3>
      <div class="mt-4 flex flex-wrap items-center gap-2">
        <Button variant="primary">
          Continuă
        </Button>
        <Button variant="secondary">
          Renunță
        </Button>
        <Button variant="text">
          Detalii
        </Button>
        <Button variant="secondary">
          Șterge
        </Button>
        <Button disabled>
          Dezactivat
        </Button>
        <Button size="sm">
          Mic
        </Button>
        <Button size="lg">
          Mare
        </Button>
      </div>
    </section>

    <section class="border-t border-border pt-6">
      <h3 class="font-display text-heading-sm text-text">
        Link
      </h3>
      <div class="mt-4 flex flex-col items-start gap-3">
        <Link
          to="/calendar"
          label="Vezi calendarul"
        />
        <Link
          to="/calendar"
          label="Blocații pe traseu"
          context="Metrou M2"
        />
        <Link
          to="https://metrorex.ro"
          label="Site oficial Metrorex"
          external
        />
        <Link
          to="/calendar"
          label="Fără subliniere"
          :underline="false"
        />
      </div>
    </section>

    <section class="border-t border-border pt-6">
      <h3 class="font-display text-heading-sm text-text">
        Câmpuri de formular
      </h3>
      <div class="mt-4 grid max-w-prose gap-4 sm:grid-cols-2">
        <Input
          v-model="text"
          label="Caută"
          placeholder="Sector, loc, eveniment"
          :icon="IconSearch"
        />
        <Input
          v-model="textErr"
          label="Eroare"
          error="Introdu un loc valid."
        />
        <Input
          v-model="textErr"
          label="Dezactivat"
          model-value=""
          disabled
        />
        <Textarea
          v-model="area"
          label="Observații"
          placeholder="Ce ai observat?"
          description="Maxim 280 de caractere."
        />
        <Select
          v-model="sel"
          label="Sector"
          placeholder="Alege un sector"
          :options="OPTIONS"
        />
        <Select
          v-model="selFilled"
          label="Completat"
          :options="OPTIONS"
        />
      </div>
    </section>

    <section class="border-t border-border pt-6">
      <h3 class="font-display text-heading-sm text-text">
        Selecție
      </h3>
      <div class="mt-4 grid max-w-prose gap-4 sm:grid-cols-2">
        <Checkbox
          v-model="check"
          label="VreauActualizări pe traseu"
          description="Un email pe zi, maximum."
        />
        <Checkbox
          v-model="checkError"
          label="Termeni nesăngiți"
          error="Trebuie să accepți termenii."
        />
        <Switch
          v-model="sw"
          label="Notificări întuneric"
        />
        <Switch
          v-model="swOff"
          label="Notificări dezactivate"
          description="Poți schimba oricând."
        />
        <div class="flex flex-col gap-2">
          <Radio
            v-model="radio"
            name="demo-radio"
            value="flori"
            label="Florești"
          />
          <Radio
            v-model="radio"
            name="demo-radio"
            value="centru"
            label="Centru"
          />
          <Radio
            v-model="radio"
            name="demo-radio"
            value="tgom"
            label="Târgu Mureș"
            disabled
          />
        </div>
      </div>
    </section>

    <section class="border-t border-border pt-6">
      <h3 class="font-display text-heading-sm text-text">
        Tag și Card
      </h3>
      <div class="mt-4 flex flex-wrap gap-2">
        <Tag
          category="evenimente"
          label="Concert"
        />
        <Tag
          category="joburi"
          label="Full-time"
          size="md"
        />
        <Tag
          category="trafic"
          label="Întrerupere"
        />
        <Tag
          category="oameni"
          label="Voluntari"
        />
        <Tag
          category="sondaje"
          label="Sondaj"
        />
      </div>
      <div class="mt-4 grid gap-3 sm:grid-cols-3">
        <Card
          category="evenimente"
          interactive
        >
          Card cu categorie, interactivă.
        </Card>
        <Card>Card simplă, fără categorie.</Card>
        <Card :padded="false">
          Card fără padding.
        </Card>
      </div>
    </section>

    <section class="border-t border-border pt-6">
      <h3 class="font-display text-heading-sm text-text">
        Carduri de conținut
      </h3>
      <div class="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
        <EventCard
          title="Nocturne pe Aleea Călărașilor"
          when="joi, 19:00"
          where="CEC Palace"
          neighborhood="Sector 3"
          :rsvp-count="42"
        />
        <JobCard
          title="Frontend developer"
          company="Atelier Digital"
          where="Remote · Florești"
          salary-label="Interval negociabil"
          commute="M2 · 18 min"
        />
        <PollCard
          question="Ce lipseste din sectorul tău?"
          scope="Florești · 128 voturi"
          :total-votes="128"
          :results="[
            { label: 'Parcuri', percent: 46 },
            { label: 'Transport', percent: 34 },
            { label: 'Spații verzi', percent: 28 },
          ]"
        />
      </div>
      <div class="mt-4 flex flex-wrap gap-2">
        <PersonChip
          name="Andrei Popescu"
          neighborhood="Florești"
        />
        <PersonChip
          name="Ioana Dumitru"
          neighborhood="Sector 1"
          initials="ID"
          interactive
        />
      </div>
    </section>

    <section class="border-t border-border pt-6">
      <h3 class="font-display text-heading-sm text-text">
        Tabs
      </h3>
      <div class="mt-4 max-w-prose">
        <Tabs
          v-model="tab"
          :tabs="TABS"
          label="Filtru de timp"
        />
        <p class="mt-2 font-body text-caption text-text-muted">
          Selecție: {{ tab }}
        </p>
      </div>
    </section>

    <section class="border-t border-border pt-6">
      <h3 class="font-display text-heading-sm text-text">
        Alert
      </h3>
      <div class="mt-4 flex flex-col gap-3">
        <Alert
          tone="info"
          title="Actualizare"
        >
          Trafic normal pe toate liniile.
        </Alert>
        <Alert
          tone="success"
          title="Ai confirmat"
        >
          Te-am reținut locul la concert.
        </Alert>
        <Alert
          tone="warning"
          title="Atenție"
        >
          Piața se închide la 20:00.
        </Alert>
        <Alert
          tone="danger"
          title="Întrerupere"
        >
          M2 nu circulă între Pipera și Băneasa.
        </Alert>
        <Alert
          tone="info"
          title="Poate fi închis"
          dismissible
        >
          Dismissibil, pentru mesaje neesențiale.
        </Alert>
      </div>
      <div class="mt-4 flex flex-wrap gap-2">
        <Button
          variant="secondary"
          @click="toastTone = 'success'; toastOpen = true"
        >
          Toast succes
        </Button>
        <Button
          variant="secondary"
          @click="toastTone = 'danger'; toastOpen = true"
        >
          Toast alertă
        </Button>
      </div>
    </section>

    <section class="border-t border-border pt-6">
      <h3 class="font-display text-heading-sm text-text">
        Goală și schelet
      </h3>
      <div class="mt-4 grid gap-3 sm:grid-cols-2">
        <EmptyState
          title="Niciun eveniment încă"
          body="Nu am găsit nimic pe traseul tău pentru seara asta."
        >
          <template #icon>
            <IconInfo :size="24" />
          </template>
        </EmptyState>
        <div class="flex flex-col gap-2 rounded-sm border border-border bg-surface p-4">
          <Skeleton
            height="1.25rem"
            width="60%"
          />
          <Skeleton height="1rem" />
          <Skeleton
            height="1rem"
            width="80%"
          />
          <Skeleton
            as="span"
            height="2rem"
            width="2rem"
            rounded="full"
          />
        </div>
      </div>
    </section>

    <section class="border-t border-border pt-6">
      <h3 class="font-display text-heading-sm text-text">
        Navigație
      </h3>
      <div class="mt-4 rounded-sm border border-border bg-surface p-3">
        <AppNav label="Navigație principală">
          <RouterLink
            v-for="link in NAV_LINKS"
            :key="link.label"
            :to="link.to"
            class="rounded-sm px-2 py-1 font-ui text-body text-text transition-colors duration-150 ease-standard hover:bg-action-hover hover:text-action-text focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-focus-ring"
          >
            {{ link.label }}
          </RouterLink>
          <template #actions>
            <IconPin
              :size="20"
              class="text-text-muted"
              aria-hidden="true"
            />
          </template>
        </AppNav>
      </div>
    </section>

    <section class="border-t border-border pt-6">
      <h3 class="font-display text-heading-sm text-text">
        Modal
      </h3>
      <p class="mt-1 max-w-prose font-body text-body text-text-muted">
        Focus este blocat în dialog, Escape închide, și focarul revine la
        elementul care l-a deschis.
      </p>
      <div class="mt-4">
        <Button @click="modalOpen = true">
          Deschide modalul
        </Button>
      </div>
    </section>

    <Modal
      :open="modalOpen"
      title="Confirmi prezența?"
      @close="modalOpen = false"
    >
      <p class="font-body text-body text-text">
        Te vom trimite un reminder cu o oră înainte. Poți schimba asta din notificări.
      </p>
      <template #footer>
        <Button
          variant="text"
          @click="modalOpen = false"
        >
          Renunță
        </Button>
        <Button @click="modalOpen = false">
          Confirmă
        </Button>
      </template>
    </Modal>

    <Toast
      :open="toastOpen"
      :tone="toastTone"
      :title="toastTone === 'danger' ? 'Întrerupere' : 'Salut'"
      @dismiss="toastOpen = false"
    >
      <span v-if="toastTone === 'danger'">M2 oprit între Pipera și Băneasa.</span>
      <span v-else>Prezența ta e confirmată.</span>
    </Toast>
  </div>
</template>
