# Parallel Work Plan — Piața (CityWeb)

Date: 2026-10-04
Status: actionable. Supersedes the *sequencing* in `docs/ROADMAP.md` (that document remains the scope and risk source of truth).
Canonical rules: `AGENTS.md`. Scope/product: `docs/PLAN.md`. Unverified + legal: `docs/OPEN_ISSUES.md`.

`ROADMAP.md` is a correct **priority** list but a bad **execution** plan: its 18 steps are serial, so N collaborators land N-deep instead of working at once. This document keeps the same scope and re-cuts it into lanes with disjoint file ownership.

---

## 1. Verified ground truth

Everything here was read directly from the tree. Anything not verified is marked UNVERIFIED.

**Merged tree.** The working tree is the union of two branches: backend (`main`) contributed Payload collections, serverless `api/*.js` handlers, `seed/`; UI (`feature/piata-ui`) contributed Vue 3 pages, router, i18n. 223 files.

**Stack is Vue 3 + Vite 6 + Tailwind v4, not Next.js.** `AGENTS.md` §Stack still says Next.js App Router — that line is stale and misleading; treat this document and the actual `package.json` as authoritative. Tailwind v4 is CSS-first (`@theme` in CSS); there is no `tailwind.config.ts`.

**Frontend and backend are not connected.** `src/data/demo.ts` is static, self-described "no backend wiring", and its `jobs`, `polls`, `disruptions`, `persona` exports are imported by no component. No API client exists anywhere in `src/`. `api/events-feed.js` has no callers.

**Field-name mismatch is the integration blocker.** The UI shapes use `when`, `localizedTitle`, `neighborhood`; Payload/Drizzle creates camelCase columns `startDate`, and `place` is a *relationship*, not a scalar. Any adapter must map deliberately — a rename will not survive.

**`lib/graph.ts` is three empty stubs.** `getConnected`, `getNeighborhoodForPerson`, `getAffectedEvents` all `return []`. The product's central differentiator is unimplemented. This is why the four `TS6133` unused-parameter errors exist — they are markers, not lint noise.

**No design system exists.** `src/styles/theme.css` holds a dead Atlassian palette including `#bf63f3` (purple) and `#ffffff`; `src/styles/globals.css` holds an unused shadcn OKLCH greyscale. **Zero components reference any color token** — every component hardcodes Tailwind defaults: `zinc-950/900/800`, `indigo-500`, `text-white`, `from-pink-500`→`to-indigo-500` with `bg-gradient`, `shadow-indigo-500`. The app is dark zinc/indigo with gradient text and glow shadows.

**Five orphan React files in a Vue project.** `src/components/ui/auth-10.tsx`, `src/components/ui/button.tsx`, `src/components/card-split-accordian.tsx`, `src/components/watermelon-ui.ts`, and `./#/components/watermelon/card-split-accordian.tsx`. Nothing imports them. `button.tsx` imports `@base-ui/react/button`, a package absent from every `package.json` — this is the 5th typecheck error.

**Baseline is not green.** Node `v20.20.2`; `jsdom@30.1.2` needs `^22.22.2 || ^24.15.0 || >=26`, `undici@8.11.2` needs `>=22.19.0`. `npm test` cannot run. `typecheck` exits 2 with 5 errors, all from dead code (4 in `lib/graph.ts`, 1 in `ui/button.tsx`). `build` passes. There is no git repository, no CI, and `.gitignore` already covers `node_modules`/`dist`.

**Fonts are vendored and Romanian-safe.** `lib/` holds Bodoni_Moda, Cormorant_Garamond, Libre_Caslon_Text, Lora, Work_Sans. All three checked carry 8/8 required codepoints including comma-below `Ș ț Ș Ț` (U+0218–021B) and `ă â î` (U+0102/0103/00C2/00CE) — verified with fontTools, not assumed. Inter loads from the Google Fonts CDN in `index.html` and is **not** vendored.

**Fixed already.** `payload.config.ts` was missing the `secret` Payload requires; it now reads `PAYLOAD_SECRET` with a dev-only fallback.

---

## 2. What is actually worth building

The strategic case, restated from verified state rather than the timed-out research agents.

**The killer feature is the connection graph, and it does not exist.** `AGENTS.md` states the philosophy — nothing is a standalone listing, a closure affects your event, an event connects you to people and jobs. That is genuinely differentiated: every competitor research row is `TBD`, and no city-map product ships neighborhood-scoped N-vs-N polls plus attendance-derived identity. But the entire mechanism is three functions returning `[]`. Shipping polished cards over an empty graph ships the commodity half of the product and hides the differentiated half.

Ranked, with evidence:

