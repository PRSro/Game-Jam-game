# ROADMAP - Piața (Bucharest living map)

Status: PROPOSED - awaiting approval, no code for phases below yet
Date: 2026-10-04
Supersedes: nothing. `docs/PLAN.md` remains the product/Phase-1 scope document.
Note: `docs/PLAN.md` still says "no app code yet" while `src/` contains a working events UI (branch `feature/piata-ui`). PLAN.md status line is stale and is corrected in step 2.

## Current state (UI shipped today)

- `src/components/TopBar.vue` - brand, search, calendar toggle
- `src/components/EventsGrid.vue` - event cards, sort/category filter, search, detail modal
- `src/components/CalendarView.vue` - month grid + day agenda
- `api/events-feed.js` - serverless JSON-LD scraper, no callers yet
- `src/data/demo.ts` - `jobs`, `polls`, `disruptions`, `persona` are exported but imported by no component

Everything else in the MVP scope has zero UI: onboarding, live feed, map, places, RSVP, QR check-in, businesses, jobs, disruptions, polls, passport, neighborhood identity, 30-day checklist, connection traversal, consent.

## Order principle

Router -> data layer -> persona spine -> signature features -> polish.

Personalization gates ranking: feed ranking needs prefs + places + disruptions, so those land before the feed. Check-in needs event state + consent. Map needs places + geo. Consent work starts in parallel with the first LEGAL-REVIEW item because it blocks launch, not just one feature.

---

## Phase 0 - Foundations

1. **Router + nav shell**
   Files: `package.json` (add `vue-router`), `src/router/index.ts`, `src/components/NavBar.vue`, `src/App.vue` (rewrite).
   Risk: Low. Test: build + route walk (deep link, back button). Size: S.
   Replaces the `view` ref switch in `App.vue`.

2. **Quality gates**
   Files: `package.json` (`lint`, `typecheck`, `test` scripts), eslint config, `vue-tsc`, `vitest`, `docs/QUALITY.md` (perf + a11y budgets), `docs/PLAN.md` (status fix).
   Risk: Low. Size: S.
   `AGENTS.md` documents `npm test` / `npm run lint` / `npm run typecheck`; none of them exist today.

3. **i18n + app shell correctness**
   Files: `src/i18n/ro.ts`, `src/i18n/en.ts`, `index.html` (`lang="ro-RO"`, real title, favicon), `src/styles/globals.css` (remove dead `Charlie*.woff2` refs).
   Risk: Low. Size: S.
   Romanian strings are currently hardcoded inside SFCs, which blocks the Phase-2 English layer.

4. **Dead code purge**
   Files: `src/views/` (`HomeFeed.vue`, `Onboarding.vue` - unresolvable `#/components/piața/button` imports, unimported `Neighborhood` type), `src/components/piata/` (8 unused primitives).
   Risk: Low. Size: S.
   Delete, or deliberately adopt one component at a time. Do not keep both paths.

## Phase 1 - Data layer

5. **API client + adapter**
   Files: `src/lib/api.ts`, `src/lib/adapters/events.ts` (feed shape -> `Event`).
   Risk: Med. Test: unit (adapter with fixture payload). Size: M.

6. **Async state primitive**
   Files: `src/composables/useAsyncData.ts`, `src/components/Skeleton.vue`, `src/components/ErrorState.vue`.
   Risk: Low. Test: unit. Size: S.
   Must expose `lastUpdated` for the graceful-degradation rule.

7. **Wire the live feed**
   Files: `src/App.vue`, `src/components/EventsGrid.vue`, `src/components/CalendarView.vue`.
   Risk: Med. Test: unit + manual. Size: M.
   Demo fallback when `/api/events-feed` fails; the UI must degrade, never hard-fail, because the feed sources are UNVERIFIED.

## Phase 2 - Persona spine

8. **Onboarding, 4 taps**
   Files: `src/views/OnboardingView.vue`, `src/lib/prefs.ts`.
   Risk: Low. Test: E2E. Size: M.
   neighborhood/sector, commute, 3-5 interests, intent.

