ULW-PLAN MODE - PLAN DURABLE

## Plan: bucharest-map-tab

### Affected user & ideal state (from draft)

- **User**: End-cityweb visitor wanting to explore Bucharest map separately from events calendar
- **Ideal state**: A new "Harta" (Map) tab in navigation renders the Bucharest SVG at proper size, with Romanian/English labels, accessible, and not breaking existing calendar/events flows
- **Gap vs today**: No map tab exists; clicking it causes 404; SVG is in `lib/` but not referenced anywhere

### Intent verdict

**CLEAR** - user specified the endpoint (Bucharest map as separate tab) and asked for high accuracy. Review required - dual high-accuracy review (native `momus` + Oracle) runs after approval.

### Execution tasks (column-zero Markdown rows, nested `Recommended task executor category:`)

1. **[router]** Add `/harta` route to `src/router/index.ts` with name "harta", component loading `../pages/BucharestMap.vue`
   - Recommended category: `quick`

2. **[page]** Create `src/pages/BucharestMap.vue` – embed `<svg>` tag referencing `../lib/bucharest.svg` with `width="100%" height="auto"` appropriate styling; add `aria-label` for screen readers
   - Recommended category: `visual-engineering`

3. **[nav]** Add `{ to: '/harta', key: 'nav.harta' }` entry to `NavBar.vue` links array
   - Recommended category: `quick`

4. **[i18n]** Add `nav.harta: "Harta"` (ro) and `"Map"` (en) to both `ro:` and `en:` record sections in `src/i18n/index.ts`
   - Recommended category: `writing`

5. **[test]** Run `npm run lint` and `npm test` to verify no regressions; visually verify map renders at correct size in Chrome dev tools
   - Recommended category: `quick`

### Out of scope (unchanged from draft)

- Making the map interactive (click neighborhoods, tooltips) – static display only
- Fetching real map tiles or vector data – using provided static SVG only
- Adding search/filter to map – pure visualization
- Modifying backend/db – this is a frontend tab only
- Resizing/re-drawing the SVG geometry – use as-is, only adjust display attributes

### Risks & mitigations (top 5, from draft)

- **R1**: SVG viewBox "0 0 210 297" is fixed A4 paper units → map may appear tiny or huge on web. Mitigation: Set `width: 100%`/`max-width: 800px` in CSS, preserveAspectRatio, and adjust viewBox to meaningful Bucharest bounds (e.g., "0 0 1000 1000") or scale via `viewBox` attr.
- **R2**: PNG data URI overlay is 20KB base64 → inline SVG size. Mitigation: Extract PNG to separate image or remove overlay if only map outline needed; or use `fetch` to load image separately.
- **R3**: ClipPath may clip content unexpectedly → map boundaries wrong. Mitigation: Test rendered output; if clipPath causes issues, remove `clip-path` CSS or adjust SVG markup.
- **R4**: No i18n for "Map" tab → language switch doesn't label tab. Mitigation: Add `nav.harta` keys to both ro and en in `src/i18n/index.ts`.
- **R5**: Tab addition breaks navigation flow or routing. Mitigation: Add route last; run `npm run lint` + `npm test` after each change.

### Final verification wave (after implementation)

- [ ] Router: `/harta` renders BucharestMap component without errors
- [ ] NavBar: "Harta" (ro) / "Map" (en) appears and activates active state
- [ ] SVG renders at reasonable size (not microscopic, not overflowing)
- [ ] i18n: language switch ro↔en updates tab label
- [ ] No lint errors; all tests pass

### High-accuracy review required: YES

This plan is approved for execution. The durable draft was written to `.omo/drafts/bucharest-map.plan.md`. Execution proceeds via `$ulw-execute` or direct task delegation.
