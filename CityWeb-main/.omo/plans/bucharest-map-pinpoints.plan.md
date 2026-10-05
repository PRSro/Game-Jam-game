ULW-PLAN MODE - PLAN DURABLE

## Plan: bucharest-map-pinpoints

### Affected user & ideal state (from draft)

- **User**: Visitor wanting event pinpoints (locations/addresses) displayed on Bucharest map tab
- **Ideal state**: Map tab shows Bucharest SVG with clickable/hoverable pinpoints per event, each with address/neighborhood label; clicking pinpoint opens event detail or shows address tooltip
- **Gap vs today**: Map tab exists (from previous plan) but shows only static SVG with no event locations; demo data has no coordinates/addresses

### Intent verdict

**UNCLEAR** - user requested pinpoints without specifying data model changes, coordinate system, or map library. This is a forkful-of-owner-decisions item. Per UNCLEAR routing: best-practice defaults adopted, `review_required: true`, high-accuracy review auto-run after approval.

### Execution tasks (column-zero Markdown rows, nested `Recommended task executor category:`)

1. **[data]** Add `address?: string;` and `localizedAddress?: string;` optional fields to Event interface in `src/data/demo.ts`; add sample addresses for 3 neighborhoods
   - Recommended category: `quick`

2. **[pinpoints]** Create `src/components/MapPinpoint.vue` – SVG `<circle>` element with radius 6px, stroke `#1868db` (Atlassian Blue), fill transparent, `aria-label` with address; position via `cx`/`cy` with neighborhood-based manual coordinates
   - Recommended category: `visual-engineering`

3. **[map]** Update `src/pages/BucharestMap.vue` – render pinpoints inside `<object>` by mapping neighborhood to `(cx, cy)` coords; display 1 pin per neighborhood initially (Floreasca, Drumul Taberei, Centru)
   - Recommended category: `visual-engineering`

4. **[i18n]** Add `event.address:` (ro: "Adresă") and `"Address"` (en) to `src/i18n/index.ts` record sections
   - Recommended category: `writing`

5. **[test]** Run `npm run lint` and `npm test`; add pinpoint render test verifying 3 circles appear with correct aria-labels
   - Recommended category: `quick`

### Out of scope (for THIS plan)

- Real map tiles or vector tile layer
- Backend database integration (keep static demo data)
- Interactive pinpoint clicking (open details on hover only)
- Geocoding API calls (street → coordinates)
- Redrawing the SVG geometry
- Making pinpoints fully interactive (click → detail panel)

### Risks & mitigations (top 5, from draft)

- **R1**: SVG has no georeferencing → pinpoints may not align with real Bucharest. Mitigation: Use simple neighborhood-based positioning (3 circles for 3 neighborhoods) or adopt a map library later.
- **R2**: Adding coordinates to demo data breaks "demo data, no backend" contract. Mitigation: Mark as placeholder/demo data; add `?` optional fields.
- **R3**: Pinpoint clutter on small SVG → overlapping labels. Mitigation: Cluster by neighborhood, show tooltip on hover, limit to events per neighborhood.
- **R4**: No map library currently → extra dependency if choosing Leaflet/Mapbox. Mitigation: Start with raw SVG pinpoints only; library can be added in Phase 2.
- **R5**: i18n address formatting differences (ro vs en street numbers, formats). Mitigation: Keep address as raw string; format via i18n later.

### Final verification wave (after implementation)

- [ ] Data model: Event has optional `address` and `localizedAddress` fields
- [ ] Pinpoints: 3 circles render on map (one per neighborhood)
- [ ] i18n: address labels ro↔en update correctly
- [ ] No lint errors; all tests pass
- [ ] SVG build succeeds

### High-accuracy review required: YES

This plan is approved for execution. The durable draft was written to `.omo/drafts/bucharest-map-pinpoints.plan.md`. Execution proceeds via `$ulw-execute` or direct task delegation.
