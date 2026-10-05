import { computed, ref } from "vue";

export type Locale = "ro" | "en";
export type MessageValue = string | string[];
export type TParams = Record<string, string | number>;

const STORAGE_KEY = "piata.locale";

const ro: Record<string, MessageValue> = {
  "brand.name": "Piața",
  "brand.tagline": "Hartă vie a Bucureștiului",

  "search.placeholder": "Caută în Piața: eveniment, loc, organizator, sector…",
  "search.label": "Caută evenimente, locuri, organizatori",

  "nav.label": "Navigare principală",
  "nav.events": "Evenimente",
  "nav.calendar": "Calendar",
  "nav.harta": "Harta",
  "nav.locale": "Limba",

  "calendar.toggle": "Calendarul evenimentelor",

  "events.title": "Evenimente în București",
  "events.sort.label": "Sortare",
  "events.sort.date": "În curând",
  "events.sort.name": "Nume (A–Z)",
  "events.category.label": "Categorie",
  "events.category.all": "Toate categoriile",
  "events.category.muzica": "Muzică",
  "events.category.cariera": "Carieră",
  "events.category.comunitate": "Comunitate",
  "events.count": "{n} evenimente",
  "events.countOne": "{n} eveniment",
  "events.empty": "Niciun eveniment nu se potrivește filtrelor.",
  "events.emptyTitle": "Nimic aici.",
  "events.emptyHint":
    "Încearcă altă zi din calendar sau alt termen de căutare.",
  "events.rsvp": "RSVP",
  "events.checkin": "check-in QR",
  "events.connections": "{n} conexiuni",

  "event.when": "Când",
  "event.where": "Unde",
  "event.organizers": "Organizatori",
  "event.interest": "Interes",
  "event.people": "{n} persoane",
  "event.connectedWith": "Conectat cu",
  "event.rsvpCta": "Vreau să merg",
  "event.address": "Adresă",

  "action.close": "Închide",

  "cal.months": [
    "ianuarie",
    "februarie",
    "martie",
    "aprilie",
    "mai",
    "iunie",
    "iulie",
    "august",
    "septembrie",
    "octombrie",
    "noiembrie",
    "decembrie",
  ],
  "cal.weekdays": ["L", "M", "M", "J", "V", "S", "D"],
  "cal.prev": "Luna anterioară",
  "cal.next": "Luna următoare",
  "cal.today": "Astăzi",
  "cal.allDays": "Toate zilele",
  "cal.count": "{n} evenimente",
  "cal.empty": "Niciun eveniment în {month}.",
  "cal.filterActive": "filtru activ",
  "cal.interested": "{n} interesați",

  "notFound.title": "Pagina nu există",
  "notFound.body": "Nu am găsit ceea ce căutai. Încearcă harta vie din nou.",
  "notFound.cta": "Vezi evenimentele",

  "theme.toDark": "Comută pe temă întunecată",
  "theme.toLight": "Comută pe temă luminoasă",
};

const en: Record<string, MessageValue> = {
  "brand.name": "Piața",
  "brand.tagline": "Bucharest, live",

  "search.placeholder": "Search Piața: event, place, organiser, sector…",
  "search.label": "Search events, places, organisers",

  "nav.label": "Main navigation",
  "nav.events": "Events",
  "nav.calendar": "Calendar",
  "nav.harta": "Map",
  "nav.locale": "Language",

  "calendar.toggle": "Event calendar",

  "events.title": "Events in Bucharest",
  "events.sort.label": "Sort",
  "events.sort.date": "Soonest",
  "events.sort.name": "Name (A–Z)",
  "events.category.label": "Category",
  "events.category.all": "All categories",
  "events.category.muzica": "Music",
  "events.category.cariera": "Careers",
  "events.category.comunitate": "Community",
  "events.count": "{n} events",
  "events.countOne": "{n} event",
  "events.empty": "No events match these filters.",
  "events.emptyTitle": "Nothing here.",
  "events.emptyHint": "Try another day on the calendar or another search term.",
  "events.rsvp": "RSVP",
  "events.checkin": "QR check-in",
  "events.connections": "{n} connections",

  "event.when": "When",
  "event.where": "Where",
  "event.organizers": "Organisers",
  "event.interest": "Interest",
  "event.people": "{n} people",
  "event.connectedWith": "Connected with",
  "event.rsvpCta": "I want to go",
  "event.address": "Address",

  "action.close": "Close",

  "cal.months": [
    "january",
    "february",
    "march",
    "april",
    "may",
    "june",
    "july",
    "august",
    "september",
    "october",
    "november",
    "december",
  ],
  "cal.weekdays": ["M", "T", "W", "T", "F", "S", "S"],
  "cal.prev": "Previous month",
  "cal.next": "Next month",
  "cal.today": "Today",
  "cal.allDays": "All days",
  "cal.count": "{n} events",
  "cal.empty": "No events in {month}.",
  "cal.filterActive": "filter active",
  "cal.interested": "{n} interested",

  "notFound.title": "Page not found",
  "notFound.body":
    "We could not find what you were looking for. Try the living map again.",
  "notFound.cta": "See events",

  "theme.toDark": "Switch to dark theme",
  "theme.toLight": "Switch to light theme",
};

const messages: Record<Locale, Record<string, MessageValue>> = { ro, en };

function readStoredLocale(): Locale {
  if (typeof localStorage === "undefined") return "ro";
  return localStorage.getItem(STORAGE_KEY) === "en" ? "en" : "ro";
}

export const locale = ref<Locale>(readStoredLocale());

const intlLocale = computed(() => (locale.value === "ro" ? "ro-RO" : "en-GB"));

export function setLocale(next: Locale): void {
  locale.value = next;
  if (typeof localStorage !== "undefined")
    localStorage.setItem(STORAGE_KEY, next);
  if (typeof document !== "undefined") document.documentElement.lang = next;
}

function lookup(key: string): MessageValue | undefined {
  return messages[locale.value][key] ?? messages.ro[key];
}

function interpolate(raw: string, params?: TParams): string {
  if (!params) return raw;
  return raw.replace(/\{(\w+)\}/g, (match, name: string) =>
    name in params ? String(params[name]) : match,
  );
}

export function t(key: string, params?: TParams): string {
  const value = lookup(key);
  if (typeof value === "string") return interpolate(value, params);
  return Array.isArray(value) ? "" : key;
}

export function tm(key: string): string[] {
  const value = lookup(key);
  return Array.isArray(value) ? value : [];
}

export function useI18n() {
  return { locale, intlLocale, t, tm, setLocale };
}
