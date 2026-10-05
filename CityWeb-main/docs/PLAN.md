# PLAN - Piața (Bucharest living map)

Status: DRAFT - For approval only (no code yet)
Date: 2026-10-04

## Product statement
"Piața" is a living map of Bucharest connecting events, businesses, jobs, traffic, polls and people through Where, When, Who. Core question: "What's happening in my Bucharest, and where do I fit in it?"

## Personas (3)
1. Newcomer/student (international/RO) - needs orientation, events, people, 30-day checklist
2. Commuter - needs route-aware disruptions, tonight near me, quick votes
3. Local business/organizer/job seeker - needs visibility, RSVPs, community reach

## Scope

### MVP (must ship)
- Onboarding (4 taps): neighborhood/sector, commute (metro/route), 3-5 interests, intent
- "Acum în București" live feed (personalized by route/neighborhood/interests)
- Places (venues/streets/areas) + map (lightweight)
- Events (map+list+calendar), RSVP, QR check-in, "who from your area"
- Businesses directory (minimal)
- Job ads (commute-aware, links to people met)
- Traffic disruptions (linked to events/commute/polls)
- Polls (place+community scoped, live)
- City passport (stamps for visited neighborhoods/venues)
- Neighborhood identity (tags, stats, N-vs-N)
- 30-day newcomer checklist
- CMS (Payload) + Postgres schema with graph edges (Connection)
- Seed data in /seed (original placeholders)

### Phase 2 (should)
- Reconnects ("you were at X"), conversation prompts for check-in opt-in
- Salary poll references for jobs
- Weekly recap
- English layer (alongside ro-RO)

### Later (could)
- Notifications beyond digest
- Advanced spatial queries
- Moderation tools expansion

## OUT OF SCOPE (explicit)
- Full social network (followers feed, DMs without opt-in) - opt-in only for "open to chat" at events
- Scraping full competitor content (only structure/fields for inspiration)
- Complex ad platform beyond named slots with kill switch
- Multi-language beyond RO (+EN later only)
- Native mobile apps in MVP

## Numbered tasks (MVP)
1. Schema design (Payload collections + Connection edges) - files: cms/collections/*.ts (or payload config), types. Risk: Med. Test: typecheck. Size: M.
2. Places + map integration (lightweight) - components/map, lib/geo. Risk: Low. Test: unit + render. Size: S.
3. Events + RSVP + check-in (QR) - collections/events, pages/events, check-in. Risk: Med. Test: E2E. Size: M.
4. Businesses/Jobs/Traffic/Polls - collections + pages. Risk: Low. Test: unit. Size: M.
5. Graph linking (traversal helpers) - lib/graph, edges. Risk: Med. Test: unit. Size: M.
6. Onboarding + personalization - app/onboarding, lib/prefs. Risk: Low. Test: E2E. Size: S.
7. "Acum în București" feed - app/feed, ranking by relevance. Risk: Med. Test: E2E. Size: M.
8. City passport + neighborhood identity + checklist - passport, neighborhoods. Risk: Low. Test: unit/E2E. Size: S.
9. Seed data (/seed) original placeholders - seed/*.json. Risk: Low. Test: import check. Size: S.
10. CI checks + budgets - .github/workflows, docs/QUALITY.md. Risk: Low. Test: build/lint/typecheck. Size: S.

Order: schema first, then core entities+links, then personalization/feed.

## Top 5 risks + mitigations
1. Over-engineering graph early (Med) - Start with explicit Connection collection + minimal traversal; avoid premature graph DB.
2. Map performance/spatial (Low-Med) - Use basic Postgres point + simple map; defer heavy geo until needed.
3. Privacy (GDPR/ePrivacy) (Med) - Minimal PII, opt-in only, no non-essential scripts before consent, document LEGAL-REVIEW.
4. Content originality (High) - Seed only from original placeholders; reject scraped full text.
5. Scope creep to social features (Med) - Strict OUT OF SCOPE; link-first not follower-first.

## Approval required
STOP and wait for approval before Phase 2 (implementation). No app code yet.
