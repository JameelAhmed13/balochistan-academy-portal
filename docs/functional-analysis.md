# Functional Analysis — Balochistan Academy Portal

Each frontend screen is documented with its fields, actions, business logic, and API dependencies.

---

## 1. Landing Page (`/`)

**Purpose:** Marketing / entry point for the platform.

| Field / Element | Type | Notes |
|----------------|------|-------|
| Hero banner | Display | Grade selector, CTA buttons |
| Features section | Display | Key platform features |
| Testimonials | Display | Static / CMS content |
| Login / Register CTA | Navigation | Routes to `/login` and `/register` |

**Actions:**
- Navigate to Login
- Navigate to Register
- Select Grade (pre-fills registration)

**API Dependencies:** None (public page)

---

## 2. Login Page (`/login`)

**Purpose:** Authenticate existing users.

| Field | Type | Validation |
|-------|------|-----------|
| Username | Text | Required, max 40 chars |
| Password | Password | Required |

**Actions:**
- Submit credentials → `POST /api/auth/login`
- Redirect to dashboard on success
- Show error message on failure

**Logic:**
- Stores JWT token and refresh token in localStorage
- Redirects admin to `/admin` and students to `/student`
- Records login history with IP address

**API:** `POST /api/auth/login` → `AuthResponse { token, refreshToken, user }`

---

## 3. Registration Page (`/register`)

**Purpose:** Create a new student account.

| Field | Type | Validation |
|-------|------|-----------|
| Full Name | Text | Required, max 100 chars |
| Username | Text | Required, 3–40 chars, alphanumeric |
| Password | Password | Required, min 4 chars |
| Grade | Dropdown | Required, fetched from API |
| Phone | Text | Optional, max 20 chars |
| Email | Email | Optional, valid email format |
| Institute | Text | Optional |
| Medium | Select | English / Urdu |

**Actions:**
- Load grades → `GET /api/grades`
- Submit → `POST /api/auth/register`
- Redirect to student dashboard on success

**API:** `GET /api/grades`, `POST /api/auth/register`

---

## 4. Student Dashboard (`/student`)

**Purpose:** Central hub for student activity.

| Element | Data Source |
|---------|------------|
| Welcome message with name | Auth store (user.name) |
| Coin balance | `GET /api/coins/balance` |
| Quick stats (attempts, avg %, passed) | `GET /api/tests/me/stats` |
| Subject cards | `GET /api/grades/{code}/subjects` |
| Recent notifications | `GET /api/notifications` |
| Quick test button | Routes to `/daily-tests` |

**Actions:**
- Navigate to subjects
- Start a test
- View coin balance
- Mark notifications read → `PATCH /api/notifications/{id}/read`

---

## 5. Preparation Module (`/preparation`)

**Purpose:** Browse and study curriculum content.

| Element | Data Source |
|---------|------------|
| Grade/Subject selector | `GET /api/grades`, `GET /api/grades/{code}/subjects` |
| Unit/Topic tree | `GET /api/grades/{code}/subjects/{id}/syllabus` |
| Content items (ebooks, past papers, videos) | `GET /api/content?gradeCode=&subjectId=&kind=` |
| Video courses | `GET /api/content/courses?gradeCode=&subjectId=` |
| Student notes | `GET /api/content/notes` |

**Actions:**
- Browse syllabus tree
- View/download content items
- Watch video lessons
- Create/edit/delete personal notes:
  - Create → `POST /api/content/notes`
  - Update → `PUT /api/content/notes/{id}`
  - Delete → `DELETE /api/content/notes/{id}`

---

## 6. AI Tutor (`/ai-tutor`)

**Purpose:** Interact with an AI tutor personalized to the student's grade and subject.

| Element | Data Source |
|---------|------------|
| Tutor selection | `GET /api/tutors?grade=&subject=` |
| Syllabus context (RAG) | `GET /api/ai/context?gradeCode=&subjectId=` |
| Chat interface | Browser → Google Gemini API (direct) |
| AI Tutor persona | AiTutor.SystemPrompt from DB |

**Actions:**
- Select tutor
- Send message (processed by Gemini with context)
- Context is injected from `/api/ai/context` to ground responses

**Logic:** AI processing happens in the browser using the Gemini API. The backend only provides context (syllabus + sample questions).

---

## 7. Daily Tests (`/daily-tests`)

**Purpose:** Take daily objective tests for practice.

| Element | Data Source |
|---------|------------|
| Available tests | `GET /api/tests?gradeCode=&kind=daily&published=true` |
| Random questions (freeform) | `GET /api/questions/random?gradeCode=&subjectId=&count=30` |
| Test questions | `GET /api/tests/{id}` |
| Submit results | `POST /api/tests/attempts` |
| Coin reward | Returned in `AttemptResultDto.coinsEarned` |

**Actions:**
- Browse daily tests
- Start a test (fetch questions)
- Submit answers → `POST /api/tests/attempts`
- View result with coin reward
- View attempt history → `GET /api/tests/attempts`

**Logic:**
- Coins awarded only for tests with ≥35 questions
- Deduplication prevents multiple coin rewards per test

---

## 8. Competition / Leaderboard (`/competition`)

**Purpose:** View top students and compete for rankings.

| Element | Data Source |
|---------|------------|
| Leaderboard table | `GET /api/tests/leaderboard?gradeCode=&top=20` |
| Own stats | `GET /api/tests/me/stats` |
| Entrance test prep | `GET /api/questions/random?kind=objective&isEntranceExam=true` |

**Actions:**
- Filter leaderboard by grade
- Take entrance exam practice
- Submit entrance attempt → `POST /api/tests/attempts`

---

## 9. Coins Wallet (`/coins`)

