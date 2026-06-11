# eStudyCard — Claude Code Project Guide

## Stack
Vue 3 + Vite · Tailwind CSS 3 · PrimeVue 4 · Pinia · Vue Router 4 · TipTap 2 · ApexCharts · @lucide/vue

## Run / Build
```bash
npm run dev      # http://localhost:5173
npm run build    # dist/
```
Login: any username/phone · use `admin` as username for the Admin Panel.

## API Keys (`.env`)
```
VITE_GEMINI_API_KEY=   # Google Gemini 2.5 Flash Lite — AI Tutor, grading, translation
VITE_TTS_API_KEY=      # Google Cloud TTS — Video Tutor audio
```

---

## Installed UI/UX Skills

All 14 skills are in `.claude/skills/`. Reference them when working on UI, design, accessibility, or component architecture.

### Design & Aesthetics
| Skill | Directory | When to Use |
|-------|-----------|-------------|
| **frontend-design** (Anthropic) | `.claude/skills/frontend-design/` | Creating any new UI screen — bold aesthetic choices, unique typography, memorable layouts |
| **canvas-design** (Anthropic) | `.claude/skills/canvas-design/` | Generating posters, banners, visual art, marketing assets as PNG/PDF |
| **brand-guidelines** (Anthropic) | `.claude/skills/brand-guidelines/` | Applying consistent Anthropic-style brand colors and fonts to artifacts |
| **theme-factory** (Anthropic) | `.claude/skills/theme-factory/` | Picking or generating a cohesive theme (Ocean Depths, Midnight Galaxy, etc.) for any artifact |
| **bencium-innovative-ux-designer** | `.claude/skills/bencium-innovative-ux-designer/` | Building distinctive, production-grade interfaces — avoid generic AI aesthetics, commit to bold creative direction |
| **bencium-controlled-ux-designer** | `.claude/skills/bencium-controlled-ux-designer/` | Ask-first collaborative UX design — never assume color/layout choices, always consult |

### Accessibility & Standards
| Skill | Directory | When to Use |
|-------|-----------|-------------|
| **web-design-guidelines** (Vercel) | `.claude/skills/web-design-guidelines/` | Auditing UI code against Web Interface Guidelines (trigger: "review my UI", "audit design") |
| **accesslint-audit** | `.claude/skills/accesslint-audit/` | Full WCAG 2.2 accessibility audit with fix mode — trigger: "audit my codebase", "fix accessibility" |
| **accesslint-scan** | `.claude/skills/accesslint-scan/` | Live-page accessibility scan — locate violations precisely without fixing |

### React/Vue Component Patterns
| Skill | Directory | When to Use |
|-------|-----------|-------------|
| **react-best-practices** (Vercel) | `.claude/skills/react-best-practices/` | Performance optimization: eliminate waterfalls, reduce bundle size, optimize re-renders (70 rules) |
| **composition-patterns** (Vercel) | `.claude/skills/composition-patterns/` | Compound components, render props, context providers — avoid boolean-prop proliferation |
| **react-view-transitions** (Vercel) | `.claude/skills/react-view-transitions/` | Page transitions, shared element morphs, route animations using `<ViewTransition>` |

### Testing & Tooling
| Skill | Directory | When to Use |
|-------|-----------|-------------|
| **webapp-testing** (Anthropic) | `.claude/skills/webapp-testing/` | Playwright-based UI testing — element discovery, automation, server lifecycle |

### AI-Powered Design Intelligence
| Skill | Directory | Description |
|-------|-----------|-------------|
| **ui-ux-pro-max** (NextLevelBuilder) | `.claude/skills/ui-ux-pro-max/` | 67 UI styles · 161 color palettes · 57 font pairings · 99 UX guidelines · 25 chart types · Vue/Nuxt stack data |

---

## Key Design Principles (from installed skills)

- **Typography**: Never use Inter/Roboto/Arial as primary. Pick unexpected, characterful fonts.
- **Color**: 4-5 neutral shades for structure + 1-3 bold accents. Never generic SaaS blue (#3B82F6).
- **Spacing**: 4px base unit (8/16/24/32/48px scale). Generous negative space.
- **Animation**: Only `transform` and `opacity`. Under 300ms for interactions. Respect `prefers-reduced-motion`.
- **Accessibility**: WCAG 2.1 AA minimum. 44×44px touch targets. Keyboard navigable. ARIA where needed.
- **Components**: Avoid boolean-prop proliferation — use composition patterns instead.

## Project Architecture
- `src/views/` — page-level components (auth, dashboard, preparation, ai-tutor, daily-tests, competition, coins, reports, complaints, admin)
- `src/components/layout/` — AppShell, AppSidebar, AppHeader, FloatingActions, NavItem
- `src/components/ui/` — KpiCard, ActionCard, BookOptionCard
- `src/components/test/` — ObjectiveQuestionCard
- `src/components/tutor/` — TutorCard
- `src/stores/` — auth.js, student.js, content.js
- `src/router/index.js` — all routes with auth guards