1. **Where/When/Who traversal** — the differentiator. `lib/graph.ts`, `Connection` edges. Currently 0%.
2. **"Acum în București"** ranked feed — needs the graph plus a data layer; both absent.
3. **City passport** — attendance as identity rather than profile. Zero UI, but it is the retention hook and it is cheap once check-ins exist.
4. **Neighborhood N-vs-N polls** — `api/polls.js` is a raw `SELECT *` with no validation; needs work regardless.

**The gap that outranks all of them:** no frontend talks to the backend. Every feature below is blocked on one typed adapter existing. That is why it is Wave 1 and not later.

---

## 3. Dependency graph

```
WAVE 0 — unblock everything (4 tasks, fully parallel)
  T0.1 repo+CI+Node pin ──┬──────────────────────────────┐
  T0.2 Node 24, green base ┤                              │
  T0.3 purge React orphans ┴─→ (must precede T1.2)        │
  T0.4 design tokens ───────────→ (must precede T1.2, T2.1)│
                                                          │
WAVE 1 — build in parallel (4 lanes, disjoint files)       │
  Lane A design system ── T1.1 ──→ T1.2 styleguide         │
  Lane B graph        ── T1.3 (needs T0.2 for tests)       │
  Lane C data plumbing── T1.4 ──→ T1.5                      │
  Lane D research/legal── T1.6, T1.7 (docs only, no deps)   │
                                       │                   │
WAVE 2 — only after Wave 1 merges                         │
  T2.1 retheme existing pages  │ T2.2 consent/auth (launch gate)
```

**Two ordering constraints exist because of file collisions, not logic:**
- `T0.3` purges `src/components/ui/`; `T1.2` writes `src/components/ui/`. Sequential.
- `T0.4` rewrites `src/styles/`; nothing else may touch it. Exclusive.

Everything else is genuinely independent. Lane D has no code dependency at all and can run from day one.

---

## 4. File ownership — the actual rule for parallel work

Merge conflicts in a 5-person team are a discipline problem, not a Git problem. Each path has exactly one owner for the whole plan.

| Path | Sole owner | Others must |
|---|---|---|
| `src/styles/**` | T0.4 | not touch, not reformat |
| `src/components/ui/**` | T1.2 (after T0.3) | not add primitives elsewhere |
| `src/components/{TopBar,EventsGrid,CalendarView,NavBar}.vue` | T2.1 | not restyle early |
| `src/lib/api.ts`, `src/lib/adapters/**` | T1.4 | import, never edit |
| `src/lib/graph.ts`, `__tests__/graph*` | T1.3 | — |
| `lib/graph.ts` | T1.3 | — |
| `src/pages/**`, `src/router/**` | T2.1 | T1.2 adds exactly one route |
| `api/**`, `src/collections/**`, `payload.config.ts` | T1.4/T1.5 | — |
| `.github/**`, `.nvmrc`, `package.json` scripts | T0.1 | — |
| `docs/research/**`, `docs/legal/**` | T1.6/T1.7 | anyone may read |

If a task requires a file it does not own, stop and ask. Do not open a cross-lane PR.

**Branch naming:** `feat/<task-id>-<slug>`, e.g. `feat/T1.4-typed-api-client`. One task per branch. Rebase on `main` before opening a PR.

---

## 5. Tasks

### WAVE 0 — unblock

**T0.1 — Repo, CI, Node pin** · owner: infra · size S · risk Low
Files: `.github/workflows/ci.yml`, `.nvmrc`, `package.json` (engines + scripts), `README.md`.
Do: init git, add remote, pin Node 24, CI running build + typecheck + test + contrast.
Accept: CI green on a clean clone.
Note: `AGENTS.md` says "git push denied". The operator has authorized pushing to `main` for this merge; that override is scoped to this task only.

**T0.2 — Node 24 + green baseline** · size S · risk Low
Do: install Node 24 (`fnm`/`mise` — `nvm` is unavailable in fish), `npm ci`, get `build`, `typecheck`, `test` reporting honestly.
Accept: `npm test` executes. Baseline failures are documented, not hidden.
Blocks: every "tests pass" claim in T1.3+.

**T0.3 — Purge React orphans** · size S · risk Low
Files: delete the 5 orphans listed in §1; drop `react`, `react-dom` from `package.json`.
Accept: `typecheck` errors drop from 5 to 4; `src/components/ui/` empty and ready.

**T0.4 — Design tokens** · size M · risk Med · **exclusive: `src/styles/**`**
Files: `theme.css` (raw palette: `--calcar --ocru --teracota --oblon --piatra --cerneala --roz`, `--linie-evenimente/joburi/trafic/oameni/sondaje`, dark `--amurg --sodiu --tei --hartie --apa`), `globals.css` (semantic tokens on `:root`; dark under **both** `@media (prefers-color-scheme: dark) :root:not([data-theme="light"])` **and** `:root[data-theme="dark"]`), `@font-face` for Bodoni Moda (display) + Lora (body) + Inter (UI, CDN with vendored Work Sans fallback), `@theme inline` mapping to `bg-bg`/`text-text`/`bg-action`/`bg-calcar`/`bg-linie-*`.
Rules: no `#000`/`#FFF` for text or bg, no gradients, no glows, no purple/blue-violet. Metro colors are category markers only, never large backgrounds. Delete `#bf63f3`.
Accept: tokens resolve in both themes; zero components changed.

