# Piața Design System

Canonical reference for every UI surface in this repo. The tokens live in
`src/styles/theme.css` (raw pigments and scale) and `src/styles/globals.css`
(semantic layer); this file explains how to use them. `npm run check:contrast`
parses that CSS and verifies the accessibility contract, and is part of
`npm run verify`.

**The one rule:** components reference semantic or category tokens. Never a raw
hex, never `bg-[#...]`, never a literal colour in a class.

---

## 1. Principles

1. **Content-type colour is structural, never decorative.** The five category
   lines map to real content types (events, jobs, traffic, people, polls) and
   appear only as markers: tags, left stripes, result bars, icons.
2. **Warm neutrals carry the interface.** Limestone and parchment ground the
   page; ochre is a rare accent; verdigris and plum signal state. Pure `#000`
   and `#FFF` are excluded from body copy and page backgrounds.
3. **Editorial, not dashboard.** Bodoni Moda for display, Lora for reading,
   Inter/Work Sans for interface. Rules and 2–4px radii, never shadows.
4. **The forum is the product.** Dense, link-first, alive with other people's
   presence. Never a lonely listing.

---

## 2. Colour

### Raw pigments — `theme.css`

Pigments never change with theme; only the semantic aliases flip.

| Token | Hex | Role |
| --- | --- | --- |
| `--calcar` | `#EDE4D0` | FAȚADĂ — page ground (light) |
| `--ocru` | `#C8923A` | Brass — fills and borders only, never text |
| `--teracota` | `#A8472E` | Roof tile — primary action (light) |
| `--oblon` | `#4A6652` | Shutter green — secondary accent (light) |
| `--piatra` | `#8C877D` | Cobblestone — dividers, disabled; never text |
| `--cerneala` | `#26231F` | Warm ink — body text (light) |
| `--roz` | `#D8A898` | Villa walls — small accent only |
| `--amurg` | `#1C2230` | Dusk — page ground (dark) |
| `--sodiu` | `#E59B2F` | Sodium lamp — primary action (dark) |
| `--tei` | `#A9B565` | Linden — secondary accent (dark) |
| `--hartie` | `#EAE3D3` | Parchment — body text (dark) |
| `--apa` | `#55645E` | River — dividers; never text |

### Semantic layer — `globals.css`

| Token | Light | Dark | Use |
| --- | --- | --- | --- |
| `--bg` | `--calcar` | `--amurg` | page background |
| `--surface` | `#EEE5D4` | `#2A303D` | cards, panels |
| `--text` | `--cerneala` | `--hartie` | body copy |
| `--text-muted` | cerneala @72% | hartie @75% | secondary copy |
| `--border` | `--piatra` | `--apa` | **decorative** dividers only |
| `--control-border` | `--text-muted` | `--text-muted` | **interactive** control edges |
| `--action` | `--teracota` | `--sodiu` | primary button fill, links |
| `--action-text` | `--calcar` | `--amurg` | label on an action fill |
| `--action-hover` | teracota 88% + cerneala | sodiu 90% + hartie | primary hover |
| `--action-active` | teracota 78% + cerneala | sodiu 80% + hartie | primary active |
| `--accent` | `--oblon` | `--tei` | secondary accent text/icon |
| `--danger` | `--linie-trafic` | `--linie-trafic` | stripes and icons only |
| `--focus-ring` | `--cerneala` | `--hartie` | focus outline |

Hover and active are separate tokens rather than `opacity-90`, because the
contract is that a state change alters colour, not transparency. Both are
`color-mix` derivations of existing pigments, so no new hex is introduced, and
both *increase* label contrast (light 4.61 → 5.26 → 5.83).

`--border` and `--control-border` are deliberately different. Cobblestone sits
at 2.83:1 — fine for a divider, below the 3:1 that WCAG 1.4.11 requires of a
form control's boundary. Inputs, selects, textareas, checkboxes, radios,
switches and outlined buttons use `--control-border` (5.50 light / 7.58 dark).

