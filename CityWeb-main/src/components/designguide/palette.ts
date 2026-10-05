/**
 * Display metadata for the style guide. The hex strings are labels shown to
 * the reader, never applied as a colour - swatch fills use token utilities.
 *
 * Every ratio below is measured, not typed by hand, and comes from
 * `npm run check:contrast -- --json`. Background colours the gate does not
 * pair are `null` and render as an em dash: the gate measures text pairs, not
 * fills. Re-run the gate after any token change and update these in step.
 */

export type Swatch = {
  token: string
  /** Literal Tailwind class so the scanner sees it. Never build this by hand. */
  fill: string
  name: string
  hex: string
  role: string
  /** Text token drawn on the swatch. */
  on: 'text-linie-on-clar' | 'text-linie-on-intens'
  /** Measured contrast against the page background, per theme. */
  onBg: { light: number | null; dark: number | null }
  decorationOnly?: boolean
}

export const FATADA: Swatch[] = [
  {
    token: '--calcar',
    fill: 'bg-calcar',
    name: 'Calcar',
    hex: '#EDE4D0',
    role: 'Page background. Cream stone of the CEC Palace.',
    on: 'text-linie-on-clar',
    onBg: { light: null, dark: null },
  },
  {
    token: '--cerneala',
    fill: 'bg-cerneala',
    name: 'Cerneală',
    hex: '#26231F',
    role: 'Warm ink. All light-mode body text.',
    on: 'text-linie-on-intens',
    onBg: { light: 12.37, dark: null },
  },
  {
    token: '--teracota',
    fill: 'bg-teracota',
    name: 'Teracotă',
    hex: '#A8472E',
    role: 'Roof tile. Primary action colour, links.',
    on: 'text-linie-on-intens',
    onBg: { light: 4.61, dark: 6.86 },
  },
  {
    token: '--oblon',
    fill: 'bg-oblon',
    name: 'Oblon',
    hex: '#4A6652',
    role: 'Shutter green. Secondary accent, success states.',
    on: 'text-linie-on-intens',
    onBg: { light: 5.01, dark: 7.18 },
  },
  {
    token: '--ocru',
    fill: 'bg-ocru',
    name: 'Ocru',
    hex: '#C8923A',
    role: 'Faded ochre stucco. Fills and borders only, never text.',
    on: 'text-linie-on-clar',
    onBg: { light: 2.18, dark: null },
    decorationOnly: true,
  },
  {
    token: '--piatra',
    fill: 'bg-piatra',
    name: 'Piatră',
    hex: '#8C877D',
    role: 'Cobblestone. Dividers and borders. Never text.',
    on: 'text-linie-on-clar',
    onBg: { light: 2.83, dark: null },
    decorationOnly: true,
  },
  {
    token: '--roz',
    fill: 'bg-roz',
    name: 'Roz prăfuit',
    hex: '#D8A898',
    role: 'Dusty pink villa walls. Small accent only.',
    on: 'text-linie-on-clar',
    onBg: { light: null, dark: null },
    decorationOnly: true,
  },
]

export const LINII: Swatch[] = [
  {
    token: '--linie-evenimente',
    fill: 'bg-linie-evenimente',
    name: 'Evenimente',
    hex: '#F2C200',
    role: 'Events. Ink on the fill.',
    on: 'text-linie-on-clar',
    onBg: { light: 1.33, dark: 9.46 },
  },
  {
    token: '--linie-joburi',
    fill: 'bg-linie-joburi',
    name: 'Joburi',
    hex: '#0B5CA8',
    role: 'Jobs. White on the fill.',
    on: 'text-linie-on-intens',
    onBg: { light: 5.34, dark: 2.35 },
  },
  {
    token: '--linie-trafic',
    fill: 'bg-linie-trafic',
    name: 'Trafic',
    hex: '#D6182B',
    role: 'Traffic and alerts. White on the fill.',
    on: 'text-linie-on-intens',
    onBg: { light: 4.13, dark: 3.05 },
  },
  {
    token: '--linie-oameni',
    fill: 'bg-linie-oameni',
    name: 'Oameni',
    hex: '#0B7A41',
    role: 'People. White on the fill.',
    on: 'text-linie-on-intens',
    onBg: { light: 4.29, dark: 2.93 },
  },
  {
    token: '--linie-sondaje',
    fill: 'bg-linie-sondaje',
    name: 'Sondaje',
    hex: '#F28A1E',
    role: 'Polls. Ink on the fill.',
    on: 'text-linie-on-clar',
    onBg: { light: 1.97, dark: 6.38 },
  },
]

