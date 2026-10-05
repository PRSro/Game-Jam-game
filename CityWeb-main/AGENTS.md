## CityWeb ("Piața") - Bucharest living map

Lead engineer/product analyst for "Piața" (the city square). A living map of Bucharest where everything is connected through Where, When, Who. The core question: "What's happening in my Bucharest, and where do I fit in it?"

Philosophy: nothing is a standalone listing. Traffic closure affects events and commute; events connect people from neighborhoods and jobs; jobs connect to commute/people; polls live in places/communities and feed results back into what gets featured. People are defined by where they showed up, not just profiles.

## Workflow

- Planning: do not edit files. Output a numbered plan with files to touch, risks, tests to run, and an explicit OUT OF SCOPE list. Keep under 400 words.
- Building: implement only the approved plan, with smallest possible diff. If plan wrong, stop and report instead of improvising.
- Token discipline: search with fewest file reads; never paste whole files in replies; delegate search/doc lookup to cheap/explore agents.
- One clarifying question max, only if truly blocked. Otherwise state assumption in one line and continue.
- Commit small and often on feature branch. Pull before starting and before pushing.
- Never invent facts/statistics/quotes/sources. Mark unverified as UNVERIFIED.
- No competitor content copied (text/images/logos/layouts pixel-for-pixel). Seed content must be original, marked as placeholder.
- Copyright/ethics: analyze structure/features/tech approach only. Flag anything needing legal review as LEGAL-REVIEW.

## Research and planning gates

- Phase 0 (no code): competitive research to `docs/research/competitors.md` (matrix + table stakes + max 7 gaps ranked by value/effort + sources with dates). Cap ~1,500 words total. Focus: city maps, hyperlocal feeds, neighborhood platforms, event check-ins, poll/community tools.
- Phase 1 (no code): `docs/PLAN.md` with product statement ("Piața" living map, core question), 3 personas (e.g. newcomer/student, commuter, local business/organizer), MVP/Phase 2/Later, explicit OUT OF SCOPE, numbered tasks (files, risk L/M/H, test, S/M/L), top 5 risks+mitigations. STOP and wait for approval before code.
- First action: confirm stack, start Phase 0 via cheap/explore agents, deliver competitors.md then PLAN.md, then STOP before writing app code.

## Core model (graph-first)

Everything hangs off Where (place/venue/neighborhood/sector/route), When (time window/recurrence), Who (person). All entities must be linkable to each other.

## Data model (minimum)

- Place: venue/street/area, lat/lng, neighborhood/sector, tags. Anchor for everything.
- Event: where+when, organizers, neighborhood tags, rsvp/check-in, connected jobs/polls/traffic, affected by disruptions.
- Person: minimal profile (neighborhood, commute route/metro, 3-5 interests, intent: newcomer/jobseeker/hiring/meeting/curious), attendance history (city passport), presence/opt-ins. Reputation from attendance/voting/helping, not followers.
- Business/Org: in places, hosts events, posts jobs, connected to community.
- JobAd: where (place/company), when valid, commute links, people met there, salary poll references.
- TrafficDisruption: route/metro/street, where+when, affects events/commute, linked polls.
- Poll: lives in place+community (neighborhood/sector/city), time window, results feed "Acum în București"/featured.
- CheckIn: event/place QR, time, opt-in "open to chat", conversation prompts, live poll/Qs.
- Neighborhood: identity (Floreasca/Drumul Taberei etc.), stats, neighborhood-vs-neighborhood poll results.
- Connection: lightweight edges (went to same event, from same neighborhood, affected by disruption, etc.). Enable traversal between all items.

## MVP features

- Onboarding (60s, 4 taps): neighborhood/sector, commute (metro/route), 3-5 interests, intent (newcomer/jobseeker/hiring/meeting/curious). Drives personalization.
- "Acum în București" live stream: mixed feed tuned to user (disruptions on route, events tonight, sector poll, jobs nearby). Feels like a local friend.
- Events: map+list+calendar, transit time from user, RSVP, QR check-in, "who's going from your area".
- Jobs: commute by metro, "you met 2 people there" when linked to attended events, verified salary poll placeholders.
- Traffic/Disruptions: linked to affected events/commutes + polls.
- Polls: place+community scoped, live during events, results affect recommendations.
- City passport: stamps for neighborhoods/venues visited. City passport + 30-day newcomer checklist.
- Neighborhood identity: tags, local stats, N-vs-N poll results.
- Daily/weekly moments: morning digest, midday vote, tonight near you, check-in, reconnects next day, weekly recap.

## Stack (working defaults)

- Frontend: Next.js (App Router) + TypeScript + Tailwind CSS. Server components; ISR for stable content, short revalidation for live stream/check-ins/polls.
- Data/Backend: Payload CMS + PostgreSQL (local Docker). Graph-like relationships (many-to-many, edges) with typed schema. Migrations checked in; .env.example only.
- Hosting: Vercel. Secrets never committed.
- Search/Discovery: Postgres full-text + unaccent; spatial queries for map (basic). Hosted engine only if needed.
- Maps: simple map view (start lightweight; choose provider only if necessary, document in PLAN).
- Images: srcset/AVIF/WebP, explicit width/height, CDN caching.
- Content: Romanian (ro-RO) default, natural tone; English layer optional later.

## Commands (repo state)

- Branch: `git checkout -b feature/<name> && git pull --rebase origin main`.
- Read-only git, ls/cat/grep/rg, npm test/lint allowed. git push/rm -rf denied. Edits require confirmation.
- Node: `npm i`, `npm run dev`, `npm test`, `npm run lint`, `npm run typecheck`, `npm run build`. Record in README.md when stable.
- Context7 MCP for library docs; prefer cheap/explore agents for search.

## Conventions

- Link-first design: every item must expose outgoing links to related Where/When/Who items.
- All UI must follow docs/DESIGN_SYSTEM.md. Never hardcode colors.
- Smallest possible diff; no unrelated refactors.
- Seed only in `/seed`, original placeholders marked demo data.
- Privacy: minimal onboarding, opt-in "open to chat", GDPR/ePrivacy-aware, no non-essential scripts before consent, labeled sponsored content, double opt-in newsletter.
- Security/reliability: validation, rate limiting, CSRF, secure headers, least-privilege roles, graceful degradation (show last-updated).
- Tests: unit for graph/linking logic + E2E for onboarding, live feed, map, RSVP/check-in, polls, neighborhood identity. Performance/accessibility budgets in PLAN/QUALITY.

## Repo hints

- `AGENTS.md` canonical; minimal high-signal.
- No app code until `docs/PLAN.md` approved. Never invent facts. Mark UNVERIFIED/LEGAL-REVIEW.
