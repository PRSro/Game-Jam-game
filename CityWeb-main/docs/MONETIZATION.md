# Monetization — Piața

> **Core principle:** residents use it free. Money comes from the people who
> want to reach those residents — organizers, venues, local businesses,
> sponsors. The audience is the product, so anything that degrades the map,
> the closures, or the live feed for residents also hurts the business.

---

## Revenue streams (priority order)

| # | Stream | Who pays | How it works | Fit |
|---|--------|----------|--------------|-----|
| 1 | **Promoted events** | Event organizers | Featured placement on map, calendar and "Tonight near you" lists. Labeled **Promovat**. Capped per area so it never floods the feed. Self-serve checkout. | Best first revenue |
| 2 | **Local business listings & offers** | Cafés, venues, shops | Claimed profile with opening hours, photos and time-limited deals for slow hours near the user ("Slow-Hour Fill-In"). Recurring monthly fee. | Strong, recurring |
| 3 | **Sponsored radio & bulletin segments** | Brands | Short sponsor credits read in the spoken bulletin — e.g. *"Prognoza vremii oferită de [Brand]"*. Sold per slot or per week. | Easy to sell, low clutter |
| 4 | **Ticket affiliate commissions** | Ticketing platforms | Deep links to Romanian ticketing sites that run affiliate programs. Commission on completed sales. | Low effort; income uncertain — validate first |
| 5 | **Organizer tools subscription** | Recurring organizers | Analytics dashboard, attendee push reminders, recurring-event templates, calendar embeds, team accounts. | Good once organizer base exists |
| 6 | **Job postings** | Employers | Pay-per-featured listing, ideally tied to a linked event. | Works, but a crowded market |
| 7 | **Aggregated insights** | City hall, researchers, brands | Anonymous demand and poll data ("what Sector 3 residents want more of"). Minimum group sizes applied; never individual-level. | Later stage only |
| 8 | **B2B / B2G white label** | Universities, companies, sector primării | Same platform deployed for a campus, district or institution. | Long sales cycles; late stage |

> **No paid consumer tier at launch.** Willingness to pay for city apps is low, and gating features shrinks the very audience we sell.

---

## Rules that protect the product

1. **Never paywall public-interest information.** Traffic closures, transit disruptions and outages are always free and always surfaced first.

2. **Label every sponsored placement clearly.** Use the design system's reserved `Promovat` / `Material sponsorizat` style. Consumer-protection rules on disguised advertising apply — have a lawyer review the exact wording before launch.

3. **Contextual before behavioral targeting.** Target by neighborhood, time of day and stated interest. This is cheaper to comply with under GDPR and does not require a consent wall.

4. **Never sell personal data.** Attendance history, check-in data and "who was here" are never shared with third parties. Only aggregated, anonymized figures with minimum group sizes leave the platform.

5. **Limit ad density.** One promoted item per screen maximum. Promoted items never appear inside closure alerts or disruption banners.

6. **Honest pay-to-rank.** Promoted placements are visually distinct and cannot pretend to be organic results.

---

## Phasing

### Phase 0 — Launch (0 → ~1,000 active users)
- No revenue. Focus entirely on getting organizers to post events and residents to check in.
- Instrument key events (event views, RSVPs, check-ins, poll votes) so you can show reach numbers to future advertisers.

### Phase 1 — Early revenue
- Promoted events: sell the first 5–10 slots by hand to known organizers and venues before building self-serve checkout.
- One sponsored bulletin slot per week, sold directly.

### Phase 2 — Growth
- Self-serve promoted-event checkout.
- Business profiles and Slow-Hour offers.
- Organizer subscription tier.
- Ticket affiliate links live.

### Phase 3 — Scale
- Aggregated insights product (with legal/privacy review).
- B2B / B2G white-label conversations.

---

## Numbers to validate — not to trust yet

> UNVERIFIED: No Romanian local-ad rates or willingness-to-pay research has been done. Any prices listed here would be guesses.

**Before setting any prices, run two interview rounds:**

1. **10 event organizers** — What do you spend today on Facebook ads, posters and ticketing fees per event?
2. **10 local venues / cafés** — What do you pay monthly for visibility (Instagram boosts, flyer printing, Google ads)?

**The comparison that sells the product:**
> *"Promoting an event on Piața costs less than the same reach on Facebook, and it reaches people who live nearby and are free tonight."*

That framing — hyper-local intent vs. broad social reach — is the core commercial pitch and should be validated with real spend data before any pricing is fixed.

---

## Related documents

- [`docs/PLAN.md`](./PLAN.md) — product statement, MVP scope, phasing
- [`docs/DESIGN_SYSTEM.md`](./DESIGN_SYSTEM.md) — `Promovat` label styles
- [`docs/BUSINESSES.md`](./BUSINESSES.md) — business listing schema
- [`docs/research/competitors.md`](./research/competitors.md) — competitive landscape
