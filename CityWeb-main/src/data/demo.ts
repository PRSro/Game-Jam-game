/**
 * Piața — Hackathon Demo Data
 * Static demo data, no backend wiring.
 * All content in ro-RO, marked as demo data.
 * DO NOT remove the DemoData marker — orchestrator uses it.
 */

export interface Neighborhood {
  id: string;
  name: string;
  localizedName: string;
  stats: {
    population: number;
    activeUsers: number;
    eventsLast30: number;
  };
  tags: string[];
  nvsN: {
    vs: string;
    results: number[]; // percentage for each neighborhood
  };
  places: string[]; // venue IDs
}

export interface Event {
  id: string;
  title: string;
  localizedTitle: string;
  neighborhood: string;
  when: string; // ISO-ish string for demo
  whenLocalized: string;
  description: string;
  localizedDescription: string;
  organizers: string[];
  rsvpCount: number;
  capacity?: number;
  tags: string[];
  image?: string; // placeholder art; real images come from the feed adapter
  // Relations — outgoing links
  connectedJobs: string[];
  connectedPolls: string[];
  connectedTraffic: string[];
  // Detail affordances
  qrCheckIn: boolean; // placeholder: visual QR only, no camera
  address?: string;
  localizedAddress?: string;
}

export interface JobAd {
  id: string;
  title: string;
  company: string;
  neighborhood: string;
  whenValid: string;
  commuteByMetro: string; // e.g. "15 min"
  peopleMet: number; // social proof: "ai întâlnit X persone aici"
  salaryPlaceholder: string; // "verificat prin sondaj"
  tags: string[];
}

export interface Poll {
  id: string;
  question: string;
  localizedQuestion: string;
  place: string; // neighborhood or "city"
  timeWindow: string;
  results: number[]; // percentage for each option
  optionLabels: string[];
  votes: number;
  isLive: boolean;
  // Relation links
  connectedEvents: string[];
  affectedNeighborhoods: string[];
}

export interface TrafficDisruption {
  id: string;
  type: "metro" | "route" | "street";
  title: string;
  localizedTitle: string;
  when: string;
  whenLocalized: string;
  affectedRoutes: string[];
  affectedEvents: string[];
  // Relation links
  connectedEvents: string[];
  affectedCommutes: string[];
}

export interface Business {
  id: string;
  name: string;
  localizedName?: string;
  neighborhood: string;
  category: "cafe" | "bookstore" | "bakery" | "tech-hub" | "bistro" | "services";
  address: string;
  localizedAddress?: string;
  description: string;
  localizedDescription?: string;
  tags: string[];
  verified: boolean;
  facebookUrl?: string;
  websiteUrl?: string;
  phone?: string;
  connectedEvents: string[];
  connectedJobs: string[];
  connectedPolls: string[];
}

export interface Persona {
  id: string;
  neighborhood: string;
  commute: string; // metro line or route
  interests: string[]; // 3-5
  intent: "newcomer" | "jobseeker" | "hiring" | "meeting" | "curious";
  cityPassport: string[]; // visited neighborhoods/venues
  reputation: number; // from attendance/voting/helping
}

// ============ Demo Data ============

export const neighborhoods: Neighborhood[] = [
  {
    id: "floreasca",
    name: "Floreasca",
    localizedName: "Floreasca",
    stats: { population: 42000, activeUsers: 2850, eventsLast30: 12 },
    tags: ["bucuresti", "central", "parc", "business"],
    nvsN: { vs: "Drumul Taberei", results: [45, 55] },
    places: ["parc-floreasca", "mall-floreasca", "univ-floreasca"],
  },
  {
    id: "drumul-taberei",
    name: "Drumul Taberei",
    localizedName: "Drumul Taberei",
    stats: { population: 38500, activeUsers: 2100, eventsLast30: 9 },
    tags: ["bucuresti", "residential", "parc", "student"],
    nvsN: { vs: "Floreasca", results: [55, 45] },
    places: ["parc-drtb", "mall-drtb", "chiesa-drtb"],
  },
  {
    id: "old-town",
    name: "Centru",
    localizedName: "Centru",
    stats: { population: 55000, activeUsers: 3200, eventsLast30: 15 },
    tags: ["bucuresti", "historic", "tourist", "nightlife"],
    nvsN: { vs: "Floreasca", results: [30, 70] },
    places: ["piata-unirii", "cartier-historic"],
  },
  {
    id: "romexpo",
    name: "Romexpo",
    localizedName: "Romexpo",
    stats: { population: 15000, activeUsers: 1900, eventsLast30: 8 },
    tags: ["bucuresti", "expo", "business"],
    nvsN: { vs: "Floreasca", results: [50, 50] },
    places: ["romexpo-hall"],
  },
];

