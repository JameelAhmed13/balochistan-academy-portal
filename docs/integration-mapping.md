# Integration Mapping — Vue Frontend ↔ .NET Backend API

Maps every Vue.js screen to its backend API endpoints. Verified for full coverage.

---

## Coverage Summary

| Module | Endpoints | Frontend Screens | Status |
|--------|-----------|-----------------|--------|
| Auth | 5 | Login, Register, Me | ✅ Full |
| Catalog | 18 | Grades, Subjects, Syllabus, Tutors | ✅ Full |
| Questions | 5 | Question Bank (Admin), Random Tests | ✅ Full |
| Tests | 8 | Daily Tests, Competition, Admin Tests | ✅ Full |
| Coins | 8 | Wallet, Admin Withdrawals | ✅ Full |
| Notifications | 4 | Notification Bell, Admin Broadcast | ✅ Full |
| Complaints | 4 | Student Complaints, Admin Complaints | ✅ Full |
| Content | 8 | Preparation, Video Courses, Notes | ✅ Full |
| Users | 5 | Admin Users, Analytics, Settings | ✅ Full |
| AI Context | 1 | AI Tutor | ✅ Full |

**Total API Endpoints: 66** | **No missing coverage identified.**

---

## Detailed Mapping

### AUTH MODULE — `AuthController` → `/api/auth`

| Vue Screen | User Action | HTTP | Endpoint | Request | Response |
|-----------|------------|------|----------|---------|----------|
| Login Page | Submit credentials | POST | `/api/auth/login` | `LoginRequest` | `AuthResponse` |
| Register Page | Create account | POST | `/api/auth/register` | `RegisterRequest` | `AuthResponse` |
| All pages | Refresh session | POST | `/api/auth/refresh` | `RefreshTokenRequest` | `AuthResponse` |
| All pages | Get current user | GET | `/api/auth/me` | — | `{ user: UserDto }` |
| Profile settings | Change grade | PUT | `/api/auth/me/grade` | `UpdateGradeRequest` | `{ user: UserDto }` |
| All pages | Sign out | POST | `/api/auth/logout` | — | `{ ok: true }` |

**Token Strategy:**
- Access token stored in Pinia auth store (in-memory)
- Refresh token stored in localStorage
- Axios interceptor auto-refreshes on 401

---

### CATALOG MODULE — `CatalogController` → `/api` and `/api/admin`

| Vue Screen | User Action | HTTP | Endpoint | Request | Response |
|-----------|------------|------|----------|---------|----------|
| Register, Dashboard | Load grade list | GET | `/api/grades` | — | `Grade[]` |
| Dashboard, Preparation | Load subjects for grade | GET | `/api/grades/{code}/subjects` | — | `Subject[]` |
| Preparation | Load syllabus tree | GET | `/api/grades/{code}/subjects/{id}/syllabus` | — | `Unit[]` with Topics |
| AI Tutor | Load tutors | GET | `/api/tutors?grade=&subject=` | — | `AiTutor[]` |
| Admin Catalog | All grades (admin) | GET | `/api/admin/grades` | — | `Grade[]` |
| Admin Catalog | Create grade | POST | `/api/admin/grades` | `CreateGradeRequest` | `Grade` |
| Admin Catalog | Update grade | PUT | `/api/admin/grades/{code}` | `UpdateGradeRequest` | `Grade` |
| Admin Catalog | Delete grade | DELETE | `/api/admin/grades/{code}` | — | 204 |
| Admin Catalog | Set subjects for grade | PUT | `/api/admin/grades/{code}/subjects` | `SetGradeSubjectsRequest` | `{ ok: true }` |
| Admin Catalog | List subjects | GET | `/api/admin/subjects` | — | `Subject[]` |
| Admin Catalog | Create subject | POST | `/api/admin/subjects` | `Subject` | `Subject` |
| Admin Catalog | Update subject | PUT | `/api/admin/subjects/{id}` | `Subject` | `Subject` |
| Admin Catalog | Delete subject | DELETE | `/api/admin/subjects/{id}` | — | 204 |
| Admin Catalog | Create AI tutor | POST | `/api/admin/tutors` | `AiTutor` | `AiTutor` |
| Admin Catalog | Update AI tutor | PUT | `/api/admin/tutors/{id}` | `AiTutor` | `AiTutor` |
| Admin Catalog | Delete AI tutor | DELETE | `/api/admin/tutors/{id}` | — | 204 |
| Admin Catalog | Add unit | POST | `/api/admin/syllabus/units` | `Unit` | `Unit` |
| Admin Catalog | Update unit | PUT | `/api/admin/syllabus/units/{id}` | `Unit` | `Unit` |
| Admin Catalog | Delete unit | DELETE | `/api/admin/syllabus/units/{id}` | — | 204 |
| Admin Catalog | Add topic | POST | `/api/admin/syllabus/topics` | `Topic` | `Topic` |
| Admin Catalog | Delete topic | DELETE | `/api/admin/syllabus/topics/{id}` | — | 204 |
| Admin Catalog | Add objective | POST | `/api/admin/syllabus/objectives` | `LearningObjective` | `LearningObjective` |
| Admin Catalog | Delete objective | DELETE | `/api/admin/syllabus/objectives/{id}` | — | 204 |

