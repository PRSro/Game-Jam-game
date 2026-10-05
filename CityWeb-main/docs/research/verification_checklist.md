# Verification Checklist (Before Approval/Implementation)

Priority: resolve UNVERIFIED items before starting code.

## Transit
- [ ] Check TPBI terms: https://gtfs.tpbi.ro/regional/ - read usage terms
- [ ] Verify GTFS static download works, note last-modified
- [ ] Check GTFS-RT availability/endpoints; NexTB API terms
- [ ] Note rate limits, attribution

## Traffic
- [ ] bpr.politiaromana.ro - robots.txt + structure (street/sector/start/end/reason)
- [ ] PMB announcements format
- [ ] "Străzi Deschise" current season dates (verify 2026)

## Events
- [ ] Meetup - check API terms/current access (https://www.meetup.com/api/)
- [ ] Eventbrite - API restrictions (https://www.eventbrite.com/platform/api/)
- [ ] iaBilet.ro, lu.ma, Roaba de Cultură - ToS/robots.txt
- [ ] bucharest-meetup-tracker terms

## Jobs
- [ ] Contact/verify partner feeds for ejobs/BestJobs/Hipo (XML/affiliate) vs scraping
- [ ] OLX Jobs ToS

## Geo/Context
- [ ] Nominatim usage policy (https://operations.osmfoundation.org/policies/nominatim/)
- [ ] OpenFreeMap tiles ToS/attribution
- [ ] Open-Meteo terms (https://open-meteo.com/en/terms)
- [ ] calitateaer.ro access method + terms
- [ ] data.gov.ro/INS relevance
- [ ] OSM sector boundaries completeness for Bucharest

## Legal/GDPR
- [ ] Check-in opt-in, retention, "open to chat" consent
- [ ] Cookie consent requirements (ePrivacy/GDPR RO context)
- [ ] Attribution requirements for all sources

All items must be marked RESOLVED with evidence (URL, date, notes) before implementation.
