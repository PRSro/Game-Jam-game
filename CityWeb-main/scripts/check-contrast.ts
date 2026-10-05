/**
 * Piața contrast gate.
 *
 * Reads the real pigment and semantic token values out of src/styles/*.css —
 * nothing is duplicated here — then verifies every semantic text and UI pair in
 * BOTH themes against WCAG 2.2 thresholds: 4.5:1 for body text, 3:1 for large
 * text and for UI components that convey state.
 *
 * Exits non-zero only on hard failures. Two classes are reported but never
 * block the build, because the design system deliberately accepts them:
 *   warn  — category markers on the page background. Yellow and orange on
 *           limestone cannot physically reach 3:1; the mitigation is that
 *           colour is never the sole carrier of meaning (every tag also
 *           carries its text label).
 *   info  — pairs the design system bans for text, printed with their
 *           measured ratios so nobody reaches for one without seeing it.
 *
 * Requires Node >= 22.6 for native TypeScript stripping.
 * Run: npm run check:contrast
 */

import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';

type Kind = 'body' | 'large' | 'ui' | 'soft';

interface Pair {
  fg: string;
  bg: string;
  kind: Kind;
  note?: string;
}

interface Rgba {
  r: number;
  g: number;
  b: number;
  a: number;
}

const THRESHOLD: Record<'body' | 'large' | 'ui', number> = { body: 4.5, large: 3, ui: 3 };

const root = process.cwd();
const themeCss = readFileSync(resolve(root, 'src/styles/theme.css'), 'utf8');
const globalsCss = readFileSync(resolve(root, 'src/styles/globals.css'), 'utf8');

function extractBlock(css: string, selector: string): string {
  const at = css.indexOf(selector);
  if (at === -1) return '';
  const open = css.indexOf('{', at);
  if (open === -1) return '';
  let depth = 0;
  for (let i = open; i < css.length; i += 1) {
    if (css[i] === '{') depth += 1;
    else if (css[i] === '}') {
      depth -= 1;
      if (depth === 0) return css.slice(open + 1, i);
    }
  }
  return '';
}

function extractVars(css: string): Record<string, string> {
  const out: Record<string, string> = {};
  for (const m of css.matchAll(/(--[a-z0-9-]+)\s*:\s*([^;]+);/gi)) {
    out[m[1]!] = m[2]!.trim();
  }
  return out;
}

function hexToRgba(hex: string): Rgba {
  let h = hex.trim().replace('#', '');
  if (h.length === 3) h = h.split('').map((c) => c + c).join('');
  if (h.length === 6) h += 'ff';
  return {
    r: parseInt(h.slice(0, 2), 16),
    g: parseInt(h.slice(2, 4), 16),
    b: parseInt(h.slice(4, 6), 16),
    a: parseInt(h.slice(6, 8), 16) / 255,
  };
}

interface Lab {
  L: number;
  a: number;
  b: number;
}

function srgbToLinear(v: number): number {
  const c = v / 255;
  return c <= 0.04045 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4);
}

function linearToSrgb(v: number): number {
  const c = v <= 0.0031308 ? 12.92 * v : 1.055 * Math.pow(v, 1 / 2.4) - 0.055;
  return Math.max(0, Math.min(255, Math.round(c * 255)));
}

/* Björn Ottosson's OKLab matrices — required because color-mix(in oklab, …)
   interpolates in OKLab, not sRGB. Mixing in sRGB would under-report contrast
   and let a failing hover state pass the gate. */
function toOklab(c: Rgba): Lab {
  const r = srgbToLinear(c.r);
  const g = srgbToLinear(c.g);
  const b = srgbToLinear(c.b);
  const l = Math.cbrt(0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b);
  const m = Math.cbrt(0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b);
  const s = Math.cbrt(0.0883024619 * r + 0.2817188376 * g + 0.6299787 * b);
  return {
    L: 0.2104542553 * l + 0.793617785 * m - 0.0040720468 * s,
    a: 1.9779984951 * l - 2.428592205 * m + 0.4505937099 * s,
    b: 0.0259040371 * l + 0.7827717662 * m - 0.808675766 * s,
  };
}

function fromOklab(lab: Lab): Rgba {
  const l = (lab.L + 0.3963377774 * lab.a + 0.2158037573 * lab.b) ** 3;
  const m = (lab.L - 0.1055613458 * lab.a - 0.0638541728 * lab.b) ** 3;
  const s = (lab.L - 0.0894841775 * lab.a - 1.291485548 * lab.b) ** 3;
  return {
    r: linearToSrgb(4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s),
    g: linearToSrgb(-1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s),
    b: linearToSrgb(-0.0041960863 * l - 0.7034186147 * m + 1.707614701 * s),
    a: 1,
  };
}

