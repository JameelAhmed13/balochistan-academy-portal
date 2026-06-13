# eStudy Backend (Express + SQLite)

Real API + database powering the admin/student roles, full K–12 curriculum, the
ingested question bank (~30k questions) and the syllabus extracted from the PDF manuals.

Runs on **port 3000**, which the Vite dev server already proxies (`/api → localhost:3000`).

## Setup

```bash
cd server
cp .env.example .env          # set JWT_SECRET + ADMIN_PASSWORD
npm install
npm run migrate               # create tables in data/estudy.db
npm run seed                  # seed grades/subjects/tutors/admin + load ingested content
```

From the repo root you can also use:

```bash
npm run db:migrate            # = server migrate
npm run db:seed               # = server seed
npm run server                # start API only
npm run dev:all               # Vite (5173) + API (3000) together
```

## Data ingestion → DB

The question bank and syllabus come from the docs via the Python ingester:

```bash
python scripts/ingest/ingest_all.py     # parses xlsx/csv/xls/docx + the 13 PDF manuals
                                         # → scripts/ingest/out/{questions,syllabus,_skipped,_stats}.json
npm run db:seed                          # loads that JSON into SQLite (INSERT OR IGNORE on content_hash)
```

`seed.js` is idempotent and re-runnable: re-running the ingester then `db:seed`
refreshes ingested rows while preserving admin-authored questions/tutors/syllabus.

## Auth

- `POST /api/auth/login` `{username,password}` → `{token,user}` (JWT, role from DB)
- `POST /api/auth/register` `{name,username,password,gradeCode,...}` → student account
- `GET /api/auth/me` · `PUT /api/auth/me/grade` · `POST /api/auth/logout`
- Initial admin seeded from `ADMIN_USERNAME`/`ADMIN_PASSWORD`.

## Route groups

- **Catalog (public reads):** `/api/grades`, `/api/grades/:code/subjects`,
  `/api/grades/:code/subjects/:id/syllabus`, `/api/tutors`
- **Student (auth):** `/api/practice`, `/api/grades/:code/tests`, `/api/tests/:id`,
  `POST /api/attempts`, `/api/attempts/me`, `/api/leaderboard`, `/api/me/stats`
- **Admin (admin only):** `/api/admin/{stats,grades,subjects,syllabus/*,tutors,questions,tests,students}`,
  `/api/admin/ingestion/skipped`
- **AI grounding (auth):** `/api/ai/context?grade=&subject=` → syllabus + sample questions +
  a flat `contextText` the browser injects into Ollama prompts (RAG, DB-sourced).

## Deploy

Netlify only serves the static SPA, so host this backend separately (Render / Railway /
Fly.io / a VPS) with a **persistent disk** for `data/estudy.db`. Then set the
`/api/*` redirect in the repo's `netlify.toml` to that host so the SPA keeps calling
same-origin `/api`. Ollama is local-only; in production the app falls back to the
Gemini path in `src/services/ollamaService.js`.