**Purpose:** Manage coin earnings and withdrawals.

| Element | Data Source |
|---------|------------|
| Coin balance + PKR value | `GET /api/coins/balance` |
| Transaction history | `GET /api/coins/transactions` |
| Payout account | `GET /api/coins/payout-account` |
| Withdrawal requests | `GET /api/coins/withdrawals` |

**Actions:**
- Update payout account → `PUT /api/coins/payout-account`
- Request withdrawal → `POST /api/coins/withdraw`
- View transaction history (paginated)

**Validation:**
- Minimum 300 coins to withdraw
- Only one pending withdrawal at a time
- Payout account required before withdrawal

---

## 10. Reports (`/reports`)

**Purpose:** Student's academic performance analytics.

| Element | Data Source |
|---------|------------|
| Attempt history | `GET /api/tests/attempts` |
| Stats overview | `GET /api/tests/me/stats` |
| Performance charts | Client-side from attempt data |

**Actions:**
- View all past attempts with scores
- See average performance per subject

---

## 11. Complaints (`/complaints`)

**Purpose:** Submit and track support tickets.

| Field | Type | Validation |
|-------|------|-----------|
| Subject | Text | Required, max 200 chars |
| Description | Textarea | Required, 10–2000 chars |
| Category | Select | general / technical / content / payment / account / other |

**Actions:**
- Submit complaint → `POST /api/complaints`
- View own complaints → `GET /api/complaints`
- Track status (open / in-progress / resolved / closed)

---

## 12. Admin Dashboard (`/admin`)

**Purpose:** Overview of platform metrics.

| KPI | Data Source |
|----|------------|
| Total students | `GET /api/admin/analytics` |
| Total questions | `GET /api/admin/analytics` |
| Total tests | `GET /api/admin/analytics` |
| Total attempts | `GET /api/admin/analytics` |
| Total coins in circulation | `GET /api/admin/analytics` |
| Students by grade | `GET /api/admin/analytics` (byGrade array) |

---

## 13. Admin — User Management (`/admin/users`)

**Fields Displayed:** Id, Username, Name, Phone, Email, Role, Grade, Medium, Coins, IsActive, CreatedAt, LastLoginAt

**Actions:**
- Search / filter users → `GET /api/admin/users?role=&gradeCode=&search=`
- Toggle user active/inactive → `PATCH /api/admin/users/{id}/toggle`
- View student test results → `GET /api/admin/users/{id}/results`

---

## 14. Admin — Question Bank (`/admin/questions`)

**Fields:** Kind, Stem, Options (JSON), CorrectIndex, GradeCode, SubjectId, UnitId, Difficulty, Marks, ModelAnswer, CognitiveLevel, SloCode, IsEntranceExam

**Actions:**
- List/search questions → `GET /api/questions?gradeCode=&subjectId=&kind=&search=`
- Create question → `POST /api/questions`
- Update question → `PUT /api/questions/{id}`
- Delete question → `DELETE /api/questions/{id}`

---

## 15. Admin — Test Management (`/admin/tests`)

**Fields:** Title, Kind, GradeCode, SubjectId, DurationMin, TotalMarks, IsPublished, QuestionIds

**Actions:**
- List tests → `GET /api/tests?published=false`
- Create test → `POST /api/tests`
- Toggle publish → `PATCH /api/tests/{id}/publish`
- Delete test → `DELETE /api/tests/{id}`

---

## 16. Admin — Syllabus Management (`/admin/catalog`)

**Entities:** Grades, Subjects, Units, Topics, Learning Objectives, AI Tutors

**Actions:**
- Grade CRUD → `/api/admin/grades`
- Subject CRUD → `/api/admin/subjects`
- Assign subjects to grade → `PUT /api/admin/grades/{code}/subjects`
- Unit CRUD → `/api/admin/syllabus/units`
- Topic CRUD → `/api/admin/syllabus/topics`
- Objective CRUD → `/api/admin/syllabus/objectives`
- AI Tutor CRUD → `/api/admin/tutors`

---

## 17. Admin — Content Management (`/admin/content`)

**Fields:** Title, Kind (video/ebook/past-paper/notes/slides/audio), GradeCode, SubjectId, UnitId, Url, ThumbnailUrl, IsPublished, SortOrder, Tags

**Actions:**
- List published content → `GET /api/content`
- Create content item → `POST /api/admin/content`
- Update content item → `PUT /api/admin/content/{id}`
- Delete content item → `DELETE /api/admin/content/{id}`

---

## 18. Admin — Notifications (`/admin/notifications`)

**Fields:** Title, Body, Type (info/success/warning/error/announcement), TargetRole, TargetGrade, ExpiresAt

**Actions:**
- Broadcast notification → `POST /api/admin/notifications`
- Auto fan-out to matching users (server-side)
- Real-time delivery via SignalR → `/hubs/notifications`

---

## 19. Admin — Withdrawal Management (`/admin/withdrawals`)

**Fields:** UserId, AmountCoins, AmountPkr, Status, AdminNote, CreatedAt, ProcessedAt

**Actions:**
- List all withdrawals → `GET /api/admin/coins/withdrawals?status=pending`
- Approve/Reject/Mark-Paid → `PUT /api/admin/coins/withdrawals/{id}`
- Coins refunded automatically on rejection

---

## 20. Admin — Complaints Management (`/admin/complaints`)

**Actions:**
- List all complaints with filter → `GET /api/admin/complaints?status=open`
- Reply and resolve → `PUT /api/admin/complaints/{id}`

---

## 21. Admin — System Settings (`/admin/settings`)

**Actions:**
- View all settings → `GET /api/admin/settings`
- Update a setting → `PUT /api/admin/settings/{key}`