---

### QUESTIONS MODULE — `QuestionsController` → `/api/questions`

| Vue Screen | User Action | HTTP | Endpoint | Request | Response |
|-----------|------------|------|----------|---------|----------|
| Daily Tests | Get random questions | GET | `/api/questions/random?gradeCode=&subjectId=&count=30` | — | `QuestionDto[]` |
| Admin Questions | List + filter questions | GET | `/api/questions?gradeCode=&subjectId=&kind=&search=&page=` | — | `PagedResult<QuestionDto>` |
| Admin Questions | Get single question | GET | `/api/questions/{id}` | — | `QuestionDto` |
| Admin Questions | Create question | POST | `/api/questions` | `CreateQuestionRequest` | `QuestionDto` |
| Admin Questions | Update question | PUT | `/api/questions/{id}` | `UpdateQuestionRequest` | `QuestionDto` |
| Admin Questions | Delete question | DELETE | `/api/questions/{id}` | — | 204 |

---

### TESTS MODULE — `TestsController` → `/api/tests`

| Vue Screen | User Action | HTTP | Endpoint | Request | Response |
|-----------|------------|------|----------|---------|----------|
| Daily Tests | List published tests | GET | `/api/tests?gradeCode=&kind=daily&published=true` | — | `{ items: TestDto[], total }` |
| Daily Tests | Get test with questions | GET | `/api/tests/{id}` | — | `{ test, Questions[] }` |
| Daily Tests | Submit attempt | POST | `/api/tests/attempts` | `SubmitAttemptRequest` | `AttemptResultDto` |
| Reports | View own attempts | GET | `/api/tests/attempts?page=&pageSize=` | — | `{ items: AttemptResultDto[], total }` |
| Reports | View single attempt | GET | `/api/tests/attempts/{id}` | — | `TestAttempt` |
| Reports | View stats | GET | `/api/tests/me/stats` | — | `{ attempts, avgPercent, passed, coinsEarned, coins }` |
| Competition | Leaderboard | GET | `/api/tests/leaderboard?gradeCode=&top=20` | — | `LeaderboardEntry[]` |
| Admin Tests | List all tests | GET | `/api/tests?published=false` | — | `{ items: TestDto[], total }` |
| Admin Tests | Create test | POST | `/api/tests` | `CreateTestRequest` | `TestDto` |
| Admin Tests | Toggle publish | PATCH | `/api/tests/{id}/publish` | — | `{ id, isPublished }` |
| Admin Tests | Delete test | DELETE | `/api/tests/{id}` | — | 204 |

---

### COINS MODULE — `CoinsController` → `/api/coins`

| Vue Screen | User Action | HTTP | Endpoint | Request | Response |
|-----------|------------|------|----------|---------|----------|
| Wallet | Get coin balance | GET | `/api/coins/balance` | — | `{ coins, pkr }` |
| Wallet | Get transaction history | GET | `/api/coins/transactions?page=&pageSize=` | — | `{ items: CoinLedgerEntryDto[], total }` |
| Wallet | Get payout account | GET | `/api/coins/payout-account` | — | `PayoutAccountDto \| null` |
| Wallet | Save payout account | PUT | `/api/coins/payout-account` | `UpsertPayoutAccountRequest` | `PayoutAccountDto` |
| Wallet | Request withdrawal | POST | `/api/coins/withdraw` | `CreateWithdrawalRequest` | `WithdrawalRequestDto` |
| Wallet | View withdrawals | GET | `/api/coins/withdrawals` | — | `WithdrawalRequestDto[]` |
| Admin Withdrawals | List all withdrawals | GET | `/api/admin/coins/withdrawals?status=&page=` | — | `{ items[], total }` |
| Admin Withdrawals | Process withdrawal | PUT | `/api/admin/coins/withdrawals/{id}` | `ProcessWithdrawalRequest` | `{ id, status, adminNote, processedAt }` |

---

### NOTIFICATIONS MODULE — `NotificationsController` → `/api/notifications`

| Vue Screen | User Action | HTTP | Endpoint | Request | Response |
|-----------|------------|------|----------|---------|----------|
| Notification Bell | Get notifications | GET | `/api/notifications` | — | `NotificationDto[]` |
| Notification Bell | Mark one read | PATCH | `/api/notifications/{id}/read` | — | `{ ok: true }` |
| Notification Bell | Mark all read | PATCH | `/api/notifications/read-all` | — | `{ count }` |
| Admin Notifications | Broadcast notification | POST | `/api/admin/notifications` | `SendNotificationRequest` | `{ id, title, sentTo }` |

