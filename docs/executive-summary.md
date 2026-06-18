# Executive Summary — Balochistan Academy Portal

## System Purpose

The Balochistan Academy Portal is a web-based digital learning platform designed to provide students across Balochistan with access to quality education resources. It delivers curriculum-aligned content, AI-assisted tutoring, self-assessment tools, and a gamified reward system to drive student engagement and academic performance improvement.

---

## Business Objectives

| # | Objective | Metric |
|---|-----------|--------|
| 1 | Provide accessible digital education to students in Grades 1–12 | Registered student count |
| 2 | Deliver self-paced preparation for board exams and entrance tests | Test attempts & pass rates |
| 3 | Incentivize consistent study behavior through a coin reward system | Coins earned, withdrawal requests processed |
| 4 | Enable AI-assisted personalized learning | AI tutor session count |
| 5 | Maintain academic content quality through admin content management | Content items published, question bank size |
| 6 | Support multiple language mediums (English / Urdu) | User-medium distribution |
| 7 | Build trust through transparent complaint handling | Complaint resolution rate |

---

## Users / Actors

### Students
- Primary users of the platform
- Access curriculum, take tests, earn coins, interact with AI tutor
- Submit complaints and track their academic progress

### Administrators
- Manage the grade/subject/syllabus catalog
- Create and publish tests and questions
- Broadcast notifications
- Handle withdrawal requests
- Manage users and review analytics
- Configure system settings

### System (Automated)
- Awards coins on qualifying test completions
- Delivers targeted notifications via SignalR
- Logs all entity changes for audit compliance

---

## Key Business Rules

1. **Coin System** — Students earn coins for completing tests (≥35 questions). Rates: 90%+ = 50 coins, 70%+ = 30 coins, 50%+ = 15 coins, else 5 coins. Minimum 300 coins required to request withdrawal.

2. **Coin Value** — 1 coin = PKR 0.50. Withdrawable via JazzCash, EasyPaisa, or Bank Transfer.

3. **Authentication** — JWT access tokens (60-minute expiry) + rotating refresh tokens (30-day expiry). Bcrypt password hashing.

4. **Role-Based Access** — Two primary roles: `student` and `admin`. Granular permission table for fine-grained admin access control.

5. **Content Access** — Students must select a grade during registration. Curriculum content, questions, and tests are filtered by grade and subject.

6. **Test Integrity** — Coins are only awarded once per test per student (deduplication by TestId and UserId).

7. **Audit Trail** — All admin create/update/delete actions are logged to the AuditLog table with old/new values and user IP.

---

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Frontend | Vue 3, Vite, Tailwind CSS, PrimeVue, Pinia |
| Backend API | .NET 10 Web API (Clean Architecture) |
| Database | Microsoft SQL Server |
| Authentication | JWT Bearer + Refresh Tokens |
| Real-time | SignalR |
| AI Integration | Google Gemini 2.5 Flash Lite (browser-side) |
| ORM | Entity Framework Core 9 |
| Logging | Serilog (Console + SQL Server sink) |
| Mapping | AutoMapper 13 |
| Validation | FluentValidation 11 |
