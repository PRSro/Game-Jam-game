# Data Sources for Piața

Status: UNVERIFIED research notes - verify terms/URLs before use.

## 1. Transit (GTFS + RT)
- TPBI regional GTFS: https://gtfs.tpbi.ro/regional/
- Static GTFS (Bucharest): https://gtfs.tpbi.ro/regional/BUCHAREST-REGION.zip
- GTFS-RT + STB/Metrorex etc. per TPBI (verify current endpoints)
- NexTB app reference for live tracking; study API terms
- GTFS enables: transit time to event/job, "your line disrupted"

## 2. Traffic restrictions & disruptions
- Brigada Rutieră București: https://bpr.politiaromana.ro/ (weekend restrictions, extract street/sector/start/end/reason - LLM parsing)
- PMB (Primăria București): https://www.pmb.ro/
- Fallback news (Mediafax/Ziare) - link only, don't copy full text
- Known recurring: "Străzi Deschise" Calea Victoriei (Sundays/weekends seasonal; verify current dates). Affected: Bd. Dacia to Splaiul Independenței (09:00-23:00 approx)
- Output: TrafficAlert (linked to events/commute/polls)

## 3. Events
- Meetup: https://www.meetup.com/ (check API access; structured data present)
- Eventbrite: https://www.eventbrite.com/ (public API access may be restricted - verify)
- iaBilet.ro: https://www.iabilet.ro/ (major ticketing)
- lu.ma: https://lu.ma/ (Bucharest events)
- Roaba de Cultură: https://roabadecultura.ro/ (curated)
- Reference aggregator: https://github.com/andreiolariu/bucharest-meetup-tracker (daily list; check terms)
- Direct submissions (Piața) - primary long-term

Rules: store title/time/place/URL only; rewrite descriptions; no hotlinking copyrighted images.

## 4. Jobs
- ejobs.ro, BestJobs, Hipo.ro - prefer partner XML/affiliate feeds; third-party scrapers (Apify) may violate ToS; republishing risky
- OLX Jobs - verify
- Direct employer posting (preferred for "met people there" linking)
- Seed demo jobs only, clearly labeled

## 5. Context (maps, geo, weather, AQ, stats)
- OpenStreetMap: https://www.openstreetmap.org/, Overpass API: https://overpass-api.de/
- Bucharest OSM references: https://github.com/paulciorbaru/bucharest-bus-map (Overpass usage)
- Map tiles: https://www.openfreemap.org/ or OSM-based (credit OSM)
- Geocoding: Nominatim https://nominatim.org/ (1 req/s, cache)
- Weather: https://open-meteo.com/ (free, no key typical; check terms)
- Air quality: https://calitateaer.ro/ (verify access)
- Open data: https://data.gov.ro/, INS: https://insse.ro/

## Ingestion principles
- Respect robots.txt + ToS for each
- Rate-limit + cache
- Transform to original content; never store full scraped text/images
- Attribution + source + last_checked
- Mark UNVERIFIED; flag LEGAL-REVIEW if unclear
- Per-source script writing to same schema; failures isolated
- GDPR: personal data (attendance/check-in opt-in only)
