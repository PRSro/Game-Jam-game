ULW-PLAN MODE ENABLED!
From now on I am working as Prometheus, a planning consultant. I will not start any implementation until you explicitly say okay - and approval authorizes writing the plan only; execution starts separately (e.g. `$ulw-execute`).

## Affected user & ideal state

- **User**: End-cityweb visitor wanting to explore Bucharest map separately from events calendar
- **Ideal state**: A new "Harta" (Map) tab in navigation renders the Bucharest SVG at proper size, with Romanian/English labels, accessible, and not breaking existing calendar/events flows
- **Gap vs today**: No map tab exists; clicking it causes 404; SVG is in `lib/` but not referenced anywhere

## Intent verdict

**CLEAR** - user specified the endpoint (Bucharest map as separate tab) and asked for high accuracy. I will ask only the genuine forks (SVG display sizing, interactivity level), then run high-accuracy review after approval.

## Explored facts (repo truth)

- Router at `src/router/index.ts` has routes: `/evenimente`, `/calendar`, `/:pathMatch(*)`
- NavBar at `src/components/NavBar.vue` links to `/evenimente` and `/calendar`
- TopBar at `src/components/TopBar.vue` has calendar link and search
- SVG at `lib/bucharest.svg` is 210mm x 297mm, viewBox="0 0 210 297", has clipPath + PNG data URI overlay
- i18n at `src/i18n/index.ts` has ro/en messages; needs new keys
- Demo data at `src/data/demo.ts` - no map data needed (static SVG)

## Gap list (every gap must be closed by the plan)

1. **Route**: Add `/harta` route to router → user sees map page
2. **Page**: Create `BucharestMap.vue` page component displaying the SVG
3. **NavBar**: Add map link to NavBar (v-for or new entry)
4. **i18n**: Add `nav.harta` key for both ro and en
5. **SVG display**: Embed SVG in page, adjust viewBox/sizing for web (currently A4 paper size)
6. **TopBar**: Consider if map needs a toggle in header (decide NO - tab suffices)

## Risks & mitigations (top 5)

- **R1**: SVG viewBox "0 0 210 297" is fixed A4 paper units → map may appear tiny or huge on web. **Mitigation**: Set `width: 100%`/`max-width: 800px` in CSS, preserveAspectRatio, and adjust viewBox to meaningful Bucharest bounds (e.g., "0 0 1000 1000") or scale via `viewBox` attr.
- **R2**: PNG data URI overlay is 20KB base64 → inline SVG size. **Mitigation**: Extract PNG to separate image or remove overlay if only map outline needed; or use `fetch` to load image separately.
- **R3**: ClipPath may clip content unexpectedly → map boundaries wrong. **Mitigation**: Test rendered output; if clipPath causes issues, remove `clip-path` CSS or adjust SVG markup.
- **R4**: No i18n for "Map" tab → language switch doesn't label tab. **Mitigation**: Add `nav.harta` keys to both ro and en in `src/i18n/index.ts`.
- **R5**: Tab addition breaks navigation flow or routing. **Mitigation**: Add route last; run `npm run lint` + `npm test` after each change.

## Out of scope

- Making the map interactive (click neighborhoods, tooltips) – static display only
- Fetching real map tiles or vector data – using provided static SVG only
- Adding search/filter to map – pure visualization
- Modifying backend/db – this is a frontend tab only
- Resizing/re-drawing the SVG geometry – use as-is, only adjust display attributes

## Plan (numbered tasks, column-zero Markdown rows)

1. **[router]** Add `/harta` route to `src/router/index.ts` with name "harta", component loading `../pages/BucharestMap.vue`
2. **[page]** Create `src/pages/BucharestMap.vue` – `<template>` embeds `<svg>` tag referencing `../lib/bucharest.svg` with `width="100%" height="auto"` appropriate styling; add `aria-label` for screen readers
3. **[nav]** Add `{ to: '/harta', key: 'nav.harta' }` entry to `NavBar.vue` links array
4. **[i18n]** Add `nav.harta: "Harta"` (ro) and `"Map"` (en) to both `ro:` and `en:` record sections in `src/i18n/index.ts`
5. **[test]** Run `npm run lint` and `npm test` to verify no regressions; visually verify map renders at correct size in Chrome dev tools

## Final verification wave (after implementation)

- [ ] Router: `/harta` renders BucharestMap component without errors
- [ ] NavBar: "Harta" (ro) / "Map" (en) appears and activates active state
- [ ] SVG renders at reasonable size (not microscopic, not overflowing)
- [ ] i18n: language switch ro↔en updates tab label
- [ ] No lint errors; all tests pass

## Approval gate

Exploration exhausted, unknowns answered. Record `status: awaiting-approval`. Wait for user's explicit okay before writing `.omo/plans/<slug>.md` and task delegation.

---

Next: Present this plan to user, ask for okay. Once approved, I will write the durable plan and the user can run `$ulw-execute` to implement.