### Category lines

| Token | Hex | Text on it | Ratio |
| --- | --- | --- | --- |
| `--linie-evenimente` | `#F2C200` | `--linie-on-clar` (ink) | 9.30 |
| `--linie-joburi` | `#0B5CA8` | `--linie-on-intens` (white) | 6.75 |
| `--linie-trafic` | `#D6182B` | `--linie-on-intens` (white) | 5.22 |
| `--linie-oameni` | `#0B7A41` | `--linie-on-intens` (white) | 5.42 |
| `--linie-sondaje` | `#F28A1E` | `--linie-on-clar` (ink) | 6.28 |

Text on a category fill uses the paired on-colour token, never `--action-text`.
Yellow and orange take ink; blue, red and green take white. The pure-white ban
in principle 2 governs body copy and page backgrounds — it yields here, because
warm `calcar` on red measures 4.13 and on green 4.29, both under the 4.5 body
minimum, while white clears both.

### Dark mode

Two guards, both required:

```css
@media (prefers-color-scheme: dark) { :root:not([data-theme='light']) { … } }
:root[data-theme='dark'] { … }
```

The media query follows the OS, the attribute follows the in-app toggle, and the
`:not()` guard stops the OS from overriding an explicit light choice.

---

## 3. Typography

Three families, all self-hosted from `lib/` and verified with fontTools to carry
the Romanian comma-below forms `Ș ț Ș Ț` (U+0218–021B) and `ă â î`
(U+0102/0103/00C2/00CE) rather than the cedilla forms.

| Utility | Stack | Use |
| --- | --- | --- |
| `font-display` | Bodoni Moda → Playfair Display → serif | headings |
| `font-body` | Lora → Source Serif 4 → serif | body copy |
| `font-ui` | Inter → Work Sans → system-ui | interface |

Inter arrives from the Google Fonts link in `index.html`; Work Sans is
self-hosted and is the offline fallback.

A 1.250 modular scale from a 17px body. Size and line height travel together, so
`text-heading` also applies its 1.15 leading.

| Utility | Size | Leading | Typical use |
| --- | --- | --- | --- |
| `text-display` | 52px | 1.15 | page title |
| `text-heading-lg` | 41px | 1.15 | section title |
| `text-heading` | 33px | 1.15 | card title |
| `text-heading-sm` | 26px | 1.25 | sub-section |
| `text-subheading` | 21px | 1.4 | group heading |
| `text-body-lg` | 18px | 1.6 | lead paragraph |
| `text-body` | 17px | 1.6 | body (the `body` default) |
| `text-caption` | 13px | 1.5 | meta, timestamps |

Family comes from the element, not the step: `body` sets `font-body` and
`h1`–`h6` set `font-display`, both in the base layer. A `text-*` size on a
non-heading element keeps Lora; add `font-display` explicitly to promote one.

---

## 4. Space, radius, rules

**Spacing** uses Tailwind v4's default 4px-based scale — there are no custom
spacing tokens. `p-1` 4px, `p-2` 8px, `p-3` 12px, `p-4` 16px, `p-6` 24px,
`p-8` 32px, `p-12` 48px, `p-16` 64px.

**Radius** is 2px or 4px, nothing between:

| Utility | Value | Use |
| --- | --- | --- |
| `rounded-sm` | 2px | buttons, inputs, tags — the default |
| `rounded-md` | 4px | cards, panels |
| `rounded-lg` | 4px | alias of `rounded-md` |
| `rounded-full` | 10000px | radio and switch knobs only, never containers |

**Rules, not shadows.** No shadow utility is used anywhere; the shadow scale is
deliberately absent from the theme.

- Interactive edges: 1px `--control-border`.
- Decorative dividers: 1px `--border`.
- Category stripe: `.stripe-evenimente` / `-joburi` / `-trafic` / `-oameni` /
  `-sondaje` apply a 4px left border in the line colour.