export const events: Event[] = [
  {
    id: "event-1",
    title: "Concert de iarnă la Parc Floreasca",
    localizedTitle: "Winter Concert at Parc Floreasca",
    neighborhood: "floreasca",
    when: "2026-12-20T19:00:00Z",
    whenLocalized: "20 dec, 19:00",
    description: "Concierto al aire libre con artistas locali.",
    localizedDescription: "Open-air concert with local artists.",
    organizers: ["Colectiv Floreasca"],
    rsvpCount: 342,
    tags: ["muzică", "concert", "free", "noapte"],
    connectedJobs: [],
    connectedPolls: ["poll-1"],
    connectedTraffic: [],
    qrCheckIn: true,
  },
  {
    id: "event-2",
    title: "Job Fair la Romexpo",
    localizedTitle: "Job Fair at Romexpo",
    neighborhood: "romexpo",
    when: "2026-12-22T10:00:00Z",
    whenLocalized: "22 dec, 10:00",
    description: "Oportunități de carriere în toată România.",
    localizedDescription: "Career opportunities across Romania.",
    organizers: ["ANOFM"],
    rsvpCount: 1200,
    capacity: 2000,
    tags: ["job", "carieră", "recrutare"],
    connectedJobs: ["job-1", "job-2"],
    connectedPolls: [],
    connectedTraffic: [],
    qrCheckIn: true,
  },
  {
    id: "event-3",
    title: "Meetup Blocuri Verzi",
    localizedTitle: "Green Buildings Meetup",
    neighborhood: "drumul-taberei",
    when: "2026-12-18T18:30:00Z",
    whenLocalized: "18 dec, 18:30",
    description: "Discuții despre sustainability urban.",
    localizedDescription: "Urban sustainability discussions.",
    organizers: ["EcoDrumb"],
    rsvpCount: 89,
    tags: ["sustainability", "urbanism", "free"],
    connectedJobs: [],
    connectedPolls: ["poll-2"],
    connectedTraffic: [],
    qrCheckIn: true,
  },
];

export const jobs: JobAd[] = [
  {
    id: "job-1",
    title: "Product Designer",
    company: "TechCorp",
    neighborhood: "floreasca",
    whenValid: "15 gen 2027",
    commuteByMetro: "15 min",
    peopleMet: 2,
    salaryPlaceholder: "verificat prin sondaj",
    tags: ["design", "product", "remote"],
  },
  {
    id: "job-2",
    title: "Frontend Developer",
    company: "StartupDB",
    neighborhood: "old-town",
    whenValid: "1 feb 2027",
    commuteByMetro: "25 min",
    peopleMet: 3,
    salaryPlaceholder: "verificat prin sondaj",
    tags: ["dev", "react", "typescript"],
  },
  {
    id: "job-3",
    title: "Community Manager",
    company: "EcoDrumb",
    neighborhood: "drumul-taberei",
    whenValid: "20 dec 2026",
    commuteByMetro: "10 min",
    peopleMet: 1,
    salaryPlaceholder: "verificat prin sondaj",
    tags: ["community", "outdoor", "NGO"],
  },
];

export const polls: Poll[] = [
  {
    id: "poll-1",
    question:
      "Care este evenimentul pe care îl ți-ai mai desidro să participate?",
    localizedQuestion: "What event would you most like to attend?",
    place: "city",
    timeWindow: "dec 2026",
    results: [35, 25, 20, 20],
    optionLabels: ["Concert", "Job Fair", "Meetup", "Expo"],
    votes: 1243,
    isLive: true,
    connectedEvents: ["event-1", "event-2", "event-3"],
    affectedNeighborhoods: ["floreasca", "old-town", "drumul-taberei"],
  },
  {
    id: "poll-2",
    question: "Cel mai importantă provocare urbanistică pentru București?",
    localizedQuestion: " biggest urban challenge for Bucharest?",
    place: "drumul-taberei",
    timeWindow: "dec 2026",
    results: [40, 30, 30],
    optionLabels: ["Transport", "Verde", "Habitare"],
    votes: 432,
    isLive: true,
    connectedEvents: ["event-3"],
    affectedNeighborhoods: ["drumul-taberei"],
  },
];

export const disruptions: TrafficDisruption[] = [
  {
    id: "disc-1",
    type: "metro",
    title: "Întârziere pe Linia M3",
    localizedTitle: "Delay on M3 Line",
    when: "2026-12-18T08:00:00Z",
    whenLocalized: "18 dec, 08:00",
    affectedRoutes: ["M3"],
    affectedEvents: ["event-3"],
    connectedEvents: ["event-3"],
    affectedCommutes: ["person-1"],
  },
];

export const persona: Persona = {
  id: "person-1",
  neighborhood: "floreasca",
  commute: "M3",
  interests: ["muzică", "tehnologie", "outdoor"],
  intent: "newcomer",
  cityPassport: ["floreasca", "old-town"],
  reputation: 7,
};