### WAVE 1 — parallel

**T1.1 → T1.2 — Lane A, design system** · size L · risk Med
T1.1 primitives in `src/components/ui/*.vue`: Button, Link, Input, Textarea, Select, Checkbox, Radio, Switch, Tag/CategoryChip, Card, EventCard, JobCard, PollCard, PersonChip, Alert, Tabs (Acum/Harta/Oameni/Eu), NavBar, Modal, Toast, EmptyState, Skeleton, plus five 24px/1.5px line icons in `currentColor`.
Every component: default, hover, focus-visible, active, disabled. Tokens only, no hex. Radius 2/4px. 1px borders, no shadows. Honor `prefers-reduced-motion`.
T1.2 `src/pages/StyleguidePage.vue` + one route + `scripts/check-contrast.ts` + `check:contrast` script.
Accept: contrast script exits 0 for both themes at 4.5 body / 3.0 large+UI.

**T1.3 — Lane B, the graph** · size M · risk Med · **the differentiated feature**
Files: `lib/graph.ts` (typed edge queries replacing the stubs), `__tests__/graph.test.ts` using `seed/` fixtures.
Do: implement `getConnected`, `getNeighborhoodForPerson`, `getAffectedEvents` against real SQL; keep it a pure module — no graph DB (`AGENTS.md` risk 5).
Accept: unit tests green; the 4 `TS6133` errors disappear as a side effect.
Depends: T0.2.

**T1.4 → T1.5 — Lane C, data plumbing** · size M · risk Med · **unblocks every feature**
T1.4 `src/lib/api.ts` + `src/lib/adapters/events.ts`. Typed client, explicit adapter mapping `startDate`→`when`, `place` relationship→`neighborhood`/`localizedTitle`. Never rename-and-hope.
T1.5 wire the live feed with graceful degradation: on failure fall back to `demo.ts`, surface `lastUpdated`, never block render. Feed sources are UNVERIFIED (`docs/OPEN_ISSUES.md`).
Accept: adapter unit-tested against a fixture payload; UI renders live data or degrades visibly.
Depends: T0.2 for tests.

**T1.6 — Lane D, verify UNVERIFIED** · size S · risk Low · docs only
Priority order from `docs/OPEN_ISSUES.md`: TPBI GTFS/RT access and terms; tile provider terms; B365 event structure; OSM sector boundaries. Confirm Metrorex M1–M5 official `route_color` values against the approximations in T0.4 and correct them if they differ.
Accept: each item moves out of UNVERIFIED or is re-flagged with evidence.

**T1.7 — Lane D, LEGAL-REVIEW memos** · size S · risk Low · docs only, starts now because it blocks launch not just features
Write `docs/legal/*.md` for: scraping vs partner feeds; check-in/attendance GDPR + retention; "open to chat" opt-in; cookie consent; image hotlinking; attribution storage.
Accept: each has a recommendation and an owner. No code until resolved.

### WAVE 2 — after Wave 1 merges

**T2.1 — Retheme existing pages onto tokens** · size M · risk Low
Owner of the 4 components + 3 pages + router. Remove zinc/indigo/pink-gradient/glow. This is when the app starts looking like Piața.
Accept: no raw palette utility remains in `src/`.

**T2.2 — Consent + auth** · size M · risk High · LEGAL-REVIEW · launch gate
Depends on T1.7.

---

## 6. Merge order

Strict, because two pairs collide:
1. T0.1, T0.2 (infra — unblocks CI for everyone)
2. T0.3 → **then** T0.4 (purge before primitives; tokens before any restyle)
3. Wave 1 in any order among themselves: T1.1/T1.2, T1.3, T1.4/T1.5, T1.6, T1.7
4. T2.1, then T2.2

---

## 7. Definition of done

Every task: rebased on `main`, no file outside its ownership row, no new dependency without asking, `build` passes, `typecheck` no new errors, tests present for logic (not just markup), docs updated if it changed a decision, and any unverified claim still marked UNVERIFIED.

Definition of demo-ready: Wave 0 and Wave 1 merged, the live feed renders or degrades visibly, and the graph returns real edges rather than `[]`.

---

## 8. OUT OF SCOPE for this plan

Native apps; follower feeds or DMs; PostGIS (point + bounding box only); graph database; languages beyond ro-RO; notifications beyond the digest; any competitor content copied; scraping beyond public listings and schema.org metadata; any color, font, or dependency not listed in the design spec without asking.