9. **Neighborhood + Place pages**
   Files: `src/views/NeighborhoodView.vue`, `src/views/PlaceView.vue`, `src/lib/graph.ts` (connection traversal).
   Risk: Med. Test: unit (traversal). Size: M.
   Where/When/Who anchors; every item exposes outgoing links.

10. **City passport + 30-day checklist**
    Files: `src/views/PassportView.vue`, `src/lib/passport.ts`.
    Risk: Low. Test: unit + E2E. Size: M.

## Phase 3 - Signature features (highest risk, last)

11. **Map, lightweight/static first**
    Files: `src/components/map/*`, `src/lib/geo.ts`.
    Risk: HIGH (tile provider terms + attribution, performance). LEGAL-REVIEW. Test: perf budget. Size: M.

12. **RSVP + QR check-in**
    Files: `api/rsvp.js`, `api/checkin.js`, check-in UI in event detail.
    Risk: HIGH (GDPR retention, rate limiting, QR validation). LEGAL-REVIEW. Test: E2E. Size: M.

13. **Traffic disruptions + commute**
    Files: `api/disruptions.js`, GTFS ingest, commute UI.
    Risk: HIGH (GTFS / GTFS-RT endpoints and terms UNVERIFIED). Test: unit with fixture feed. Size: L.

14. **Polls**
    Files: `api/polls.js` (rewrite - currently a raw `SELECT *` with no validation), `src/views/PollsView.vue`.
    Risk: Med. Test: unit. Size: M.

15. **Jobs + businesses directory**
    Files: `api/jobs.js`, `api/businesses.js`, `src/views/{JobsView,BusinessesView}.vue`.
    Risk: Med (source terms). LEGAL-REVIEW. Size: L.

16. **"Acum in Bucuresti" ranked feed**
    Files: `src/views/FeedView.vue`, `src/lib/rank.ts`.
    Risk: Med. Test: unit (ranking) + E2E. Size: M.
    Blocked by steps 8, 9, 13.

## Phase 4 - Polish

17. **Consent + a11y + perf**
    Files: `src/components/ConsentBanner.vue`, opt-in "deschis la conversatie" toggle, `docs/QUALITY.md` budget enforcement.
    Risk: Med. LEGAL-REVIEW (cookie consent, non-essential scripts before consent, chat opt-in). Size: M.
    Start the legal review in parallel with step 11, not here.

18. **Weekly recap** (PLAN Phase 2), then the English layer.
    Files: `api/digest.js`, `src/views/RecapView.vue`. Risk: Low. Size: M.

---

## OUT OF SCOPE

- Native mobile apps
- Full social network: follower feeds, DMs, anything without opt-in
- Ad platform beyond named slots with a kill switch
- PostGIS / heavy spatial queries (basic point + bounding box only)
- Scraping beyond public listing pages and schema.org metadata; no verbatim markup or content copying
- Notifications beyond the daily/weekly digest
- Languages beyond ro-RO (en later)

## Top risks + mitigations

1. **No tests until step 2** - regressions compound across 18 steps. Mitigation: quality gates are step 2, before any feature work.
2. **LEGAL-REVIEW cluster** (map tiles, check-in GDPR, chat opt-in, cookie consent, scraping) blocks launch, not just individual features. Mitigation: start the review at step 11, track in `docs/OPEN_ISSUES.md`.
3. **Feed sources UNVERIFIED** - `api/events-feed.js` was verified only against a stubbed fetch, never against the live network. Mitigation: UI degrades to demo data, shows `lastUpdated`, and never blocks on the network.
4. **Scope creep to social features** - the product is link-first, not follower-first. Mitigation: hold the OUT OF SCOPE list.
5. **Graph over-engineering** - explicit `Connection` records plus small traversal helpers; no graph DB. Mitigation: `lib/graph.ts` stays a pure function module.

## Verification before this roadmap is trustworthy

- `docs/research/competitors.md` comparison matrix is entirely `TBD`, so the UX/Mobile benchmark column is unset. The ordering above is driven by `AGENTS.md` + `docs/PLAN.md` scope only, not by verified competitor gaps.
- `docs/OPEN_ISSUES.md` UNVERIFIED items (GTFS, tile terms, OSM sector boundaries, scraping approach) are all consumed by Phase 3.

</content>
