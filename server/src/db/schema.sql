-- eStudy schema — idempotent. Run by migrate.js.
PRAGMA journal_mode = WAL;
PRAGMA foreign_keys = ON;

-- ── USERS ───────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS users (
  id            INTEGER PRIMARY KEY AUTOINCREMENT,
  username      TEXT UNIQUE NOT NULL,
  password_hash TEXT NOT NULL,
  role          TEXT NOT NULL CHECK (role IN ('admin','student')),
  name          TEXT,
  phone         TEXT,
  email         TEXT,
  grade_code    TEXT REFERENCES grades(code),
  medium        TEXT DEFAULT 'English',
  institute     TEXT,
  board         TEXT DEFAULT 'Balochistan',
  coins         INTEGER NOT NULL DEFAULT 0,
  active        INTEGER NOT NULL DEFAULT 1,
  created_at    TEXT NOT NULL DEFAULT (datetime('now')),
  last_login_at TEXT
);

CREATE TABLE IF NOT EXISTS login_history (
  id      INTEGER PRIMARY KEY AUTOINCREMENT,
  user_id INTEGER REFERENCES users(id) ON DELETE CASCADE,
  ts      TEXT NOT NULL DEFAULT (datetime('now')),
  status  TEXT
);

-- ── CURRICULUM ──────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS grades (
  code       TEXT PRIMARY KEY,          -- 'ECD','1'..'12'
  label      TEXT NOT NULL,             -- 'ECD','Grade 1'..
  band       TEXT NOT NULL,             -- 'ECD-5','6-8','9-12'
  sort_order INTEGER NOT NULL DEFAULT 0,
  enabled    INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS subjects (
  id      INTEGER PRIMARY KEY AUTOINCREMENT,
  name    TEXT NOT NULL UNIQUE,         -- canonical, e.g. 'Mathematics'
  name_ur TEXT,
  icon    TEXT,
  color   TEXT,
  medium  TEXT DEFAULT 'english'
);

CREATE TABLE IF NOT EXISTS grade_subjects (
  grade_code TEXT NOT NULL REFERENCES grades(code) ON DELETE CASCADE,
  subject_id INTEGER NOT NULL REFERENCES subjects(id) ON DELETE CASCADE,
  PRIMARY KEY (grade_code, subject_id)
);

-- ── SYLLABUS: units -> topics -> objectives (from PDF manuals) ───────
CREATE TABLE IF NOT EXISTS units (
  id          INTEGER PRIMARY KEY AUTOINCREMENT,
  grade_code  TEXT NOT NULL REFERENCES grades(code) ON DELETE CASCADE,
  subject_id  INTEGER NOT NULL REFERENCES subjects(id) ON DELETE CASCADE,
  name        TEXT NOT NULL,
  number      INTEGER,
  sort_order  INTEGER NOT NULL DEFAULT 0,
  description TEXT,
  source      TEXT
);
CREATE INDEX IF NOT EXISTS idx_units_gs ON units(grade_code, subject_id);

CREATE TABLE IF NOT EXISTS topics (
  id         INTEGER PRIMARY KEY AUTOINCREMENT,
  unit_id    INTEGER NOT NULL REFERENCES units(id) ON DELETE CASCADE,
  name       TEXT NOT NULL,
  sort_order INTEGER NOT NULL DEFAULT 0
);
CREATE INDEX IF NOT EXISTS idx_topics_unit ON topics(unit_id);

CREATE TABLE IF NOT EXISTS learning_objectives (
  id              INTEGER PRIMARY KEY AUTOINCREMENT,
  unit_id         INTEGER REFERENCES units(id) ON DELETE CASCADE,
  topic_id        INTEGER REFERENCES topics(id) ON DELETE CASCADE,
  code            TEXT,
  text            TEXT NOT NULL,
  cognitive_level TEXT,
  source          TEXT,
  ai_structured   INTEGER NOT NULL DEFAULT 0
);
CREATE INDEX IF NOT EXISTS idx_lo_unit ON learning_objectives(unit_id);

-- ── AI TUTORS ───────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS ai_tutors (
  id            INTEGER PRIMARY KEY AUTOINCREMENT,
  subject_id    INTEGER REFERENCES subjects(id) ON DELETE SET NULL,
  grade_code    TEXT REFERENCES grades(code) ON DELETE SET NULL,  -- null = all grades
  persona       TEXT NOT NULL,
  slug          TEXT NOT NULL UNIQUE,
  icon          TEXT,
  color         TEXT,
  description   TEXT,
  system_prompt TEXT,
  active        INTEGER NOT NULL DEFAULT 1
);

-- ── QUESTION BANK ───────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS questions (
  id              INTEGER PRIMARY KEY AUTOINCREMENT,
  grade_code      TEXT REFERENCES grades(code),
  subject_id      INTEGER REFERENCES subjects(id),
  unit_id         INTEGER REFERENCES units(id) ON DELETE SET NULL,
  topic_id        INTEGER REFERENCES topics(id) ON DELETE SET NULL,
  kind            TEXT NOT NULL CHECK (kind IN ('objective','subjective')),
  stem            TEXT NOT NULL,
  options_json    TEXT,            -- JSON array of strings (objective)
  correct_index   INTEGER,         -- 0-based (objective)
  q_type          TEXT,            -- 'Short'|'Long' (subjective)
  marks           INTEGER,
  model_answer    TEXT,
  topic_text      TEXT,            -- raw topic string from source
  skill           TEXT,
  difficulty      TEXT,
  cognitive_level TEXT,
  feedback        TEXT,
  article         TEXT,
  source_paper    TEXT,
  source_format   TEXT,            -- 'xlsx'|'csv'|'xls'|'docx'|'admin'
  ai_generated    INTEGER NOT NULL DEFAULT 0,
  content_hash    TEXT UNIQUE,
  created_at      TEXT NOT NULL DEFAULT (datetime('now'))
);
CREATE INDEX IF NOT EXISTS idx_q_gsu ON questions(grade_code, subject_id, unit_id);
CREATE INDEX IF NOT EXISTS idx_q_kind ON questions(kind);

-- ── TESTS / ATTEMPTS ────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS tests (
  id           INTEGER PRIMARY KEY AUTOINCREMENT,
  title        TEXT NOT NULL,
  grade_code   TEXT REFERENCES grades(code) ON DELETE CASCADE,
  subject_id   INTEGER REFERENCES subjects(id) ON DELETE SET NULL,
  unit_id      INTEGER REFERENCES units(id) ON DELETE SET NULL,
  kind         TEXT NOT NULL DEFAULT 'objective',
  duration_min INTEGER DEFAULT 30,
  total_marks  INTEGER,
  is_published INTEGER NOT NULL DEFAULT 0,
  created_by   INTEGER REFERENCES users(id) ON DELETE SET NULL,
  created_at   TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS test_questions (
  test_id     INTEGER NOT NULL REFERENCES tests(id) ON DELETE CASCADE,
  question_id INTEGER NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
  sort_order  INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY (test_id, question_id)
);

CREATE TABLE IF NOT EXISTS test_attempts (
  id           INTEGER PRIMARY KEY AUTOINCREMENT,
  user_id      INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  test_id      INTEGER REFERENCES tests(id) ON DELETE SET NULL,
  grade_code   TEXT,
  subject_id   INTEGER,
  score        INTEGER,
  total        INTEGER,
  percent      REAL,
  duration_sec INTEGER,
  answers_json TEXT,
  started_at   TEXT,
  submitted_at TEXT NOT NULL DEFAULT (datetime('now'))
);
CREATE INDEX IF NOT EXISTS idx_attempts_user ON test_attempts(user_id);
CREATE INDEX IF NOT EXISTS idx_attempts_grade ON test_attempts(grade_code);

CREATE TABLE IF NOT EXISTS coin_ledger (
  id      INTEGER PRIMARY KEY AUTOINCREMENT,
  user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  amount  INTEGER NOT NULL,
  reason  TEXT,
  ts      TEXT NOT NULL DEFAULT (datetime('now'))
);

-- ── STUDY SCHEDULE (AI/adaptive study planner — spec §3–5, §9) ───────
-- Admin-set exam dates per grade+subject (spec §1.1)
CREATE TABLE IF NOT EXISTS exam_dates (
  id          INTEGER PRIMARY KEY AUTOINCREMENT,
  grade_code  TEXT NOT NULL REFERENCES grades(code) ON DELETE CASCADE,
  subject_id  INTEGER NOT NULL REFERENCES subjects(id) ON DELETE CASCADE,
  exam_date   TEXT NOT NULL,                  -- ISO date
  paper_type  TEXT DEFAULT 'combined',        -- objective|subjective|combined
  total_marks INTEGER DEFAULT 100,
  updated_at  TEXT NOT NULL DEFAULT (datetime('now')),
  UNIQUE (grade_code, subject_id)
);

-- One generated plan per student per subject
CREATE TABLE IF NOT EXISTS student_schedules (
  id          INTEGER PRIMARY KEY AUTOINCREMENT,
  user_id     INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  subject_id  INTEGER NOT NULL REFERENCES subjects(id) ON DELETE CASCADE,
  grade_code  TEXT,
  exam_date   TEXT NOT NULL,
  daily_hours REAL NOT NULL DEFAULT 2,
  created_at  TEXT NOT NULL DEFAULT (datetime('now')),
  updated_at  TEXT NOT NULL DEFAULT (datetime('now')),
  UNIQUE (user_id, subject_id)
);

-- Per-day plan rows (spec §4.2 schedule entry)
CREATE TABLE IF NOT EXISTS schedule_days (
  id          INTEGER PRIMARY KEY AUTOINCREMENT,
  schedule_id INTEGER NOT NULL REFERENCES student_schedules(id) ON DELETE CASCADE,
  day_date    TEXT NOT NULL,                  -- ISO date
  unit_id     INTEGER REFERENCES units(id) ON DELETE SET NULL,
  unit_title  TEXT,
  task        TEXT,
  mode        TEXT NOT NULL DEFAULT 'study',  -- study|revision
  est_minutes INTEGER NOT NULL DEFAULT 90,
  self_test   INTEGER NOT NULL DEFAULT 1,
  status      TEXT NOT NULL DEFAULT 'pending',-- pending|done|missed|needs_review
  sort_order  INTEGER NOT NULL DEFAULT 0
);
CREATE INDEX IF NOT EXISTS idx_sched_days ON schedule_days(schedule_id);

-- ── derived leaderboard ─────────────────────────────────────────────
DROP VIEW IF EXISTS leaderboard;
CREATE VIEW leaderboard AS
  SELECT u.id AS user_id, u.name, u.grade_code, u.coins,
         COUNT(a.id) AS attempts,
         COALESCE(ROUND(AVG(a.percent), 1), 0) AS avg_percent
  FROM users u
  LEFT JOIN test_attempts a ON a.user_id = u.id
  WHERE u.role = 'student' AND u.active = 1
  GROUP BY u.id;
