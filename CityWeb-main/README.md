# CityWeb 🏙️ — "Piața" (Deployed on Vercel)

A high-performance hybrid web application built with **Vue 3**, **Vite**, **React 19**, and **Tailwind CSS v4**, deployed live on **[Vercel](https://vercel.com)**. Powered by a **Neon PostgreSQL** database cloud backend.

---

## 🏛️ Product Vision & Concept: "Piața"

> **Working Concept**: **"Piața"** — the city square where people once met, traded, argued, and found work. Inspired by the *Micul Paris* brand (Paris had its cafés, Bucharest had its *piețe*). Alternate names to test: *Cafeneaua*, *Cartier*, *Pe Bulevard*.

### Core Question
> **"What's happening in my Bucharest, and where do I fit in it?"**

### The 3-Pillar Data Model: `Where`, `When`, `Who`
Nothing exists as a standalone listing. Every node in the application is cross-linked across the city graph:
- **Traffic Closure**: Links to affected events, broken commute routes, and a local civic poll (*e.g., "Should Calea Victoriei be pedestrian on Sundays?"*).
- **Event**: Displays who is attending from your neighborhood, job openings from companies present in the room, and a live event poll.
- **Job Opening**: Shows real transit time from your home via metro, people you've met who work there, and verified salary polls for that role.
- **Poll**: Rooted in a specific place & community (*e.g., "Sector 3 Residents"*); results directly feed featured events & city topics.
- **Person / Profile**: A historical log of where they showed up and participated, not a static CV.

---

## 🛠️ Database Infrastructure: Neon PostgreSQL

- **Provider**: **[Neon Database](https://neon.tech)** (Serverless PostgreSQL with connection pooling)
- **Host**: `ep-morning-sound-zakjxw3c-pooler.c-2.eu-west-2.aws.neon.tech`
- **ORM / CMS Bridge**: Payload CMS + `@payloadcms/db-postgres`
- **Configured Environment File**: `c:\HACKATON\CityWeb\.env`

---

## 🌐 Live Production Deployment

- **Live Site**: [https://city-web-navy.vercel.app](https://city-web-navy.vercel.app)
- **Deployment Platform**: Vercel (Edge Network)
- **Routing Configuration**: `vercel.json` SPA rewrite engine

---

## 🛠️ Complete Tech Stack & Library Breakdown

### Deployment, Database & Core Frameworks
- **[Neon PostgreSQL](https://neon.tech/)** — Serverless PostgreSQL database engine with connection pooling.
- **[Vercel Platform](https://vercel.com/)** — Production hosting & SPA rewrite engine.
- **[Vue 3](https://vuejs.org/)** (`^3.5.42`) — Reactive UI layout and navigation engine.
- **[React 19 & React DOM](https://react.dev/)** (`^19.3.0`) — React engine powering Watermelon UI components.
- **[Veaury](https://github.com/kalacloud-inc/veaury)** (`^2.6.3`) — Dual-framework bridge using `applyReactInVue`.

### Styling, Fonts & Themes
- **Typography**: Custom `@font-face` definitions for **Charlie Display** (Headings) and **Charlie Text** (Body text).
- **Tailwind CSS v4**: Utility-first CSS engine configured via `@tailwindcss/vite` and `src/styles/theme.css`.
- **Theme Variables**: Custom tokens for Atlassian Blue (`#1868db`), Midnight Navy (`#101214`), Taxicab Yellow (`#fca700`), Lavender Wash (`#eed7fc`), Confetti Gradient (`#bf63f3`), etc.
- **Design Systems**: **Shadcn UI** (`base-nova`), **Radix Vue**, `cva`, `clsx`, `tailwind-merge`.

---

## 🚀 Getting Started & Deployment

### Run Locally
```bash
npm install
npm run dev
```

### Deploy Environment Secret to Vercel
```bash
vercel env add DATABASE_URL
```

### Deploy to Vercel Production
```bash
vercel --prod
```
