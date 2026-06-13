# Claude Code Prompt — Landing 4: Critical Audit + Complete Professional Redesign

> Run this from the project root of the **Daikho Suno Parho** learning platform.
> Context: Landing 4 was reviewed by the client and **rejected** — feedback: "childish, looks like scrap, especially on mobile." This is the **last chance**. The page must be rebuilt from scratch to a professional, premium standard.

---

## PHASE 1 — Locate and Audit Landing 4 (be brutal)

1. Find the Landing 4 page in this project (search for files/routes named `landing4`, `landing-4`, `Landing4`, or the 4th landing variant). Open it and render/inspect it.
2. Before reading the skills, write an honest UI/UX audit of the CURRENT page as if you were a senior design reviewer who has to reject it. Do NOT defend it. Cover, at minimum:
   - **Visual maturity** — childish elements: cartoon clipart, rainbow/over-saturated colors, bouncy animations, emoji overuse, rounded blob shapes, comic-style fonts, gradient abuse.
   - **Typography** — font choices, inconsistent sizes/weights, poor hierarchy, low-contrast text.
   - **Layout & spacing** — misalignment, cramped or random spacing, no grid, inconsistent section rhythm.
   - **Mobile experience** — broken/overflowing sections, tiny tap targets, horizontal scroll, stacked chaos, unreadable text. (This is where the client said it looks like scrap — be specific.)
   - **Color & branding** — clashing palette, no defined design tokens, inconsistency with the rest of the app's modern dark-card theme.
   - **Content & messaging** — does it actually explain how students learn and study on this system? Or is it generic filler?
   - **Trust & professionalism** — would a school principal, board official, or parent take this product seriously after seeing this page?
3. Output this audit as a numbered list of concrete defects (file + section + problem). Then conclude: this page cannot be patched — it must be rebuilt.

## PHASE 2 — Read the skills (mandatory before any code)

- Read and follow the **`frontend-design`** skill (SKILL.md) fully before writing a single line of UI.
- Apply professional UI/UX principles throughout: clear hierarchy, generous whitespace, an 8px spacing system, a strict type scale, a restrained palette (1 primary + 1 accent + neutrals), real grid alignment, and purposeful (not decorative) motion.

## PHASE 3 — Rebuild Landing 4 from scratch (do NOT reuse the old markup or styles)

Delete/retire the old Landing 4 implementation. Build a brand-new page with this goal:

> **Show clearly and professionally how students learn and study on our system** — Watch · Listen · Read (Daikho Suno Parho), board-exam preparation, video lectures, self-tests, daily/monthly tests, progress tracking, and parent visibility.

### Design direction (non-negotiable)
- **Professional, premium, education-tech grade** — think the polish level of Coursera / Khan Academy / Notion landing pages, NOT a children's game site.
- One unified theme consistent with the app's modern dark-card design language (or a clean light theme with the same tokens — pick one and commit; no mixing).
- Real design tokens: CSS variables for colors, spacing, radii, shadows, type scale. No hard-coded random values.
- Typography: one professional font family (e.g., Inter/Plus Jakarta Sans for Latin; proper Urdu font like Noto Nastaliq Urdu for Urdu text). Strict scale (e.g., 14/16/18/24/32/48). No comic or display-gimmick fonts.
- Color: maximum 1 primary, 1 accent, plus neutrals. No rainbows, no neon overload, no more than one gradient and only if subtle.
- Imagery: clean device mockups, UI screenshots of the actual app, abstract geometric illustrations — NO cartoon clipart, NO stock-emoji style graphics.
- Motion: subtle fade/slide on scroll only. No bouncing, spinning, or confetti.
- Urdu/RTL: every section must render correctly in RTL with Urdu strings where the app is bilingual.

### Required page structure (in order)
1. **Sticky nav** — logo, 4–5 links (How it Works, Features, Tests, For Parents, Contact), one primary CTA ("Start Learning" / login).
2. **Hero** — one sharp headline about exam success through Watch · Listen · Read, one supporting line, primary + secondary CTA, and a real product visual (app screenshot in a device frame). No clutter.
3. **How students learn — the core section.** A clear 3-step learning journey:
   - **Watch (Daikho)** — video lectures mapped to board textbooks, chapter by chapter.
   - **Listen (Suno)** — audio explanations for revision anywhere.
   - **Read (Parho)** — notes, solved examples, textbook-aligned content.
   Each step with a real UI snapshot or clean illustration, short benefit-focused copy.
4. **Study & test system** — grid of the actual modules: Self-Tests, Daily Tests, Monthly Tests, Objective/Subjective practice, instant results & solutions. Professional cards, consistent icons (one icon set, e.g., Lucide).
5. **Progress & parents** — progress dashboard preview + parent test/visibility feature. This builds trust with parents.
6. **Why it works / stats strip** — chapters covered, video lectures count, tests available, board alignment (Balochistan Board, PTB textbooks). Use real numbers from the project data; never invent fake testimonials.
7. **Final CTA section** — strong, simple, one action.
8. **Footer** — links, contact, institute branding (bbisedemo), language toggle if applicable.

### Mobile-first requirements (the part that got it rejected — get this right)
- Build **mobile-first**, then scale up. Test at 360px, 390px, 768px, 1024px, 1440px.
- No horizontal scrolling at any width. No overlapping or clipped elements.
- Tap targets ≥ 44px. Body text ≥ 16px on mobile. Nav collapses to a clean hamburger drawer.
- Sections stack with consistent vertical rhythm; hero visual scales without cropping key content.
- Lighthouse-level basics: responsive images, no layout shift, fast load.

## PHASE 4 — Verify and prove it

1. Run the app and open the new Landing 4.
2. Capture/verify both **desktop (1440px)** and **mobile (390px)** renders of every section — confirm nothing breaks, overflows, or misaligns.
3. Re-run your Phase 1 audit checklist against the NEW page; every defect category must now pass. If anything still reads as childish or inconsistent, fix it before finishing.
4. Final report: list of files created/changed, the design tokens used, and a before/after summary of what was wrong and how the rebuild fixed it.

## Hard rules
- Do NOT patch or restyle the old page — rebuild it.
- No "Coming Soon", no Lorem Ipsum, no TODOs, no dead buttons or links.
- No emojis in UI copy. No cartoon assets. No more than one accent color.
- Every claim on the page must reflect real features that exist in this app.
