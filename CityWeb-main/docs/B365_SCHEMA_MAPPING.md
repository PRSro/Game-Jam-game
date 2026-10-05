# B365 to Piața Schema Mapping

UNVERIFIED - based on conceptual analysis. Verify with actual data.

## Source fields (minimal only - store as-is, don't copy full text)
- title (string)
- startDate/time, endDate/time (Bucharest/EET/EEST)
- location/venue/address
- neighborhood/sector (infer if missing - mark UNVERIFIED)
- category/tags
- price (free/paid)
- organizer
- url (original, attribution only)
- image refs (do not store full images; use placeholders)

## Mapping to Piața collections

| B365 | Piața Collection | Notes |
|------|------------------|-------|
| Venue/location | Places | Create Place if not exists; geocode later (Nominatim, cache, 1 req/s). Mark coords UNVERIFIED. |
| Event title/desc | Events | Rewrite description to original placeholder; never store full scraped text. Link to Place. |
| Category | Events.category, tags | Normalize to controlled tags |
| Date/time | Events.startDate/endDate | Convert to ISO UTC; preserve local time context |
| Neighborhood | Places.neighborhood, Events.neighborhoodTags, Neighborhoods | Infer if missing; mark UNVERIFIED |
| Organizer | Businesses/Persons | Link via Connections if applicable |
| URL | metadata.sourceUrl | Store for attribution only, don't expose in public content if not needed |

## Ingestion rules
- Only extract minimal fields (title, time, place, url)
- Mark every imported item as unverified=true where applicable (TrafficDisruptions already have flag)
- Attribution: include source + lastChecked
- Never hotlink images; use local placeholders
- Respect robots.txt, rate limits (>=1-2s delays)
- Transform pipeline: raw (temp, never commit full) → normalize → map → rewrite to original → validate
- Per-source isolated failures
- GDPR-safe: no personal PII extraction beyond what's public and needed

## Connection edges
- Event → Place: type 'event_place'
- Event → Neighborhood: via tags + connections as needed
- Organizer (Business) → Place: 'business_place'
- Event → related Jobs if linked: explicit connections only
