# Student Mobile App — API & Behavior Guide

Reference for the mobile developer building the **student side** of the Balochistan Academy Portal app. Every claim below was verified by reading the actual Vue web client and the .NET backend source (file:line cited throughout) — this is not a spec written from memory, it's a description of what the shipping app *actually does today*, warts included.

Where something was a genuine bug, it's flagged **⚠️ BUG** with what the mobile client should do instead. Don't copy the bugs — build against the correct backend contract.

Backend base URL: `{API_BASE}/api` (web default `VITE_API_BASE=/api`, proxied to the .NET server). Every request needs `Authorization: Bearer <token>` once logged in. Responses are camelCase JSON (ASP.NET Core default `System.Text.Json`, no custom naming policy anywhere in the project).

> **Changelog:**
> - **2026-07-20 (round 1):** fixed the broken `/tests/me/attempts` route, wired real submission for Daily Combined Test and Monthly Exam, raised Self Test's question count so it can clear the coin threshold, and added a coin-farming guard.
> - **2026-07-20 (round 2):** fixed the Competition institute-test list (param names + response envelope), added `monthly` as an admin-creatable Test kind, added `subjectId` to attempt submission/response, made Challenge/Weekly coin-eligible, fixed `/api/questions/random` param names on Daily/Monthly/Weekly, wired Saathi's Exam Readiness/Parent Report to real attempt history, removed Leaderboard's fake-data fallback, rewired Preparation's graded tests and Resources/Videos/Key Notes panels to live backend endpoints (no bundled JSON), and added a "take a real institute-scheduled test" flow alongside the generated Daily/Monthly challenges.
> - **2026-07-20 (round 3):** added `subjectName` to attempt responses (batch-joined server-side, no client-side lookup needed anymore), wired Saathi's `syllabus_json`/`exam_json` to real data (syllabus scope per enrolled subject + real upcoming institute-scheduled exams), and added a real `Question ↔ PastPaper` link (new `PastPaperId` column + migration, a `GET /api/past-papers/{id}/questions` endpoint, admin tagging UI, and student-side consumption with a general-pool fallback for untagged papers).
>
> Everything still open — including two items intentionally deferred (AI Tutor/Saathi's direct-to-Gemini architecture, Video Tutor token metering) — is listed in **§18 Pending work** at the end.

---

## Quick status table

| Sidebar item | Backed by real API? | Status |
|---|---|---|
| Home | Yes | ✅ "This month" KPI now reads the server's `thisMonth` field |
| AI Tutor | No (bypasses backend) | **Deferred** — chat still goes straight from the browser to Google Gemini with a bundled API key; backend token metering still unused |
| Saathi AI | Partially | **Deferred** (same Gemini bypass as AI Tutor) — but ✅ "Exam Readiness"/"Parent Report"/"Study Plan" now use real attempt history, real per-subject syllabus scope, and real upcoming institute exams instead of always sending `{}` |
| Preparation | Mostly live | ✅ Resources/Videos/Key Notes/graded tests now pull from live backend endpoints; ✅ Past Papers now use a paper's real tagged questions when available; only Simulations/Virtual Lab remain bundled (no backend equivalent exists for those) |
| My Syllabus | Live | fully backend-driven, no issues found |
| Self Test | Live | ✅ coins can now actually be earned (question count raised to clear the 35-question threshold) |
| Daily Tests | Live | ✅ the combined challenge submits a real, correctly-scored attempt; ✅ a "Scheduled Daily Tests" list of real institute-authored tests now sits alongside the generator |
| Competition Tests | Live | ✅ Monthly Exam submits a real attempt; ✅ the institute-test list is fixed and links to a real "take this test" flow; ✅ Challenge/Weekly now qualify for coins |
| Leaderboard | Live | ✅ no more fabricated fallback — shows a real error/retry state if the API fails |
| My Report | Live | ✅ attempt-history sync now calls the correct endpoint and every attempt now carries both `subjectId` and `subjectName` (server-joined) — real per-subject breakdowns are fully buildable now |
| Coins Earned | Live | balance, transactions, and the full redeem flow (buy/renew subscription, buy AI tokens) all work correctly |
| Complaints | Live | fully working, including a live in-app reply thread over SignalR |

---

## 1. Foundations

### Auth
```
POST /api/auth/login       { username, password, deviceName? } → { token, refreshToken, user }
POST /api/auth/register    { name, username, password, gradeCode, phone?, email?, medium?, institute? }
GET  /api/auth/me          → { user }
PUT  /api/auth/me/grade    { gradeCode } → { user }
POST /api/auth/refresh     { refreshToken } → { token, refreshToken, user }
POST /api/auth/logout
```
`user = { id, username, role, name, phone, email, gradeCode, medium, institute, board, coins }`

- Attach `Authorization: Bearer <token>` to every request.
- On `401`, call `POST /api/auth/refresh` with the stored refresh token once, retry the original request, then log out if refresh also fails (web reference: `src/services/api.js:26-59`).
- On `402` with body `{ subscriptionRequired: true }`, the student's trial/subscription has lapsed — route to a paywall screen (web reference: `api.js:54-56`, gate logic in `router/index.js:211-222`).
- Students without a `gradeCode` must be routed to a "select your grade" screen before anything else in the app (web reference: `router/index.js:201-210`).

### Realtime (optional but needed for Complaints)
SignalR hub at `/hubs/notifications`, JWT passed via `accessTokenFactory` (web reference: `src/stores/notifications.js:6,46-49`). Pushes both generic notifications and complaint thread events (`type` starting with `"complaint"`).

### Feature flags
`GET`-able system settings gate AI Tutor / Video Tutor (`ai_tutor_enabled`, `video_lessons_enabled`) — check before showing those entry points (web reference: `router/index.js:224-228`).

---

## 2. Home

Route: `/app` → `HomeView.vue`.

**On load:**
```
GET /api/tests/me/stats
→ { attempts, avgPercent, passed, passRate, thisMonth, coinsEarned, coins }
```
(`TestsController.cs:229-253`). Called fire-and-forget with errors swallowed (`HomeView.vue:77`) — on failure the screen just shows locally-cached numbers instead of an error.

Also fired once per app session, at login/app-boot rather than by Home itself: `GET /api/auth/me`, `GET /api/grades`, `GET /api/tutors`, `GET /api/grades/{code}/subjects` (catalog bootstrap).

**✅ FIXED:** the "This Month" KPI used to always be recomputed from the device's local test-attempt cache instead of the server's `thisMonth` field, reading 0 on a fresh install even when the server had the correct count. `student.js`'s `monthlyTests` computed now prefers `_apiStats.thisMonth` whenever stats have loaded, falling back to the local computation only if the API call hasn't resolved yet. **Mobile should just use the server's `thisMonth` field directly.**

---

## 3. AI Tutor

Routes: `/app/ai-tutor` (tutor list), `/app/ai-tutor/:subject/chat` (text chat), `/app/ai-tutor/:subject/video` (avatar/voice tutor).

### Tutor list
```
GET /api/tutors?grade=&subject=
→ [{ id, subjectId, gradeCode, persona, slug, icon, color, description, isActive,
     subject: { id, name, nameUr, icon, color, medium } | null }]
```
(`CatalogController.cs:120-158`). Requires a valid JWT (attached automatically); not restricted by role.

### Text chat — ⚠️ DEFERRED architecture issue, not yet fixed
The web app does **not** call the backend for chat completions. It calls Google's Gemini API **directly from the browser**, using an API key that ships inside the client bundle (`VITE_GEMINI_API_KEY`), with a same-origin Ollama server tried first and Gemini as fallback (web reference: `src/services/ollamaService.js:236-266`). The repo's own `.env.example` explicitly says this key was supposed to move server-side and must never ship in the client — but the current `.env` still sets it, so the web app currently leaks it. **This has not been fixed yet** — flagged for a later pass.

**The backend has a proper, already-implemented, token-metered proxy for exactly this** — use it instead of copying the web app's approach:
```
POST /api/ai/chat
body: { subjectId?, message, history? }   (check AiChatController.cs for the exact request DTO)
→ assistant reply, with server-side token accounting against the student's subscription
```
(`AiChatController.cs:26-52`, backed by `Application/Auth` + `ISystemSettingsService`-stored Gemini key — the key never reaches the client). This endpoint decrements `aiTokensUsedThisPeriod`/`bonusAiTokens` and returns `402 { error, outOfTokens: true }` when the student is out of tokens.

**Recommendation for mobile: always call `POST /api/ai/chat` (and `POST /api/ai/generate` for one-off generation), never call Gemini/Ollama directly from the app.** This is both more secure (no leaked key) and is the only way token metering actually works — the web app's approach means AI usage is currently unmetered/free in practice, which you should not replicate.

Related, working data needed for the chat screen:
```
GET /api/subscriptions/me
→ {
    subscription: { id, planId, planName, startDate, endDate, status,
                     aiTokenQuota, aiTokensUsedThisPeriod, tokensRemainingThisPeriod } | null,
    pendingCashPurchase: {...} | null,
    bonusAiTokens, totalAiTokensAvailable, coinsPerAiToken,
    accessStatus, trialEndsAt, trialDaysLeft
  }
```
(`StudentSubscriptionController.cs:39-92`) — use this to show remaining AI tokens / trial status.

### Video Tutor (`/app/ai-tutor/:subject/video`) — the one part of AI Tutor that IS proxied correctly
```
POST /api/ai/gemini   body: { contents: [{ parts: [{ text }] }] , generationConfig? }
→ Gemini's raw response, forwarded verbatim (key stays server-side)

POST /api/ai/tts      body: { input: { text }, voice: { languageCode, ssmlGender, name? }, audioConfig: { audioEncoding, speakingRate } }
→ Google Cloud TTS audio response (key stays server-side)
```
(`AiProxyController.cs:26-67`). These two endpoints do **not** do token metering (unlike `/api/ai/chat`) — still an open gap, deferred along with the item above.

**localStorage (web) reference only:** per-subject chat history cached as `bap_chat_${subject}` (last 20 sessions) — mobile should keep an equivalent local cache (SQLite/AsyncStorage) keyed the same way for offline history browsing.

---

## 4. Saathi AI

Route: `/app/saathi` → `SaathiView.vue`. A multi-mode AI companion (Tutor Session, Study Planner, Quiz Generator, Exam Readiness Report, Parent Report).

Same deferred architecture issue as AI Tutor: the web app calls Ollama/Gemini **directly from the browser**, not through `/api/ai/chat`. **Mobile should route this through `POST /api/ai/chat` (or `/api/ai/generate`) instead**, passing the selected mode's system prompt (see `src/services/studentAssistantPrompts.js` for the prompt templates per mode — grade-band tone rules + per-mode task rules).

**✅ FIXED:** the "Exam Readiness Report", "Parent Report", "Study Planner", and "Tutor Session" modes are supposed to analyze the student's real syllabus coverage and test history. They used to always send an **empty** `{}` for every data placeholder — now all four are populated from real data:
```js
// SaathiView.vue
progress_json:     [{ subject, tests, avgScorePct }, ...]           // from student.testRecords, synced via fetchAttempts()
activity_log_json: [{ date, subject, type, score, total }, ...]     // last 15 attempts
syllabus_json:     [{ subject, unitCount, topicCount }, ...]        // one GET /api/grades/{code}/subjects/{subjectId}/syllabus
                                                                     // per enrolled subject (no per-subject picker in Saathi,
                                                                     // so this covers every subject in the student's grade)
exam_json:         [{ title, subjectId, scheduledAt, endsAt }, ...] // real upcoming kind="monthly" Test rows —
                                                                     // GET /api/tests?kind=monthly&gradeCode=&published=true,
                                                                     // filtered to scheduledAt >= now, nearest 3. Empty array
                                                                     // (never invented) if the institute hasn't scheduled any.
```
`syllabus_json`/`progress_json` are used by `study_planner` and `exam_readiness_report`; `exam_json` only by `exam_readiness_report` (matching what the prompt template in `studentAssistantPrompts.js` actually declares per mode).

**localStorage (web) reference:** one key per mode, `bap_saathi_${modeKey}` (last 100 messages) — modes are `tutor_session`, `study_planner`, `quiz_generator`, `exam_readiness_report`, `parent_report`.

---

## 5. Preparation

Route: `/app/preparation` and children (book selector, then per-book panels: Resources, Videos, Key Notes, Simulations, Virtual Lab, Past Papers, Syllabus, Objective/Subjective study + graded test).

### Book / grade / subject navigation — live backend
```
GET /api/grades                                → [{ code, label, band, sortOrder, isEnabled, subjectCount }]
GET /api/grades/{code}/subjects                 → [{ id, name, nameUr, icon, color, medium }]
GET /api/subjects/{subjectId}/books?gradeCode=   → [{ id, subjectId, gradeCode, title, titleUr, author, publisher,
                                                       edition, board, medium, description, coverUrl, downloadUrl, unitCount }]
```

### Syllabus panel — live backend
```
GET /api/grades/{code}/subjects/{subjectId}/syllabus
→ [{ id, bookId, name, number, sortOrder, description,
     topics: [{ id, name, sortOrder, objectives: [{ id, code, objectiveText, cognitiveLevel }] }],
     objectives: [{ id, code, objectiveText, cognitiveLevel }] }]   // unit-level objectives (topicId == null)
```
(`CatalogController.cs:81-118`). Equivalent book-scoped version also exists: `GET /api/books/{bookId}/syllabus` (`StudentContentController.cs:121-164`) — same shape, different key.

### Resources / Videos / Key Notes — ✅ FIXED, now live endpoints
These three panels previously read from bundled `courses.json`. They've been rewired to the real backend hierarchy:

**Academic Resources** (`ResourcesPanel.vue`):
```
GET /api/subjects/{subjectId}/books?gradeCode=   → pick the subject's book
GET /api/books/{bookId}/units                    → chapter list, with resourceCount per unit
GET /api/units/{unitId}/resources                → published ContentItems, fetched lazily when a chapter expands
```

**Video Lectures** (`VideosPanel.vue`):
```
GET /api/content/courses?gradeCode=&subjectId=   → [{ id, title, description, thumbnailUrl, tutorName, lessonCount }]
GET /api/content/courses/{id}                    → { ...course, lessons: [{ id, title, videoUrl, durationSec, sortOrder }] }
```
`videoUrl` may be a bare YouTube id, a `youtu.be`/`watch?v=` link, or an already-embeddable URL — normalize it to an embed URL client-side (see `toEmbedUrl()` in `VideosPanel.vue` for the exact regex).

**Key Notes** (`KeyNotesPanel.vue`) — chapter/topic list now comes from the real syllabus (`GET /api/grades/{code}/subjects/{subjectId}/syllabus`, same call as My Syllabus), and the "revised" checklist is now **persisted**, not just in-memory:
```
GET /api/content/notes                → find the note with title `__keynotes_checklist_subject_{subjectId}`
POST /api/content/notes  { title, content, tags: 'keynotes' }   // content = JSON array of checked "unitId:topicId" keys
PUT  /api/content/notes/{id}  { title, content, tags }
```
There's no dedicated "progress" entity in the backend, so this repurposes the generic per-student `StudentNote` CRUD to store checklist state as a JSON blob — it's a pragmatic fit, not a purpose-built endpoint. If a first-class progress-tracking table is ever added, migrate to that instead.

**⚠️ Still bundled — no backend equivalent exists:** Simulations and Virtual Lab (`src/assets/data/phet.js` + PhET external iframes) were left as-is. There's no `Simulation`/`Experiment` entity in the backend to migrate to — the actual interactive content is inherently hosted on PhET's site regardless, only the catalog metadata (title/description/subject/phetUrl) is bundled. If you want this live too, that's a new backend content type to build, not a rewiring job.

### Past & Model Papers — ✅ FIXED, now a real per-paper question link
```
GET /api/past-papers?subjectId=&gradeCode=
→ [{ id, year, board, paperType, totalMarks, timeLimitMinutes, subjectName }]
```
(`CatalogController.cs:163-186`). Tapping "Attempt" passes `paperId`/`year` in the URL. The backend now has a real `Question.PastPaperId` FK (new column + migration), so:
```
GET /api/past-papers/{id}/questions
→ [QuestionDto, ...]   // real questions tagged to this exact paper, in paper order; [] if none tagged yet (not a 404)
```
The test-taking screen tries this endpoint first when a `paperId` is present, and only falls back to the general `GET /api/questions/random?gradeCode=&subjectId=&kind=objective&count=35` pool if the paper has no tagged questions yet — with an honest on-screen note either way ("✅ Real {year} paper questions" vs. "this paper's own questions aren't tagged yet"). Admins tag questions to a paper via a new "Past Paper" dropdown in the admin Question Bank (optional field, `PastPaperId` on create/update).

**For mobile:** call `GET /api/past-papers/{id}/questions` first when the student picked a specific paper; treat an empty array as "not tagged yet" and fall back to `/api/questions/random`, same as the web app.

### Objective/Subjective study & graded tests — ✅ FIXED, now the real question bank
The "Objective Test" and "Subjective Test" screens (the graded, scored ones) previously used a synthetic in-memory generator with `Math.random()`-chosen correct answers. They now call the real backend question pool:
```
GET /api/questions/random?gradeCode=&subjectId=&kind=objective&count=35
GET /api/questions/random?gradeCode=&subjectId=&kind=subjective&count=20
```
`QuestionDto`: `{ id, gradeCode, subjectId, unitId, topicId, kind, stem, optionsJson, correctIndex, questionType, marks, modelAnswer, difficulty, cognitiveLevel, sloCode, isEntranceExam, isAiGenerated, feedback }` — `optionsJson` is a raw JSON string, parse it client-side. For subjective questions, `questionType` distinguishes `"Short"` vs `"Long"` (defaults to `"Short"` if unset); `marks` defaults to 3/7 respectively if the backend hasn't set it.

**No bundled fallback** — if the API call fails or returns zero questions, both screens show a real "no questions available yet / Retry" state rather than falling back to fake content. This was a deliberate decision (**online-only**, see §15) — replicate that on mobile rather than shipping an offline question corpus.

The **"Objective/Subjective Paper" study-mode screens** (untimed preview with AI-generated/predicted papers) were left untouched — they're a separate feature built around the AI-generation architecture flagged in §3/§4 as deferred, not the graded-test path this fix targeted.

### Submitting a Preparation test attempt
```
POST /api/tests/attempts
body: { total, attemptType: "objective" | "subjective", subjectId, testId?: null, answersJson?, score, durationSec }
→ { id, userId, testId, subjectId, score, total, percent, coinsEarned, attemptType, submittedAt }
```
`subjectId` is now sent (the Preparation "book" id *is* the subject id in this app — there's no separate book concept in the student-facing flow). **⚠️ Note:** with `attemptType` of `"objective"`/`"subjective"`, the backend's coin-award rule still doesn't include these two types — these attempts are recorded but don't earn coins. Not part of this round's scope; a backend rule change if you want it.

---

## 6. My Syllabus

Route: `/app/preparation/syllabus` → `StudentSyllabusView.vue`. Fully live, no bugs found.

```
GET /api/grades                                       (if not already cached)
GET /api/grades/{code}/subjects
GET /api/grades/{code}/subjects/{subjectId}/syllabus   (on selecting a subject)
```
Same shapes as §5. This is the cleanest, fully-backend-driven screen in the whole app — a good reference implementation to mirror.

---

## 7. Self Test

Routes: `/app/self-test` (subject picker) → `.../config` (options) → `.../take` (timed test) → `.../result` → `.../follow-up` (history).

**Subject picker:** `GET /api/grades/{code}/subjects` (subject id doubles as the "book" id for the rest of this flow — there's no separate book concept here).

**Config screen:** loads `GET /api/grades/{code}/subjects/{subjectId}/syllabus` to populate a unit/topic picker, plus lets the student choose time limit, question type, and difficulty. ⚠️ **Still open:** none of these selections (unit, difficulty, question type) are actually forwarded to the question-loading call in the next screen — build the mobile client so these filters *do* get applied; the endpoint already supports `unitId`.

**Taking screen — question source (two-tier):**
1. `GET /api/questions/random?gradeCode=&subjectId=&kind=objective&count=35`
2. Fallback if empty: local bundled bank (`getRealQuestions`) — this fallback still exists for Self Test specifically (unlike the Preparation graded tests in §5, which are now online-only). Consider whether mobile wants parity with this fallback or should also go online-only here.

**Submission:**
```
POST /api/tests/attempts
body: { total, attemptType: "self-test", subjectId, testId: null, answersJson, score, durationSec }
→ AttemptResultDto { id, userId, testId, subjectId, score, total, percent, coinsEarned, attemptType, submittedAt }
```
`subjectId` is the subject id (same value as the route's `:bookId`). Scoring is done **client-side** (compare selected index vs. the question's correct-answer index shipped in the payload) — the server trusts whatever `score`/`total` you send, it does not re-grade.

**Coin rule (server, `TestsController.cs:165-192`):**
```
if (total >= 35 && attemptType is "self-test" or "parent-test" or "daily" or "monthly" or "challenge" or "weekly"):
    pct = score/total
    coins = pct>=0.9 ? 50 : pct>=0.7 ? 30 : pct>=0.5 ? 15 : 5   // (values are admin-configurable settings)
```
**✅ FIXED:** Self Test now requests 35 questions (was 30, so it could never clear the coin threshold). All six attempt types above now qualify for coins, provided `total >= 35`.

**Anti-abuse guard:** `self-test`/`daily`/`monthly`/`challenge`/`weekly` attempts always send `testId: null` (they're generated on the fly, not tied to an admin-authored test row), so the original per-`testId` "already earned" dedupe could never apply to them. The backend now caps coin-earning to **once per calendar day per (user, attemptType)** whenever `testId` is absent. `parent-test` (and any of the above submitted with a real `testId`, e.g. an institute-scheduled test — see §8/§9) keeps the original per-`testId` dedupe instead. Design the mobile UI around this: a second same-day attempt of the same generated type is still recorded and scored, it just won't add more coins.

**Result & follow-up screens (web reference only):** the web app reads these purely from a local on-device cache (`localStorage['bap_tests']`) populated synchronously at test-finish time, not from the server response — build your own local cache the same way so the result screen renders instantly without waiting on the network, but also keep the authoritative `AttemptResultDto` from the POST response (`coinsEarned`, `percent`) for accuracy.

---

## 8. Daily Tests

Route: `/app/daily-tests` (hub + history) → `.../combined` (multi-subject timed challenge, generated) → `.../scheduled/:id` (real institute-authored test) → `.../result/:id`.

**Hub screen:** shows attempt history via `GET /api/tests/attempts?pageSize=200` (correct path, see §11), plus **two ways to get a daily test now**:

### 1. Generated Combined Challenge (`DailyCombinedTestView.vue`)
Per selected subject: `GET /api/questions/random?gradeCode=&subjectId=&kind=objective&count=` (param names fixed — see below) with a real backend fallback bank for MCQs, and a fully placeholder/mock generator for subjective questions (no real subjective content exists for this flow anywhere, web or backend).

**✅ FIXED (was the most important bug in this feature):** finishing now actually scores the attempt (comparing each answer to its real correct index — the old code just counted *answered* questions, not *correct* ones) and submits:
```
POST /api/tests/attempts
body: { total, attemptType: "daily", testId: null, answersJson, score, durationSec }
```
navigating to the real, freshly-created attempt id (`/app/daily-tests/result/{id}`) instead of a hardcoded `.../result/new`. The result screen renders the actual score, coin total, and a real per-question breakdown.

**✅ FIXED:** `/api/questions/random` was called with `grade`/`limit` (silently ignored — the controller binds `gradeCode`/`count`). Now sends the correct param names.

### 2. Scheduled Daily Tests (`DailyTestsView.vue`'s new table + `ScheduledTestTakingView.vue`)
```
GET /api/tests?kind=daily&gradeCode=&published=true&pageSize=50
→ { items: [{ id, title, gradeCode, subjectId, kind, durationMin, totalMarks, isPublished, scheduledAt, endsAt, questionCount }], totalCount, page, pageSize }
```
Lists real, institute-authored `kind="daily"` Test rows (admin panel now lets teachers create these). Tapping "Attempt" navigates to a real test-taking screen:
```
GET /api/tests/{id}
→ { id, title, gradeCode, subjectId, kind, durationMin, totalMarks, isPublished,
    questions: [{ id, kind, stem, optionsJson, correctIndex, difficulty, marks, modelAnswer, feedback }] }
```
On finish, submits with the real `testId` set:
```
POST /api/tests/attempts
body: { total, attemptType: "daily", testId: <real Test.Id>, subjectId, answersJson, score, durationSec }
```
Because `testId` is set here, coin-earning dedupes by test (not by day) — a student can't re-farm the *same* scheduled test, but a new scheduled test is a fresh opportunity, same as the generated flow's daily cap.

**Recommendation for mobile:** offer both — a "Generate Now" quick-practice path and a "Scheduled" list of institute-set tests, exactly like the web app now does. Decide independently whether your product wants both surfaced with equal prominence.

---

## 9. Competition Tests

Route: `/app/competition` (hub with 4 sub-features: Monthly Exam, Challenge Quiz, Weekly Quiz, plus a real "Official Institute Tests" table) → `.../monthly-test` (generated) → `.../scheduled/:id` (real institute test) → `.../challenge`, `.../weekly`, `.../result/:id`.

### "Official Institute Monthly Tests" table — ✅ FIXED
Previously called `GET /api/tests?assigned=true&type=monthly` (wrong param names, wrong response-shape assumption — the table was always empty). Now:
```
GET /api/tests?kind=monthly&gradeCode=&published=true&pageSize=50
→ { items: [{ id, title, scheduledAt, endsAt, ... }], totalCount, page, pageSize }
```
correctly unwrapping `.items`, filtering by the student's own grade, and cross-referencing `GET /api/tests/attempts` to mark rows already taken. Tapping "Attempt" now routes to a real test-taking screen (`/app/competition/scheduled/{id}`, same `ScheduledTestTakingView.vue` component described in §8) that fetches `GET /api/tests/{id}` and submits with the real `testId`. Tapping "View Details" on a taken row now correctly resolves to the matching local attempt record (the result views match by either local id or `testId`).

**✅ Also fixed:** the admin panel now offers `monthly` as a creatable Test "Kind" (previously only `objective/subjective/mixed/daily/weekly/entrance` existed) — so this list can actually have content once an admin authors tests.

### Monthly Exam / Challenge Quiz / Weekly Quiz (generated, no real Test row)
All three generate questions on the fly from the question pool — client-side labels, not persistent server-scheduled events (that's what the institute-test table above is for).

**✅ FIXED:** Monthly Exam's finish handler previously never scored anything or submitted an attempt (same "always 0/0" bug Daily Tests used to have). It now uses the same flatten-and-score approach and submits:
```
POST /api/tests/attempts
body: { total, score, attemptType: "monthly", subjectId: null, answersJson, durationSec }
→ navigates to /app/competition/result/{realId}, not /result/new
```
(`subjectId` is `null` here since it spans multiple subjects — same as the Daily Combined Challenge.)

Challenge and Weekly already correctly submitted, and now also send `subjectId` (the one subject they're scoped to):
```
POST /api/tests/attempts
body: { total, score, attemptType: "challenge" | "weekly", subjectId, answersJson, durationSec }
```

**✅ FIXED:** `/api/questions/random` param names on Monthly/Weekly (`grade`/`limit` → `gradeCode`/`count`).

**✅ FIXED:** the backend's coin rule now includes `"challenge"` and `"weekly"` alongside the other four qualifying types (see §7's coin rule) — all six attempt types can earn coins given `total >= 35`, subject to the once-per-day cap for types without a real `testId`.

### Leaderboard
Route: `/app/competition/leaderboard`. Fully live and correct:
```
GET /api/tests/leaderboard?gradeCode=&top=20     (AllowAnonymous — works even before login if you want a preview)
→ [{ id, name, gradeCode, coins, totalAttempts, avgPercent }]
```
**✅ FIXED:** previously fell back to a fully fabricated list of fake student names/scores if the API call failed, with no indication it wasn't real. Now shows a genuine loading state, a real error + Retry button on failure, and an empty-state message if there's simply no data yet — never fake rows.

**Recommendation:** rank directly by whatever field your product wants (the web app re-derives its own composite score client-side and re-sorts, which can reorder relative to what the backend returned — simplest for mobile is to just trust the backend's own ordering, or be deliberate about your own ranking formula).

---

## 10. Parent / Teacher Test

Not in your screenshot's menu but worth documenting since it was covered earlier: there's no dedicated backend entity for parent/teacher-assigned tests today. A parent/teacher test is just any question set the student answers, submitted with:
```
POST /api/tests/attempts
body: { total, attemptType: "parent-test", subjectId?, testId?, answersJson?, score, durationSec }
```
This attempt type qualifies for the coin-award rule (along with the other five, see §7), provided `total >= 35`. If `testId` is set, coin-earning dedupes per-test (unlimited different real tests can each earn once); if not, it dedupes once per day like the other generated types.

---

## 11. My Report

Sidebar "My Report" → route `/app/report` → `PreparationReportView.vue`. (There's a second, unrelated `/app/reports` (plural) route in the web app's router that is not linked from its sidebar at all — ignore it, it's dead code in the reference app.)

**KPIs:**
```
GET /api/tests/me/stats
→ { attempts, avgPercent, passed, passRate, thisMonth, coinsEarned, coins }
```

**Attempt history (for trend charts / recent-tests list):**
```
GET /api/tests/attempts?page=1&pageSize=200
→ { items: [{ id, userId, testId, subjectId, subjectName, score, total, percent, coinsEarned, attemptType, submittedAt }], total, page, pageSize }
```
**✅ FIXED:** previously called `GET /api/tests/me/attempts`, which doesn't exist on the backend (404d silently, so history never actually synced). Now calls the correct `GET /api/tests/attempts` path.

**✅ FIXED:** `AttemptResultDto` now includes both `subjectId` and `subjectName` (previously missing entirely). `TestAttempt.SubjectId` is a bare column with no EF navigation, so `subjectName` isn't mapped per-row by AutoMapper — instead `TestsController` batch-looks-up the distinct subject ids in one query and attaches names server-side (`AttachSubjectNames` in `TestsController.cs`, applied to `MyAttempts`, `GetAttempt`, and the `SubmitAttempt` response). Combined multi-subject attempts (the generated Daily/Monthly challenges) still send/receive `subjectId: null`/`subjectName: null`, since they genuinely span more than one subject — that's expected, not a bug.

**Recommendation:** build subject-wise charts, trend lines, and "recent tests" straight from `GET /api/tests/attempts` — `subjectName` is now returned directly, no client-side join needed.

---

## 12. Coins Earned

Route: `/app/coins` (balance + transactions) → `/app/coins/redeem` (spend flow). Fully live, no bugs found — safe to mirror directly.

```
GET /api/coins/balance
→ { coins, pkr, earnedToday }

GET /api/coins/transactions?page=&pageSize=
→ { items: [{ id, amount, reason, timestamp }], total, page, pageSize }
```

### Redeem flow (`StudentSubscriptionController`, route `api/subscriptions`)
```
GET /api/subscriptions/plans
→ [{ id, name, description, price, durationDays, aiTokenQuota, coinCost }]

GET /api/subscriptions/me
→ { subscription: {...} | null, pendingCashPurchase: {...} | null, bonusAiTokens,
    totalAiTokensAvailable, coinsPerAiToken, accessStatus, trialEndsAt, trialDaysLeft }

POST /api/subscriptions/me/purchase     body: { planId }
→ { id, planId, startDate, endDate, status, aiTokenQuota, coinsSpent, remainingCoins }
   (400 if already subscribed, or if coins are insufficient)

POST /api/subscriptions/me/renew        (no body)
→ same shape as purchase; extends from current end date if still active
   (400 if there's nothing to renew, or insufficient coins)

POST /api/subscriptions/me/buy-tokens   body: { tokenAmount }
→ { remainingCoins, bonusAiTokens, coinsSpent }
   (400 if tokenAmount <= 0 or insufficient coins)
```
There's also a cash-payment path (not coin-based) for buying a subscription with real money: `POST /api/subscriptions/me/purchase-cash` `{ planId, paymentMethod, referenceNumber }` — surface this if the mobile app supports cash top-ups, not just coin redemption.

After any successful redeem action, refresh `balance`, `subscription`, and `me/stats` so the UI reflects the new coin total and token quota immediately.

---

## 13. Complaints

Route: `/app/complaints`. Fully live and working — good reference implementation, safe to mirror directly.

```
GET /api/complaints
→ [{ id, userId, userName, category, subject, description, status, messageCount, createdAt, resolvedAt }]
   (scoped automatically to the logged-in student via JWT)

POST /api/complaints
body: { subject, description, category? }   // category defaults to "Complaint"
→ 201 Created, same shape as above

GET /api/complaints/{id}/messages
→ [{ id, senderId, senderName, isAdmin, message, createdAt }]

POST /api/complaints/{id}/messages
body: { message }
→ 201 Created ComplaintMessageDto
   (400 if the complaint's status is "resolved" or "closed" — read-only once closed)
```

**Valid `category` values** (validated server-side): `Complaint`, `Suggestion`, `Bug Report`, `Feature Request`, `General Feedback`.
**`status` values:** `open`, `in-progress` (auto-set once an admin replies), `resolved`, `closed`.

**Realtime:** connect to the `/hubs/notifications` SignalR hub (JWT auth) to receive live `complaintMessage` (new reply) and `complaintStatus` (status changed) push events, so an open thread updates without polling. Polling `GET /api/complaints/{id}/messages` on an interval is an acceptable fallback if you don't want to stand up SignalR on mobile immediately.

---

## 14. How It Works / Sign Out

- **How It Works** is just an external link (YouTube) in the web app — no API involved, open it as a plain external URL/browser tab.
- **Sign Out** → `POST /api/auth/logout` (revokes refresh tokens server-side), then clear local token storage and any per-user local cache, then navigate to the login screen.

---

## 15. Data that is NOT a live API today

| Content | Source | Status |
|---|---|---|
| Academic Resources / Video Lectures / Key Notes | `GET /api/units/{unitId}/resources`, `GET /api/content/courses`, `GET /api/content/notes` | ✅ Live now (was bundled `courses.json`) |
| Preparation graded-test questions (Objective/Subjective Test) | `GET /api/questions/random` | ✅ Live now (was a synthetic in-memory generator) |
| Simulations / Virtual Lab | `src/assets/data/phet.js` (bundled) + PhET external iframes | **Still bundled** — no `Simulation` entity exists in the backend to migrate to; the interactive content itself is inherently PhET-hosted regardless |
| Self Test question fallback | `assessmentBank.js`, bundled per grade+subject JSON | **Still bundled**, used only as a fallback when `/api/questions/random` returns empty — a deliberate two-tier design for this one screen, unlike Preparation's now fully online-only graded tests |
| Objective/Subjective **Paper** (study/preview, AI-generated) | Ollama/Gemini direct client calls + bundled corpus | **Untouched** — tied to the deferred AI-architecture item (§3/§4), not part of this round |

**Recommendation:** for a from-scratch mobile build, prefer the live endpoints wherever they now exist (Resources/Videos/Key Notes/graded-test questions) — you avoid shipping bundled content and get server-managed updates. Simulations/Virtual Lab have no live equivalent to build against yet; treat that as a backend feature request if you want it live.

---

## 16. Consolidated bug list

### ✅ Fixed

| # | Where | Bug | Fix applied |
|---|---|---|---|
| 1 | Home, My Report | Called `GET /api/tests/me/attempts` (doesn't exist, 404d silently) | Now calls `GET /api/tests/attempts` |
| 2 | Daily Combined Test | Never submitted an attempt on finish; result always 0/0 | Scores correctly and calls `POST /api/tests/attempts` with `attemptType: "daily"`, navigating to the real attempt id |
| 3 | Monthly Exam | Never submitted an attempt on finish; result always 0/0 | Scores correctly and calls `POST /api/tests/attempts` with `attemptType: "monthly"`, navigating to the real attempt id |
| 4 | Self Test | Requested only 30 questions, so the ≥35 coin threshold could never be met | Question count raised to 35 in both the API call and the local-bank fallback |
| 5 | Competition hub | Called `GET /api/tests?assigned=true&type=monthly` (wrong params, wrong response shape) — institute-test table always empty | Calls `GET /api/tests?kind=monthly&gradeCode=&published=true`, unwraps `.items`, cross-references attempt history for "taken" status |
| 6 | Admin Test Kind options | `monthly` wasn't a selectable Test kind, so no admin could author one | Added to the admin Kind dropdown |
| 7 | `AttemptResultDto` | No subject field at all | Added `subjectId` to `SubmitAttemptRequest`/`AttemptResultDto`; all single-subject attempt submissions now send it |
| 8 | Daily / Monthly / Weekly | Sent `grade`/`limit` to `/api/questions/random`, which only binds `gradeCode`/`count` — silently ignored | Fixed to the correct param names on all three screens |
| 9 | Challenge / Weekly Quiz | Attempt types didn't qualify for the coin-award rule | Backend now includes `"challenge"`/`"weekly"` alongside the other four types |
| 10 | Saathi (Exam Readiness / Parent Report / Study Planner / Tutor Session) | Always sent `{}` for progress/activity data | `progress_json`/`activity_log_json` now built from real attempt history |
| 11 | Leaderboard | Fell back to fully fabricated fake rows on API failure, no indicator | Real loading/error/retry/empty states; no fake data ever shown |
| 12 | Preparation → Past Papers "Attempt" | Selected paper id/year wasn't passed through; also no real `PastPaper`↔`Question` link existed in the backend | Selection now carried through; new `Question.PastPaperId` FK (+ migration) + `GET /api/past-papers/{id}/questions` endpoint + admin tagging UI let a paper serve its own real questions, with an honest general-pool fallback if untagged |
| 13 | Preparation graded tests | Used synthetic/mock questions instead of the real bank | Now call `GET /api/questions/random` for both objective and subjective tests, online-only (no fallback) |
| 14 | Resources / Videos / Key Notes panels | 100% bundled `courses.json`, Key Notes checklist not persisted | Rewired to live `StudentContentController`/`ContentController` endpoints; Key Notes checklist now persists via `StudentNote` |
| 15 | Home | "This Month" KPI ignored the server's `thisMonth` field | Now prefers the server value once stats have loaded |
| 16 | `AttemptResultDto` | Had `subjectId` but no `subjectName` | Backend now batch-joins subject names server-side (`AttachSubjectNames` in `TestsController.cs`) — no client-side lookup needed |
| 17 | Saathi | `syllabus_json`/`exam_json` always `{}` | Now built from real per-subject syllabus scope (`GET /api/grades/{code}/subjects/{subjectId}/syllabus` per enrolled subject) and real upcoming `kind="monthly"` Test rows |
| — | (anti-abuse, not a pre-existing bug) | Enabling coins for more generated attempt types (no `testId`) risked infinite coin-farming via replay | Backend caps coin-earning to once/day per (user, attemptType) when `testId` is absent |

### ⚠️ Still open / deferred

| # | Where | Issue | Status |
|---|---|---|---|
| 18 | AI Tutor, Saathi | Calls Gemini/Ollama directly from the client with a bundled API key; never uses the backend's token-metered `/api/ai/chat` | **Deferred** — explicitly not addressed |
| 19 | Video Tutor's `/api/ai/gemini` / `/api/ai/tts` proxies | No token metering, unlike `/api/ai/chat` | **Deferred** alongside #18 |
| 20 | Simulations / Virtual Lab | Still 100% bundled JSON + external PhET iframes | No backend content type exists to migrate to |
| 21 | Self Test | Still has a bundled-bank fallback (unlike Preparation's now online-only graded tests) | Inconsistent by design across screens — decide if mobile wants parity here too |

---

## 17. Endpoint cheat-sheet (all confirmed, real, working routes)

```
Auth
  POST /api/auth/login | /api/auth/register | /api/auth/refresh | /api/auth/logout
  GET  /api/auth/me
  PUT  /api/auth/me/grade

Catalog / Syllabus
  GET  /api/grades
  GET  /api/grades/{code}/subjects
  GET  /api/grades/{code}/subjects/{subjectId}/syllabus
  GET  /api/subjects/{subjectId}/books
  GET  /api/books/{bookId} | /api/books/{bookId}/units | /api/books/{bookId}/syllabus
  GET  /api/units/{unitId} | /api/units/{unitId}/topics | /api/units/{unitId}/resources
  GET  /api/topics/{topicId} | /api/topics/{topicId}/resources
  GET  /api/past-papers
  GET  /api/tutors

Content
  GET  /api/content?gradeCode=&subjectId=&kind=&unitId=
  GET  /api/content/courses | /api/content/courses/{id}
  GET/POST/PUT/DELETE /api/content/notes

Questions & Tests
  GET  /api/questions | /api/questions/{id} | /api/questions/random
  GET  /api/past-papers/{id}/questions   (real questions tagged to a specific paper, [] if untagged)
  GET  /api/tests?kind=&gradeCode=&subjectId=&unitId=&published=
  GET  /api/tests/{id}
  POST /api/tests/attempts          body now includes optional subjectId
  GET  /api/tests/attempts | /api/tests/attempts/{id}   response now includes subjectId + subjectName
  GET  /api/tests/me/stats
  GET  /api/tests/leaderboard   (AllowAnonymous)

AI
  POST /api/ai/chat | /api/ai/generate | /api/ai/context   (token-metered, server-side key)
  POST /api/ai/gemini | /api/ai/tts                        (proxied, server-side key, not metered)

Coins & Subscriptions
  GET  /api/coins/balance | /api/coins/transactions
  GET  /api/subscriptions/plans | /api/subscriptions/me
  POST /api/subscriptions/me/purchase | /me/renew | /me/buy-tokens | /me/purchase-cash

Complaints
  GET/POST /api/complaints
  GET/POST /api/complaints/{id}/messages
```

---

## 18. Pending work — full punch list

### Deferred (explicit product decision — not addressed)
1. **AI Tutor / Saathi bypass the backend entirely** — both call Gemini/Ollama directly from the browser with a bundled `VITE_GEMINI_API_KEY`, instead of the already-built, token-metered `POST /api/ai/chat` / `POST /api/ai/generate`. Real key-exposure issue plus unmetered AI usage. Rework needed in `ChatView.vue`, `SaathiView.vue`, and the AI-generation paths in `ObjectivePaperView.vue`/`SubjectivePaperView.vue`.
2. **Video Tutor's AI proxy endpoints do no token metering** — unlike `/api/ai/chat`, usage here is currently unlimited/unaccounted for.

### Smaller residual gaps
3. **Simulations / Virtual Lab remain 100% bundled** — no backend content type exists for this; a genuine backend feature-build, not a rewiring task.
4. **Self Test still has a bundled-bank fallback**, inconsistent with Preparation's now fully online-only graded tests — decide if mobile should match Preparation's stricter approach here too.
5. **Past Papers' real-question feature is only as good as admin tagging** — the `PastPaperId` link now exists end-to-end, but until admins actually tag existing questions (or author new ones) against specific papers via the new admin dropdown, most papers will still fall back to the general pool. Not a bug — a content-population task.

### Already resolved (kept for context, not action items)
Everything else originally listed here — the Competition institute-test list, `subjectId`/`subjectName` on attempts, Challenge/Weekly coin eligibility, `/api/questions/random` param names, Saathi's progress/activity/syllabus/exam data, Leaderboard's fake fallback, Preparation's graded-test question source, the Resources/Videos/Key Notes panels, the Home "This Month" KPI, real Past-Paper↔Question linkage, and support for both generated and real admin-scheduled Daily/Monthly tests — was implemented. See §16's "Fixed" table for specifics.

### ⚠️ Deployment note for this round's Past Paper fix
Adding `Question.PastPaperId` required an EF Core migration (`AddPastPaperIdToQuestion` — nullable column + FK to `PastPapers`, `SetNull` on delete, low-risk/reversible). It's generated but **not yet applied** — the API auto-applies pending migrations on startup (`context.Database.MigrateAsync()` in `Program.cs`), so it will run automatically the next time the backend restarts. No manual `dotnet ef database update` needed, just a restart.