/**
 * Resolve a token value to concrete sRGB + alpha. Handles literal hex,
 * var(--x) indirection, `color-mix(in oklab, var(--x) NN%, transparent)`
 * (reduces to the base colour at NN% alpha, since CSS premultiplies the mix),
 * and `color-mix(in oklab, var(--x) NN%, var(--y))` (interpolated in OKLab).
 */
function resolveToken(name: string, table: Record<string, string>, seen = new Set<string>()): Rgba {
  if (seen.has(name)) throw new Error(`circular token reference: ${name}`);
  seen.add(name);

  const raw = table[name];
  if (raw === undefined) throw new Error(`token not defined in CSS: ${name}`);

  const solidMix = raw.match(
    /^color-mix\(\s*in\s+oklab\s*,\s*var\((--[a-z0-9-]+)\)\s+(\d+(?:\.\d+)?)%\s*,\s*var\((--[a-z0-9-]+)\)\s*\)$/i,
  );
  if (solidMix) {
    const p = parseFloat(solidMix[2]!) / 100;
    const x = toOklab(resolveToken(solidMix[1]!, table, new Set(seen)));
    const y = toOklab(resolveToken(solidMix[3]!, table, new Set(seen)));
    return fromOklab({ L: x.L * p + y.L * (1 - p), a: x.a * p + y.a * (1 - p), b: x.b * p + y.b * (1 - p) });
  }

  const alphaMix = raw.match(
    /^color-mix\(\s*in\s+\w+\s*,\s*var\((--[a-z0-9-]+)\)\s+(\d+(?:\.\d+)?)%\s*,\s*transparent\s*\)$/i,
  );
  if (alphaMix) {
    const base = resolveToken(alphaMix[1]!, table, new Set(seen));
    return { ...base, a: base.a * (parseFloat(alphaMix[2]!) / 100) };
  }

  const ref = raw.match(/^var\((--[a-z0-9-]+)\)$/i);
  if (ref) return resolveToken(ref[1]!, table, new Set(seen));

  if (/^#[0-9a-f]{3,8}$/i.test(raw)) return hexToRgba(raw);

  throw new Error(`cannot parse token ${name}: ${raw}`);
}

function composite(fg: Rgba, bg: Rgba): Rgba {
  if (fg.a >= 1) return fg;
  return {
    r: fg.r * fg.a + bg.r * (1 - fg.a),
    g: fg.g * fg.a + bg.g * (1 - fg.a),
    b: fg.b * fg.a + bg.b * (1 - fg.a),
    a: 1,
  };
}

function luminance(c: Rgba): number {
  const channel = (v: number) => {
    const s = v / 255;
    return s <= 0.03928 ? s / 12.92 : Math.pow((s + 0.055) / 1.055, 2.4);
  };
  return 0.2126 * channel(c.r) + 0.7152 * channel(c.g) + 0.0722 * channel(c.b);
}

function contrast(fg: Rgba, bg: Rgba): number {
  const l1 = luminance(composite(fg, bg));
  const l2 = luminance(bg);
  const [hi, lo] = l1 >= l2 ? [l1, l2] : [l2, l1];
  return (hi + 0.05) / (lo + 0.05);
}

const pigments = extractVars(themeCss);
const light = { ...pigments, ...extractVars(extractBlock(globalsCss, ':root {')) };
const darkBlock =
  extractBlock(globalsCss, ":root:not([data-theme='light'])") ||
  extractBlock(globalsCss, ':root:not([data-theme="light"])');
const dark = { ...pigments, ...extractVars(darkBlock) };

const CATEGORIES = ['evenimente', 'joburi', 'trafic', 'oameni', 'sondaje'] as const;

const SEMANTIC_PAIRS: Pair[] = [
  { fg: '--text', bg: '--bg', kind: 'body', note: 'body text on page' },
  { fg: '--text', bg: '--surface', kind: 'body', note: 'body text on card' },
  { fg: '--text-muted', bg: '--bg', kind: 'body', note: 'muted text on page' },
  { fg: '--text-muted', bg: '--surface', kind: 'body', note: 'muted text on card' },
  { fg: '--action-text', bg: '--action', kind: 'body', note: 'label on action fill' },
  { fg: '--action-text', bg: '--action-hover', kind: 'body', note: 'label on action hover' },
  { fg: '--action-text', bg: '--action-active', kind: 'body', note: 'label on action active' },
  { fg: '--action', bg: '--bg', kind: 'body', note: 'link text' },
  { fg: '--accent', bg: '--bg', kind: 'body', note: 'secondary accent text' },
  { fg: '--focus-ring', bg: '--bg', kind: 'ui', note: 'focus indicator' },
  { fg: '--focus-ring', bg: '--surface', kind: 'ui', note: 'focus indicator on card' },
  { fg: '--control-border', bg: '--bg', kind: 'ui', note: 'form control boundary' },
  { fg: '--control-border', bg: '--surface', kind: 'ui', note: 'control boundary on card' },
  { fg: '--danger', bg: '--bg', kind: 'ui', note: 'danger stripe/icon — never body copy' },
];

const CATEGORY_PAIRS: Pair[] = [
  { fg: '--linie-on-clar', bg: '--linie-evenimente', kind: 'body', note: 'ink on yellow' },
  { fg: '--linie-on-intens', bg: '--linie-joburi', kind: 'body', note: 'white on blue' },
  { fg: '--linie-on-intens', bg: '--linie-trafic', kind: 'body', note: 'white on red' },
  { fg: '--linie-on-intens', bg: '--linie-oameni', kind: 'body', note: 'white on green' },
  { fg: '--linie-on-clar', bg: '--linie-sondaje', kind: 'body', note: 'ink on orange' },
  ...CATEGORIES.map(
    (c): Pair => ({
      fg: `--linie-${c}`,
      bg: '--bg',
      kind: 'soft',
      note: `${c} marker — colour is never the sole indicator; the tag carries its label`,
    }),
  ),
];

const BANNED: Pair[] = [
  { fg: '--ocru', bg: '--bg', kind: 'soft', note: 'ochre — fills/borders only, never text' },
  { fg: '--piatra', bg: '--bg', kind: 'soft', note: 'cobblestone — decoration only, never text' },
  { fg: '--apa', bg: '--amurg', kind: 'soft', note: 'river — decoration only, never text' },
];

interface Row {
  theme: string;
  label: string;
  ratio: number;
  threshold: number | null;
  pass: boolean | null;
  note: string;
}

function check(theme: string, table: Record<string, string>, pairs: Pair[]): Row[] {
  return pairs.map((p) => {
    const fg = resolveToken(p.fg, table);
    const bg = resolveToken(p.bg, table);
    const row: Row = {
      theme,
      label: `${p.fg} on ${p.bg}`,
      ratio: contrast(fg, bg),
      threshold: null,
      pass: null,
      note: p.note ?? '',
    };
    if (p.kind === 'soft') return row;
    const threshold = THRESHOLD[p.kind];
    row.threshold = threshold;
    row.pass = row.ratio >= threshold;
    return row;
  });
}

const rows: Row[] = [
  ...check('light', light, SEMANTIC_PAIRS),
  ...check('dark', dark, SEMANTIC_PAIRS),
  ...check('light', light, CATEGORY_PAIRS),
  ...check('dark', dark, CATEGORY_PAIRS),
  ...check('light', light, BANNED),
];

const pad = (s: string, n: number) => s.padEnd(n);

let failures = 0;
let soft = 0;

if (process.argv.includes('--json')) {
  const failed = rows.filter((r) => r.pass === false).length;
  const documented = rows.filter((r) => r.threshold === null).length;
  console.log(JSON.stringify({ rows, failures: failed, documented }, null, 2));
  process.exit(failed > 0 ? 1 : 0);
}

console.log('');
console.log(`  ${pad('THEME', 6)}${pad('PAIR', 38)}${pad('RATIO', 8)}${pad('MIN', 6)}RESULT`);
console.log(`  ${'-'.repeat(94)}`);

for (const r of rows) {
  let verdict: string;
  if (r.threshold === null) {
    soft += 1;
    verdict = r.ratio < 3 ? `warn  ${r.note}` : `info  ${r.note}`;
  } else if (r.pass) {
    verdict = 'pass';
  } else {
    verdict = `FAIL (needs ${r.threshold}:1)`;
    failures += 1;
  }
  const min = r.threshold === null ? '—' : r.threshold.toFixed(1);
  console.log(
    `  ${pad(r.theme, 6)}${pad(r.label, 38)}${r.ratio.toFixed(2).padStart(6)}  ${pad(min, 6)}${verdict}`,
  );
}

console.log(`  ${'-'.repeat(94)}`);
console.log(
  `  ${rows.filter((r) => r.pass === true).length} verified · ${soft} documented · ${failures} failing`,
);
console.log('');

if (failures > 0) {
  console.error(`  check:contrast FAILED — ${failures} pair(s) below threshold.`);
  process.exit(1);
}

console.log('  check:contrast passed.');