/** Ink or white text drawn on each category fill. */
export const LINIE_ON = [
  { name: 'Evenimente', ratio: 9.3, on: 'text-linie-on-clar', fill: 'bg-linie-evenimente' },
  { name: 'Joburi', ratio: 6.75, on: 'text-linie-on-intens', fill: 'bg-linie-joburi' },
  { name: 'Trafic', ratio: 5.22, on: 'text-linie-on-intens', fill: 'bg-linie-trafic' },
  { name: 'Oameni', ratio: 5.42, on: 'text-linie-on-intens', fill: 'bg-linie-oameni' },
  { name: 'Sondaje', ratio: 6.28, on: 'text-linie-on-clar', fill: 'bg-linie-sondaje' },
]

export const SEARA: Swatch[] = [
  {
    token: '--amurg',
    fill: 'bg-amurg',
    name: 'Amurg',
    hex: '#1C2230',
    role: 'Dusk. Page background in dark mode.',
    on: 'text-linie-on-intens',
    onBg: { light: null, dark: null },
  },
  {
    token: '--hartie',
    fill: 'bg-hartie',
    name: 'Hârtie',
    hex: '#EAE3D3',
    role: 'Paper. All dark-mode body text.',
    on: 'text-linie-on-clar',
    onBg: { light: null, dark: 12.44 },
  },
  {
    token: '--sodiu',
    fill: 'bg-sodiu',
    name: 'Sodiu',
    hex: '#E59B2F',
    role: 'Sodium streetlight. Primary accent, replaces terracotta.',
    on: 'text-linie-on-clar',
    onBg: { light: null, dark: 6.86 },
  },
  {
    token: '--tei',
    fill: 'bg-tei',
    name: 'Tei',
    hex: '#A9B565',
    role: 'Linden. Secondary accent.',
    on: 'text-linie-on-clar',
    onBg: { light: null, dark: 7.18 },
  },
  {
    token: '--apa',
    fill: 'bg-apa',
    name: 'Apă',
    hex: '#55645E',
    role: 'River Dâmbovița. Dividers only, never text.',
    on: 'text-linie-on-intens',
    onBg: { light: null, dark: 2.55 },
    decorationOnly: true,
  },
]

export const SEMANTIC = [
  { token: '--text', use: 'body text', light: 12.37, dark: 12.44, min: 4.5 },
  { token: '--text-muted', use: 'secondary text', light: 5.5, dark: 7.58, min: 4.5 },
  { token: '--action', use: 'links, action text', light: 4.61, dark: 6.86, min: 4.5 },
  { token: '--accent', use: 'secondary accent', light: 5.01, dark: 7.18, min: 4.5 },
  { token: '--action-text', use: 'label on action fill', light: 4.61, dark: 6.86, min: 4.5 },
  { token: '--control-border', use: 'input boundary', light: 5.5, dark: 7.58, min: 3 },
  { token: '--focus-ring', use: 'focus indicator', light: 12.37, dark: 12.44, min: 3 },
  { token: '--danger', use: 'alert marker only', light: 4.13, dark: 3.05, min: 3 },
]

export const DIACRITICS =
  'Știri București Țară Șoseaua Păcii — ș ț ș ț, ă â î Ă Â Î'