export const businesses: Business[] = [
  {
    id: "biz-1",
    name: "Cafenea de Specialitate Floreasca",
    localizedName: "Floreasca Specialty Coffee",
    neighborhood: "floreasca",
    category: "cafe",
    address: "Calea Floreasca nr. 42, București",
    description: "Prăjitorie meșteșugărească de cafea și spațiu de coworking comunitar.",
    localizedDescription: "Artisanal coffee roastery and community coworking space.",
    tags: ["cafe", "coworking", "specialty", "wifi"],
    verified: true,
    facebookUrl: "https://facebook.com/floreascacoffee.demo",
    websiteUrl: "https://floreascacoffee.ro.demo",
    phone: "+40 721 000 111",
    connectedEvents: ["event-1"],
    connectedJobs: ["job-1"],
    connectedPolls: ["poll-1"],
  },
  {
    id: "biz-2",
    name: "Librăria Independentă Centru",
    localizedName: "Old Town Independent Books",
    neighborhood: "old-town",
    category: "bookstore",
    address: "Str. Lipscani nr. 15, București",
    description: "Librărie de cartier cu club de carte și lansări de autori locali.",
    localizedDescription: "Neighborhood bookstore hosting book clubs and local author launches.",
    tags: ["cărți", "cultură", "evenimente", "centru"],
    verified: true,
    facebookUrl: "https://facebook.com/librariacentru.demo",
    websiteUrl: "https://librariacentru.ro.demo",
    phone: "+40 722 000 222",
    connectedEvents: [],
    connectedJobs: ["job-2"],
    connectedPolls: [],
  },
  {
    id: "biz-3",
    name: "Brutaria Meșteșugărească Drumul Taberei",
    localizedName: "Drumul Taberei Artisan Bakery",
    neighborhood: "drumul-taberei",
    category: "bakery",
    address: "Bulevardul Drumul Taberei nr. 88, București",
    description: "Pâine cu maia și patiserie artizanală dintr-un atelier de familie.",
    localizedDescription: "Sourdough bread and artisan pastries from a local family bakery.",
    tags: ["brutărie", "bio", "maia", "local"],
    verified: true,
    facebookUrl: "https://facebook.com/brutariadrumultaberei.demo",
    phone: "+40 723 000 333",
    connectedEvents: ["event-3"],
    connectedJobs: ["job-3"],
    connectedPolls: ["poll-2"],
  },
  {
    id: "biz-4",
    name: "Hub de Inovare Romexpo",
    localizedName: "Romexpo Innovation Hub",
    neighborhood: "romexpo",
    category: "tech-hub",
    address: "Bulevardul Mărăști nr. 65, București",
    description: "Spațiu pentru startup-uri, workshop-uri tehnice și hackathoane urbanistic.",
    localizedDescription: "Startup incubator, tech workshops, and urban hackathons.",
    tags: ["tech", "startup", "incubator", "networking"],
    verified: true,
    facebookUrl: "https://facebook.com/romexpohub.demo",
    websiteUrl: "https://romexpohub.ro.demo",
    phone: "+40 724 000 444",
    connectedEvents: ["event-2"],
    connectedJobs: [],
    connectedPolls: [],
  },
  {
    id: "biz-5",
    name: "Bistro Teatral Calea Victoriei",
    localizedName: "Calea Victoriei Theater Bistro",
    neighborhood: "old-town",
    category: "bistro",
    address: "Calea Victoriei nr. 120, București",
    description: "Bistro urban gastronomic și terasă cu muzică acustică live.",
    localizedDescription: "Urban gastronomic bistro and terrace with live acoustic music.",
    tags: ["bistro", "victoriei", "mâncare", "terasa"],
    verified: true,
    facebookUrl: "https://facebook.com/bistrovictoriei.demo",
    phone: "+40 725 000 555",
    connectedEvents: ["event-1"],
    connectedJobs: [],
    connectedPolls: [],
  },
  {
    id: "biz-6",
    name: "Ceainărie de Cartier Floreasca",
    localizedName: "Floreasca Neighborhood Teahouse",
    neighborhood: "floreasca",
    category: "cafe",
    address: "Str. Tudor Ștefan nr. 8, București",
    description: "Grădină liniștită de ceai, selecție de infuzii organice și jocuri de societate.",
    localizedDescription: "Quiet tea garden, organic infusion selection, and board games.",
    tags: ["ceainarie", "gradina", "relaxare", "floreasca"],
    verified: true,
    facebookUrl: "https://facebook.com/ceainariafloreasca.demo",
    connectedEvents: [],
    connectedJobs: [],
    connectedPolls: ["poll-1"],
  },
];
