# B365 Events API Conceptualization

This document is a planning-only reference (no code implemented yet). It outlines how to extract/ingest events from b365.ro for inspiration/seed data only - never copy full texts, images, logos. All ingested items must be rewritten as original placeholders or used only for structure/fields.

## Goal

Conceptualize an approach to fetch event data from b365.ro to understand structure/fields for seeding "Piața" (living map). Respect ToS, robots.txt, rate limits. Treat scraped data as UNVERIFIED.

## Ethics & constraints

- No wholesale copying of descriptions/images/logos; transform to original content.
- Check robots.txt and terms. Prefer official APIs if any exist; otherwise polite scraping (rate-limited, cached).
- Only extract fields needed for schema mapping.
- Mark all imported items as placeholder/demo.
- Flag LEGAL-REVIEW if unclear.

## Target pages (to inspect)

- Homepage/events section
- Events listing/category pages
- Individual event detail pages
- Archive/past events if useful

## Likely fields to extract

- Title
- Date/time (start/end), timezone (Bucharest)
- Location/venue/address, neighborhood/sector (if present)
- Category/tags
- Description (summary only, rewrite)
- Price (free/paid)
- Organizers
- Image URLs (reference only, don't hotlink copyrighted images; replace with placeholders)
- Links/original URL (for attribution of source only, internal seed won't expose it)

## Technical approaches

1. Static HTML parsing (cheerio/BeautifulSoup) - check if server-rendered.
2. Headless browser (Playwright/Puppeteer) if JS-heavy.
3. RSS/sitemap - check b365.ro for feeds/sitemaps.
4. API calls - look for JSON embedded in pages (window.**NEXT_DATA**, JSON-LD, etc.)

## Implementation notes (conceptual)

- Respect delay between requests (>=1-2s)
- User-agent identifiable, cache results
- Store raw as UNVERIFIED in separate temp dir, never commit raw full content.
- Transform pipeline: normalize fields -> map to Piața schema (Place/Event) -> rewrite to original -> validate.
- Rate-limit + backoff.

## Mapping to Piața schema

- Event: where (Place), when (start/end), who (organizers), links to neighborhood/sector
- Place: venue name, address -> geocode later (UNVERIFIED coordinates)
- Neighborhood/Sector: infer if missing, mark UNVERIFIED

## Next steps (no code yet)

- Inspect robots.txt, sitemap, HTML structure via fetch (polite).
- Draft minimal extractor spec.
- Get approval via PLAN.md before implementing.
- Add to docs/OPEN_ISSUES.md as UNVERIFIED/LEGAL-REVIEW.

All items marked UNVERIFIED. No scraping of full articles.