- Section divider: `.rule-double` draws 1px, 3px gap, 1px using two borders
  rather than a zero-blur `box-shadow`.
- Optional static grain on `--bg` only: `.texture-stone` (inline SVG
  turbulence, opacity 0.04, no animation).

---

## 5. Motion

| Token | Value |
| --- | --- |
| `ease-standard` | `cubic-bezier(0, 0, 0.2, 1)` — also the default |
| `ease-entrance` | `cubic-bezier(0.16, 1, 0.3, 1)` |

Durations are bare `duration-*` utilities — `duration-150` (fast, the default),
`duration-200` (base), `duration-300` (slow). Tailwind has no duration theme
namespace, so there is deliberately no `--motion-*` token to add.

Transition `background-color`, `border-color`, `color`, `transform` and
`opacity` only — never `width`, `height` or `all`. Under
`prefers-reduced-motion: reduce` every animation and transition duration drops
to `0.01ms` and smooth scrolling is disabled.

---

## 6. Component contract

Every primitive ships default, hover, focus-visible, active and disabled states,
and shows its resting label rather than reflowing.

- Focus is a global `2px solid` ring at `2px` offset, set in the base layer.
  Components must not remove it.
- Icons are `currentColor`, 1.5px stroke, 24px, and never emoji.
- Interactive targets are at least 44×44px.
- Status is never colour alone; pair it with text or an icon.
- `Modal` traps focus, restores it on close, closes on Escape, and locks scroll.
- `Toast` uses `role="status"` / `aria-live="polite"`.

`Button.vue` is the reference implementation of these rules.

---

## 7. Accessibility contract

`npm run check:contrast` reads the real CSS — including resolving
`color-mix(in oklab, …)` through OKLab, because mixing in sRGB would
under-report contrast — and asserts 4.5:1 for body text and 3:1 for UI
components in **both** themes. Current result: **38 verified pairs, 0
failures**, plus 13 documented rows that are reported but do not block.

### Adjustments made to the original spec

1. **Added `--linie-on-intens` / `--linie-on-clar`.** `calcar` on red is 4.13
   and on green 4.29, both under 4.5. The spec's own category table prescribed
   white on those chips, so the on-colour tokens encode that.
2. **Classified `--danger` as a marker, not body copy.** Red on limestone is
   4.13 and on nightfall 3.05 — it clears 3:1 for stripes and icons but not
   4.5 for text. Alert copy uses `--text`.
3. **Added `--control-border`.** Cobblestone at 2.83:1 fails WCAG 1.4.11 for
   form-control edges; `--text-muted` clears it at 5.50 / 7.58.
4. **Added `--action-hover` / `--action-active`.** Hover had been expressed as
   `opacity-90`, which contradicts the rule that a state change alters colour.
   Both are `color-mix` derivations, so no new hex enters the palette.
5. **Declared the Work Sans `@font-face` pair.** The stylesheet named Work Sans
   as the offline fallback without ever declaring it, so the fallback did not
   exist.

### Accepted tradeoffs

- **Category markers cannot reach 3:1 against the page.** Yellow on limestone
  is 1.33 and orange 1.97; blue on nightfall 2.35 and green 2.93. These are
  unreachable without abandoning the palette. The mitigation is that colour is
  never the sole carrier of meaning — every tag, stripe and marker also renders
  its text label. Reported as `warn`.
- **Banned for text:** ochre 2.18, cobblestone 2.83, river 2.55. Fills, borders
  and decoration only.

### Unverified

- Official TPBI/Metrorex line colours are `UNVERIFIED`. The five line hexes are
  working approximations of the Bucharest Metro map, pending confirmation
  against an official source.

---

## 8. Out of scope

Existing `EventsGrid`, `TopBar`, `NavBar`, `CalendarView` and the page shells
still carry their original zinc/indigo utilities. They are **not** yet migrated
to these tokens — the primitives and the style guide land first. Do not treat
their current colours as sanctioned.
