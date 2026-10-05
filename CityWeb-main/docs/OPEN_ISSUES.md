# Open Issues

## UNVERIFIED
- b365.ro event structure (fetch homepage + 2 event pages, record URLs + dates)
- TPBI GTFS/RT endpoints and terms (verify current access)
- Brigada Rutieră parsing approach (street/sector extraction accuracy)
- Meetup/Eventbrite API availability (restrictions may apply)
- lu.ma/Roaba de Cultură terms for aggregation
- Nominatim usage limits, tile provider terms, Open-Meteo/calitateaer access
- Sector/neighborhood boundaries from OSM (accuracy)

## LEGAL-REVIEW
- Web scraping vs partner feeds for events/jobs (Meetup/Eventbrite/ejobs/BestJobs/Hipo)
- Image hotlinking - must replace with placeholders
- Storing original URLs/attribution
- Check-in/attendance data (GDPR, opt-in, retention)
- Polls with demographic slices (neighborhood-vs-neighborhood)
- "Open to chat" opt-in, consent
- Cookie consent + non-essential scripts before consent
- Copyright for any imported references

## Technical
- Graph traversal performance at scale (Connection edges)
- Spatial queries (PostGIS) if needed later
- Live feed ranking algorithm
- QR check-in validation/rate limiting
- GTFS routing for transit time (lightweight vs full routing)

## Product
- Neighborhood naming consistency (Floreasca, Drumul Taberei etc.)
- Newcomer checklist localization
- Poll weighting (neighborhood vs city)

All items require verification before implementation. No code until PLAN.md approved.
