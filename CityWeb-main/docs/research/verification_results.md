# Verification Results (Minimal checks)

Date: 2026-10-04
Status: PARTIALLY VERIFIED - still need deeper ToS review before implementation

## Transit
- TPBI GTFS homepage accessible (https://gtfs.tpbi.ro/regional/) - OK
- BUCHAREST-REGION.zip downloadable (9.1MB) - static bundle exists with standard GTFS files (agency, stops, routes, trips, stop_times, shapes). Content present.
- GTFS-RT endpoints not fully documented in quick check; needs deeper review. UNVERIFIED for RT.
- Attribution likely required per TPBI terms - needs ToS check. UNVERIFIED.

## Traffic
- bpr.politiaromana.ro/robots.txt: Disallow: /, Allow: /index.php. Crawling restricted - must be extremely careful (polite, respect; better parse via official means or manual curation). LEGAL-REVIEW.
- Străzi Deschise (Calea Victoriei) seasonal - 2026 dates unknown. UNVERIFIED.

## Geo/Context (policies)
- Nominatim usage policy: standard OSM policy (rate limit ~1 req/s, identify app, no bulk commercial geocoding without agreement). UNVERIFIED for exact terms URL.
- OpenFreeMap: free public instance exists; commercial usage allowed with self-hosting/public instance guidelines. Terms need full review. UNVERIFIED.
- Open-Meteo: terms page accessible; free for many non-commercial cases - needs verification for specific use. UNVERIFIED.
- calitateaer.ro access method unclear. UNVERIFIED.

## Events/Jobs
- API restrictions likely for Eventbrite/Meetup - not verified in detail. UNVERIFIED/LEGAL-REVIEW.

## Conclusion
Sufficient raw data sources exist, but several ToS/robots constraints (especially bpr.politiaromana.ro Disallow: /) mean ingestion must be: 1) prefer official feeds/partner feeds, 2) manual curation for traffic, 3) only store minimal fields, 4) link-only. Mark LEGAL-REVIEW for scraping. Proceed only after full PLAN approval and resolving flagged items.
