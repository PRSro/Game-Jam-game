ULW-PLAN MODE ENABLED!
From now on I am working as Prometheus, a planning consultant. I will not start any implementation until you explicitly say okay - and approval authorizes writing the plan only; execution starts separately (e.g. `$ulw-execute`).

## Affected user & ideal state

- **User**: Visitor wanting event pinpoints (locations/addresses) displayed on Bucharest map tab
- **Ideal state**: Map tab shows Bucharest SVG with clickable pinpoints per event, each with address/neighborhood label; clicking pinpoint opens event detail or shows address tooltip
- **Gap vs today**: Map tab exists (from previous plan) but shows only static SVG with no event locations; demo data has no coordinates/addresses

## Intent verdict

**UNCLEAR** - user requested pinpoints without specifying data model changes, coordinate system, or map library. This is a forkful-of-owner-decisions item. I will adopt best-practice defaults, set `review_required: true`, and auto-run high-accuracy review after approval per UNCLEAR routing rules.

## Explored facts (repo truth)

- Map tab already implemented: `src/pages/BucharestMap.vue` with `<object data="../lib/bucharest.svg">`
- SVG at `lib/bucharest.svg`: 210mm×297mm A4 viewBox="0 0 210 297", clipPath + PNG data URI overlay
- Events in `src/data/demo.ts`: have `neighborhood` (floreasca/drumul-taberei/old-town) but NO coordinates, addresses, or lat/lng
- No map library in use (raw SVG only, Vite build confirmed working)
- No address field in event interface
- Router has `/harta` route; NavBar has map link; i18n has `nav.harta` keys

## Gap list (every gap must be closed by the plan)

1. **Data model**: Add `address` and/or `lat/lng` fields to Event interface in `src/data/demo.ts`
2. **Coordinate system**: Determine how to map coordinates to SVG viewBox (raw SVG has no inherent georeferencing; options: manual mapping, simple neighborhood circles, or map library)
3. **Pinpoint rendering**: Create pinpoint SVG elements (circles) positioned on map, with address labels
4. **Event-to-pinpoint mapping**: Link each event to its pinpoint via ID or neighborhood
5. **i18n**: Address formatting for ro and en
6. **Testing**: Verify pinpoints render, tooltips work, no regressions

## Risks & mitigations (top 5)

- **R1**: SVG has no georeferencing → pinpoints may not align with real Bucharest. Mitigation: Use simple neighborhood-based positioning (3 circles for 3 neighborhoods) or adopt a map library later.
- **R2**: Adding coordinates to demo data breaks "demo data, no backend" contract. Mitigation: Mark as placeholder/demo data; add `?` optional fields.
- **R3**: Pinpoint clutter on small SVG → overlapping labels. Mitigation: Cluster by neighborhood, show tooltip on hover, limit to events per neighborhood.
- **R4**: No map library currently → extra dependency if choosing Leaflet/Mapbox. Mitigation: Start with raw SVG pinpoints only; library can be added in Phase 2.
- **R5**: i18n address formatting differences (ro vs en street numbers, formats). Mitigation: Keep address as raw string; format via i18n later.

## Out of scope (for THIS plan)

- Real map tiles or vector tile layer
- Backend database integration (keep static demo data)
- Interactive pinpoint clicking (open details on hover only)
- Geocoding API calls (street → coordinates)
- Redrawing the SVG geometry

## Plan (column-zero Markdown rows, nested `Recommended task executor category:`)

1. **[data]** Add `address?: string;` and `localizedAddress?: string;` optional fields to Event interface in `src/data/demo.ts`; add sample addresses for 3 neighborhoods
   - Recommended category: `quick`

2. **[pinpoints]** Create `src/components/MapPinpoint.vue` – SVG `<circle>` element with radius 6px, stroke #1868db (Atlassian Blue), fill transparent, `aria-label` with address; position via `cx`/`cy` with neighborhood-based manual coordinates
   - Recommended category: `visual-engineering`

3. **[map]** Update `src/pages/BucharestMap.vue` – render pinpoints inside `<object>` by mapping neighborhood to `(cx, cy)` coords; display 1 pin per neighborhood initially (Floreasca, Drumul Taberei, Centru)
   - Recommended category: `visual-engineering`

4. **[i18n]** Add `event.address:` (ro) and `"Address"` (en) to `src/i18n/index.ts` record sections
   - Recommended category: `writing`

5. **[test]** Run `npm run lint` + `npm test`; add pinpoint render test verifying 3 circles appear with correct aria-labels
   - Recommended category: `quick`

### Final verification wave (after implementation)

- [ ] Data model: Event has optional `address` and `localizedAddress` fields
- [ ] Pinpoints: 3 circles render on map (one per neighborhood)
- [ ] i18n: address labels ro↔en update correctly
- [ ] No lint errors; all tests pass
- [ ] SVG build succeeds

### High-accuracy review required: YES

This plan is presented for approval. Once approved, I will write the durable plan to `.omo/plans/` and the user can run `$ulw-execute` to implement. No code changes are made until explicit okay.

---

Status: awaiting-approval. Please confirm with "okay" to write durable plan, or "keep static" to retain the current map--only tab without pinpoints.
