# Documentație: Afaceri Locale (Local Businesses Schema)

## Core Philosophy
In Piața ("Acum în București" living map), local businesses are not standalone listings. They are graph nodes connected through **Where** (Neighborhood, Place, Address), **When** (Operating Hours, Hosted Events), and **Who** (Community Members, Job Openings, Salary Polls).

## Entity Data Schema

```typescript
export interface Business {
  id: string
  name: string
  localizedName?: string
  neighborhood: string // Relation: Neighborhood.id
  category: 'cafe' | 'bookstore' | 'bakery' | 'tech-hub' | 'bistro' | 'services'
  address: string
  localizedAddress?: string
  description: string
  localizedDescription?: string
  tags: string[]
  verified: boolean
  // Contact & Social Links (UNVERIFIED demo metadata)
  facebookUrl?: string
  websiteUrl?: string
  phone?: string
  // Graph Relations (Outgoing Links)
  connectedEvents: string[] // Event.id[]
  connectedJobs: string[]   // JobAd.id[]
  connectedPolls: string[]  // Poll.id[]
}
```

## Graph Connections
- **Business ➔ Place / Neighborhood**: Anchored in Bucharest neighborhoods (e.g. Floreasca, Centru, Drumul Taberei, Romexpo).
- **Business ➔ Events**: Hosts community meetups, concerts, or workshops.
- **Business ➔ Jobs**: Posts local job openings reachable via Metro commute.
- **Business ➔ Polls**: Participates in neighborhood polls & local preference surveys.