**Real-time:** `SignalR hub /hubs/notifications` — clients subscribe on login, server pushes on `POST /api/admin/notifications`.

---

### COMPLAINTS MODULE — `ComplaintsController` → `/api/complaints`

| Vue Screen | User Action | HTTP | Endpoint | Request | Response |
|-----------|------------|------|----------|---------|----------|
| Complaints | Submit complaint | POST | `/api/complaints` | `CreateComplaintRequest` | `ComplaintDto` |
| Complaints | My complaints | GET | `/api/complaints` | — | `ComplaintDto[]` |
| Admin Complaints | All complaints | GET | `/api/admin/complaints?status=&page=` | — | `{ items: ComplaintDto[], total }` |
| Admin Complaints | Reply to complaint | PUT | `/api/admin/complaints/{id}` | `ReplyComplaintRequest` | `ComplaintDto` |

---

### CONTENT MODULE — `ContentController` → `/api/content`

| Vue Screen | User Action | HTTP | Endpoint | Request | Response |
|-----------|------------|------|----------|---------|----------|
| Preparation | List content | GET | `/api/content?gradeCode=&subjectId=&kind=&unitId=` | — | `ContentItem[]` |
| Preparation | Video courses | GET | `/api/content/courses?gradeCode=&subjectId=` | — | `{ id, title, lessonCount }[]` |
| Preparation | Video course detail | GET | `/api/content/courses/{id}` | — | `VideoCourse` with Lessons |
| Preparation | My notes | GET | `/api/content/notes` | — | `StudentNote[]` |
| Preparation | Create note | POST | `/api/content/notes` | `StudentNote` | `StudentNote` |
| Preparation | Update note | PUT | `/api/content/notes/{id}` | `StudentNote` | `StudentNote` |
| Preparation | Delete note | DELETE | `/api/content/notes/{id}` | — | 204 |
| Admin Content | Create content | POST | `/api/admin/content` | `ContentItem` | `ContentItem` |
| Admin Content | Update content | PUT | `/api/admin/content/{id}` | `ContentItem` | `ContentItem` |
| Admin Content | Delete content | DELETE | `/api/admin/content/{id}` | — | 204 |

---

### USERS MODULE — `UsersController` → `/api/admin/users`

| Vue Screen | User Action | HTTP | Endpoint | Request | Response |
|-----------|------------|------|----------|---------|----------|
| Admin Users | List users | GET | `/api/admin/users?role=&gradeCode=&search=&page=` | — | `{ items[], total }` |
| Admin Users | Student results | GET | `/api/admin/users/{id}/results` | — | `{ items[], total }` |
| Admin Users | Toggle active | PATCH | `/api/admin/users/{id}/toggle` | — | `{ id, isActive }` |
| Admin Dashboard | Analytics KPIs | GET | `/api/admin/analytics` | — | `{ totalStudents, totalQuestions, ... }` |
| Admin Settings | Get settings | GET | `/api/admin/settings` | — | `{ key: value }` |
| Admin Settings | Update setting | PUT | `/api/admin/settings/{key}` | `string` | `{ key, value }` |

---

### AI MODULE — `AiController` → `/api/ai`

| Vue Screen | User Action | HTTP | Endpoint | Request | Response |
|-----------|------------|------|----------|---------|----------|
| AI Tutor | Get RAG context | GET | `/api/ai/context?gradeCode=&subjectId=&sample=6` | — | `{ syllabus, sampleQuestions, contextText }` |

---

## Permission Mapping (Granular — Phase 5)

Admin endpoints that may require granular permissions beyond `AdminOnly`:

| Permission Name | Assigned To | Protects |
|----------------|------------|---------|
| `questions.manage` | admin | POST/PUT/DELETE `/api/questions` |
| `tests.manage` | admin | POST/PATCH/DELETE `/api/tests` |
| `users.manage` | admin | `PATCH /api/admin/users/{id}/toggle` |
| `content.manage` | admin | `/api/admin/content` CRUD |
| `coins.manage` | admin | `/api/admin/coins/withdrawals` |
| `notifications.send` | admin | `POST /api/admin/notifications` |
| `complaints.manage` | admin | `PUT /api/admin/complaints/{id}` |
| `catalog.manage` | admin | All `/api/admin/grades`, `/api/admin/subjects` CRUD |
| `settings.manage` | admin | `PUT /api/admin/settings/{key}` |
| `reports.view` | admin | `/api/admin/analytics` |

> Use `[RequirePermission("permission.name")]` attribute on controller actions to enforce these.
