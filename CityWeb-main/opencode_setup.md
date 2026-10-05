# OpenCode Setup & AI Agent Master System Prompt

`opencode.jsonc` and `AGENTS.md` are untracked in git for this repo. This document serves as the live config specification, project architecture guide, and **Master System Prompt** for AI coding agents building **CityWeb ("Piața")**.

---

## 1. Product Concept & Master AI Directive

### 1.1 Product Vision: "Piața"
AI agents working on **CityWeb** MUST align all features, components, and data structures with the product vision:
> **"Piața"** — the city square where people once met, traded, argued, and found work (inspired by Bucharest's *Micul Paris* legacy).
> **Core Question Answered**: *"What's happening in my Bucharest, and where do I fit in it?"*

### 1.2 The 3-Pillar Interlinked Data Model (`Where`, `When`, `Who`)
Nothing in the codebase is a standalone listing. Every component or data model must connect cross-domain nodes:
- **Traffic Closure**: Linked to affected events, broken transit routes, and local civic polls (*e.g., "Should Calea Victiorei be pedestrian on Sundays?"*).
- **Event**: Linked to neighborhood attendees, job openings from companies present in the room, and live polls.
- **Job Opening**: Linked to home-to-work metro commute time, mutual connections who work there, and verified salary polls.
- **Poll**: Rooted in place & community (*e.g., "Sector 3 Residents"*); feeds featured events.
- **Profile / Person**: Historical log of verified event appearances & contributions, NOT a static CV.

### 1.3 Core 5-Tab Navigation Structure
AI agents building views MUST map all pages into these 5 core tabs + 1 central action modal:
1. 📻 **Acum**: Live personalized feed (closures, events tonight, sector polls, nearby jobs).
2. 🗺️ **Harta**: Interactive filterable city map.
3. 👥 **Oameni**: Neighborhood hubs & local community members.
4. 📅 **Calendar**: Event schedule & attendance tracker.
5. 👤 **Eu**: City Passport, visit stamps, and history.
6. ➕ **Central Plus Button**: Modal launcher to post an Event, Job, or Poll.

---

## 2. Technical Implementation & Architectural Standards

### 2.1 Dual-Framework Component Pattern (Vue 3 + React 19 via Veaury)
- **Primary View Engine**: Vue 3 using `<script setup>` Single File Components (SFCs).
- **React Component Integration**: React components (from **Watermelon UI**, **Shadcn UI**, or custom TSX) MUST be placed inside `src/components/ui/*.tsx`.
- **Bridge Export Standard**: React components MUST NOT be imported directly into `.vue` files. They MUST be wrapped using `applyReactInVue` inside `src/components/watermelon-ui.ts` (or dedicated wrapper modules):

```tsx
// src/components/watermelon-ui.ts
import { applyReactInVue } from 'veaury';
import CardSplitAccordionReact from './ui/card-split-accordian';
import ButtonReact from './ui/button';

// Export Vue-compatible components
export const CardSplitAccordion = applyReactInVue(CardSplitAccordionReact);
export const Button = applyReactInVue(ButtonReact);
```

### 2.2 Design System, Custom Fonts & Theme Tokens
- **Font Integration**: Uses custom `@font-face` definitions configured in `src/styles/globals.css`:
  - Display Font: `var(--font-charlie-display)` (`'Charlie Display'`) for `h1`-`h6`.
  - Text Font: `var(--font-charlie-text)` (`'Charlie Text'`) for `body`.
- **Tailwind v4 Engine**: Configured via `@tailwindcss/vite` and `src/styles/theme.css`.
- **Custom Theme Variables**:
  - Colors: `atlassian-blue` (`#1868db`), `midnight-navy` (`#101214`), `carbon-edge` (`#292a2e`), `slate-current` (`#1c2b42`), `taxicab-yellow` (`#fca700`), `lavender-wash` (`#eed7fc`), `confetti-gradient` (`#bf63f3`).
  - Spacing & Radius: `--radius-sm` (2px), `--radius-md` (5px), `--radius-xl` (15px), `--radius-2xl` (20px), `--radius-3xl` (24px), `--radius-full` (10000px).
- **Class Composition**: Use `cva`, `clsx`, and `tailwind-merge` (`cn` helper in `src/lib/utils.ts`).

---

## 3. Config File (`opencode.jsonc`)

Save the block below as `opencode.jsonc` in your project root.

```jsonc
{
  "$schema": "https://opencode.ai/config.json",

  "autoupdate": "notify",
  "share": "disabled",

  // ---------- Plugins ----------
  "plugin": [
    "@tarquinen/opencode-dcp",
    "opencode-mem",
    "cc-safety-net",
    "opencode-snip@1.6.1",
    "opencode-caveman",
    "oh-my-opencode-slim",
    "opencode-usage-plugin@0.0.1",
  ],

  // ---------- Instructions ----------
  "instructions": ["AGENTS.md", ".opencode/rules/*.md"],

  // ---------- Context Control ----------
  "compaction": {
    "auto": true,
    "prune": true,
  },

  "watcher": {
    "ignore": [
      "node_modules/**",
      "dist/**",
      "build/**",
      ".next/**",
      ".git/**",
      "coverage/**",
      "*.lock",
      "*.min.js",
      "*.map",
    ],
  },

  // ---------- MCP Servers ----------
  "mcp": {
    "context7": {
      "type": "remote",
      "url": "https://mcp.context7.com/mcp",
      "enabled": true,
    },
  },

  // ---------- Permissions ----------
  "permission": {
    "edit": "ask",
    "webfetch": "ask",
    "bash": {
      "*": "ask",
      "git status*": "allow",
      "git diff*": "allow",
      "git log*": "allow",
      "ls*": "allow",
      "cat *": "allow",
      "grep *": "allow",
      "rg *": "allow",
      "npm test*": "allow",
      "npm run lint*": "allow",
      "git push*": "deny",
      "rm -rf*": "deny",
    },
  },

  // ---------- Formatters ----------
  "formatter": {
    "prettier": {
      "command": ["npx", "prettier", "--write", "$FILE"],
      "extensions": [".js", ".jsx", ".ts", ".tsx", ".json", ".css", ".md"],
    },
  },
}
```

---

## 4. Master `AGENTS.md` File

Save the block below as `AGENTS.md` in the project root.

```md
# AI Agent Execution Rules & Project Directives

## 1. Product Identity ("Piața")
Building Bucharest's digital city square ("Piața").
Core Model: Connect everything via Where, When, Who.
Tabs: Acum (Feed), Harta (Map), Oameni (People), Calendar (Events), Eu (Passport).

## 2. Workflow Protocols
- **Planning Phase**: Output a numbered plan (< 400 words) with target files, risks, and OUT OF SCOPE items.
- **Execution Phase**: Implement only approved plan with minimal diffs.
- **Search & Inspection**: Check exact line numbers before editing.

## 3. Strict Code Implementation Rules

### RULE 1: React Components in Vue
NEVER import `.tsx` or React components directly into `.vue` template files.
ALWAYS wrap them with `applyReactInVue` inside `src/components/watermelon-ui.ts` and import the wrapped component into Vue.

### RULE 2: Design Token & Typography Usage
ALWAYS use custom theme variables and fonts in `src/styles/globals.css` and `src/styles/theme.css`:
- Fonts: `var(--font-charlie-display)` for titles, `var(--font-charlie-text)` for body text.
- Colors: `atlassian-blue`, `midnight-navy`, `carbon-edge`, `slate-current`, `taxicab-yellow`, `lavender-wash`, `confetti-gradient`.
- Utilities: Apply classes via `cn(...)` utility helper (`src/lib/utils.ts`).

### RULE 3: Verification & Deployment
Verify build locally (`npm run build` or `npm run dev`) and deploy updates via `vercel --prod`.
```

---

## 5. First-run Steps

1. Run `opencode` in your project root.
2. Run `/connect` and authenticate your AI provider.
3. Run `/models` and select your target model.
4. Use `Tab` to switch between `plan` mode (read-only) and `build` mode.
