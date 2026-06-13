import { Router } from 'express'
import { z } from 'zod'
import { existsSync, readFileSync } from 'node:fs'
import { fileURLToPath } from 'node:url'
import { dirname, resolve, isAbsolute } from 'node:path'
import db from '../db/connection.js'
import { parse } from '../util/validate.js'
import { requireAdmin } from '../middleware/auth.js'

const router = Router()
router.use(requireAdmin)

const __dirname = dirname(fileURLToPath(import.meta.url))
const serverRoot = resolve(__dirname, '..', '..')

// ── dashboard counts ─────────────────────────────────────────────────
router.get('/stats', (_req, res) => {
  const one = (sql) => db.prepare(sql).get().c
  res.json({
    students: one("SELECT COUNT(*) c FROM users WHERE role='student'"),
    grades: one('SELECT COUNT(*) c FROM grades WHERE enabled=1'),
    subjects: one('SELECT COUNT(*) c FROM subjects'),
    tutors: one('SELECT COUNT(*) c FROM ai_tutors WHERE active=1'),
    questions: one('SELECT COUNT(*) c FROM questions'),
    units: one('SELECT COUNT(*) c FROM units'),
    tests: one('SELECT COUNT(*) c FROM tests'),
    attempts: one('SELECT COUNT(*) c FROM test_attempts'),
  })
})

// ── GRADES ───────────────────────────────────────────────────────────
router.get('/grades', (_req, res) => res.json(db.prepare('SELECT * FROM grades ORDER BY sort_order').all()))
const gradeSchema = z.object({ code: z.string().min(1), label: z.string().min(1), band: z.string().min(1), sort_order: z.number().optional(), enabled: z.number().optional() })
router.post('/grades', (req, res) => {
  const g = parse(gradeSchema, req.body)
  db.prepare('INSERT INTO grades (code,label,band,sort_order,enabled) VALUES (?,?,?,?,?) ON CONFLICT(code) DO UPDATE SET label=excluded.label, band=excluded.band, sort_order=excluded.sort_order, enabled=excluded.enabled')
    .run(g.code, g.label, g.band, g.sort_order ?? 0, g.enabled ?? 1)
  res.json(db.prepare('SELECT * FROM grades WHERE code=?').get(g.code))
})
router.put('/grades/:code', (req, res) => {
  const g = parse(gradeSchema.partial(), req.body)
  db.prepare('UPDATE grades SET label=COALESCE(?,label), band=COALESCE(?,band), sort_order=COALESCE(?,sort_order), enabled=COALESCE(?,enabled) WHERE code=?')
    .run(g.label ?? null, g.band ?? null, g.sort_order ?? null, g.enabled ?? null, req.params.code)
  res.json(db.prepare('SELECT * FROM grades WHERE code=?').get(req.params.code))
})
router.delete('/grades/:code', (req, res) => { db.prepare('DELETE FROM grades WHERE code=?').run(req.params.code); res.json({ ok: true }) })

// set which subjects belong to a grade
router.put('/grades/:code/subjects', (req, res) => {
  const ids = parse(z.object({ subjectIds: z.array(z.number()) }), req.body).subjectIds
  db.transaction(() => {
    db.prepare('DELETE FROM grade_subjects WHERE grade_code=?').run(req.params.code)
    const ins = db.prepare('INSERT OR IGNORE INTO grade_subjects (grade_code,subject_id) VALUES (?,?)')
    ids.forEach((id) => ins.run(req.params.code, id))
  })()
  res.json({ ok: true, count: ids.length })
})

// ── SUBJECTS ─────────────────────────────────────────────────────────
router.get('/subjects', (_req, res) => res.json(db.prepare('SELECT * FROM subjects ORDER BY name').all()))
const subjectSchema = z.object({ name: z.string().min(1), name_ur: z.string().optional(), icon: z.string().optional(), color: z.string().optional(), medium: z.string().optional() })
router.post('/subjects', (req, res) => {
  const s = parse(subjectSchema, req.body)
  const info = db.prepare('INSERT INTO subjects (name,name_ur,icon,color,medium) VALUES (?,?,?,?,?) ON CONFLICT(name) DO UPDATE SET name_ur=excluded.name_ur, icon=excluded.icon, color=excluded.color, medium=excluded.medium')
    .run(s.name, s.name_ur ?? null, s.icon ?? null, s.color ?? null, s.medium ?? 'english')
  res.json(db.prepare('SELECT * FROM subjects WHERE id=? OR name=?').get(info.lastInsertRowid, s.name))
})
router.put('/subjects/:id', (req, res) => {
  const s = parse(subjectSchema.partial(), req.body)
  db.prepare('UPDATE subjects SET name=COALESCE(?,name), name_ur=COALESCE(?,name_ur), icon=COALESCE(?,icon), color=COALESCE(?,color), medium=COALESCE(?,medium) WHERE id=?')
    .run(s.name ?? null, s.name_ur ?? null, s.icon ?? null, s.color ?? null, s.medium ?? null, req.params.id)
  res.json(db.prepare('SELECT * FROM subjects WHERE id=?').get(req.params.id))
})
router.delete('/subjects/:id', (req, res) => { db.prepare('DELETE FROM subjects WHERE id=?').run(req.params.id); res.json({ ok: true }) })

// ── SYLLABUS (units / topics / objectives) ───────────────────────────
const unitSchema = z.object({ grade_code: z.string(), subject_id: z.number(), name: z.string().min(1), number: z.number().optional(), sort_order: z.number().optional(), description: z.string().optional() })
router.post('/syllabus/units', (req, res) => {
  const u = parse(unitSchema, req.body)
  const info = db.prepare('INSERT INTO units (grade_code,subject_id,name,number,sort_order,description,source) VALUES (?,?,?,?,?,?,?)')
    .run(u.grade_code, u.subject_id, u.name, u.number ?? null, u.sort_order ?? 0, u.description ?? '', 'admin')
  res.status(201).json(db.prepare('SELECT * FROM units WHERE id=?').get(info.lastInsertRowid))
})
router.put('/syllabus/units/:id', (req, res) => {
  const u = parse(unitSchema.partial(), req.body)
  db.prepare('UPDATE units SET name=COALESCE(?,name), number=COALESCE(?,number), sort_order=COALESCE(?,sort_order), description=COALESCE(?,description) WHERE id=?')
    .run(u.name ?? null, u.number ?? null, u.sort_order ?? null, u.description ?? null, req.params.id)
  res.json(db.prepare('SELECT * FROM units WHERE id=?').get(req.params.id))
})
router.delete('/syllabus/units/:id', (req, res) => { db.prepare('DELETE FROM units WHERE id=?').run(req.params.id); res.json({ ok: true }) })

router.post('/syllabus/topics', (req, res) => {
  const t = parse(z.object({ unit_id: z.number(), name: z.string().min(1), sort_order: z.number().optional() }), req.body)
  const info = db.prepare('INSERT INTO topics (unit_id,name,sort_order) VALUES (?,?,?)').run(t.unit_id, t.name, t.sort_order ?? 0)
  res.status(201).json(db.prepare('SELECT * FROM topics WHERE id=?').get(info.lastInsertRowid))
})
router.delete('/syllabus/topics/:id', (req, res) => { db.prepare('DELETE FROM topics WHERE id=?').run(req.params.id); res.json({ ok: true }) })

router.post('/syllabus/objectives', (req, res) => {
  const o = parse(z.object({ unit_id: z.number().optional(), topic_id: z.number().optional(), code: z.string().optional(), text: z.string().min(1), cognitive_level: z.string().optional() }), req.body)
  const info = db.prepare('INSERT INTO learning_objectives (unit_id,topic_id,code,text,cognitive_level,source) VALUES (?,?,?,?,?,?)')
    .run(o.unit_id ?? null, o.topic_id ?? null, o.code ?? null, o.text, o.cognitive_level ?? null, 'admin')
  res.status(201).json(db.prepare('SELECT * FROM learning_objectives WHERE id=?').get(info.lastInsertRowid))
})
router.delete('/syllabus/objectives/:id', (req, res) => { db.prepare('DELETE FROM learning_objectives WHERE id=?').run(req.params.id); res.json({ ok: true }) })

// ── AI TUTORS ────────────────────────────────────────────────────────
router.get('/tutors', (_req, res) => res.json(db.prepare('SELECT t.*, s.name AS subject_name FROM ai_tutors t LEFT JOIN subjects s ON s.id=t.subject_id ORDER BY t.id').all()))
const tutorSchema = z.object({ persona: z.string().min(1), slug: z.string().min(1), subject_id: z.number().nullable().optional(), grade_code: z.string().nullable().optional(), icon: z.string().optional(), color: z.string().optional(), description: z.string().optional(), system_prompt: z.string().optional(), active: z.number().optional() })
router.post('/tutors', (req, res) => {
  const t = parse(tutorSchema, req.body)
  const info = db.prepare('INSERT INTO ai_tutors (persona,slug,subject_id,grade_code,icon,color,description,system_prompt,active) VALUES (?,?,?,?,?,?,?,?,?) ON CONFLICT(slug) DO UPDATE SET persona=excluded.persona, subject_id=excluded.subject_id, grade_code=excluded.grade_code, icon=excluded.icon, color=excluded.color, description=excluded.description, system_prompt=excluded.system_prompt, active=excluded.active')
    .run(t.persona, t.slug, t.subject_id ?? null, t.grade_code ?? null, t.icon ?? null, t.color ?? null, t.description ?? null, t.system_prompt ?? null, t.active ?? 1)
  res.status(201).json(db.prepare('SELECT * FROM ai_tutors WHERE id=? OR slug=?').get(info.lastInsertRowid, t.slug))
})
router.put('/tutors/:id', (req, res) => {
  const t = parse(tutorSchema.partial(), req.body)
  db.prepare('UPDATE ai_tutors SET persona=COALESCE(?,persona), subject_id=COALESCE(?,subject_id), grade_code=?, icon=COALESCE(?,icon), color=COALESCE(?,color), description=COALESCE(?,description), system_prompt=COALESCE(?,system_prompt), active=COALESCE(?,active) WHERE id=?')
    .run(t.persona ?? null, t.subject_id ?? null, t.grade_code ?? null, t.icon ?? null, t.color ?? null, t.description ?? null, t.system_prompt ?? null, t.active ?? null, req.params.id)
  res.json(db.prepare('SELECT * FROM ai_tutors WHERE id=?').get(req.params.id))
})
router.delete('/tutors/:id', (req, res) => { db.prepare('DELETE FROM ai_tutors WHERE id=?').run(req.params.id); res.json({ ok: true }) })

// ── QUESTIONS (admin-authored + browse ingested) ─────────────────────
router.get('/questions', (req, res) => {
  const { grade, subject, search, limit = 50 } = req.query
  let sql = 'SELECT q.*, s.name AS subject_name FROM questions q LEFT JOIN subjects s ON s.id=q.subject_id WHERE 1=1'
  const args = []
  if (grade) { sql += ' AND q.grade_code=?'; args.push(grade) }
  if (subject) { sql += ' AND s.name=?'; args.push(subject) }
  if (search) { sql += ' AND q.stem LIKE ?'; args.push(`%${search}%`) }
  sql += ' ORDER BY q.id DESC LIMIT ?'
  args.push(Math.min(Number(limit), 200))
  res.json(db.prepare(sql).all(...args))
})

// ── STUDENTS + results ───────────────────────────────────────────────
router.get('/students', (req, res) => {
  const { grade } = req.query
  let sql = "SELECT id,username,name,phone,email,grade_code,medium,institute,coins,active,created_at,last_login_at FROM users WHERE role='student'"
  const args = []
  if (grade) { sql += ' AND grade_code=?'; args.push(grade) }
  sql += ' ORDER BY created_at DESC'
  res.json(db.prepare(sql).all(...args))
})
router.get('/students/:id/results', (req, res) => {
  res.json(db.prepare('SELECT * FROM test_attempts WHERE user_id=? ORDER BY submitted_at DESC').all(req.params.id))
})
router.put('/students/:id', (req, res) => {
  const b = parse(z.object({ grade_code: z.string().optional(), active: z.number().optional(), coins: z.number().optional() }), req.body)
  db.prepare('UPDATE users SET grade_code=COALESCE(?,grade_code), active=COALESCE(?,active), coins=COALESCE(?,coins) WHERE id=? AND role=\'student\'')
    .run(b.grade_code ?? null, b.active ?? null, b.coins ?? null, req.params.id)
  res.json({ ok: true })
})

// ── TESTS ────────────────────────────────────────────────────────────
router.get('/tests', (_req, res) => res.json(db.prepare('SELECT t.*, s.name AS subject_name FROM tests t LEFT JOIN subjects s ON s.id=t.subject_id ORDER BY t.created_at DESC').all()))
const testSchema = z.object({ title: z.string().min(1), grade_code: z.string(), subject_id: z.number().nullable().optional(), unit_id: z.number().nullable().optional(), kind: z.string().optional(), duration_min: z.number().optional(), is_published: z.number().optional(), questionIds: z.array(z.number()).optional() })
router.post('/tests', (req, res) => {
  const t = parse(testSchema, req.body)
  const info = db.prepare('INSERT INTO tests (title,grade_code,subject_id,unit_id,kind,duration_min,is_published,created_by) VALUES (?,?,?,?,?,?,?,?)')
    .run(t.title, t.grade_code, t.subject_id ?? null, t.unit_id ?? null, t.kind ?? 'objective', t.duration_min ?? 30, t.is_published ?? 0, req.user.id)
  const id = info.lastInsertRowid
  if (t.questionIds?.length) {
    const ins = db.prepare('INSERT OR IGNORE INTO test_questions (test_id,question_id,sort_order) VALUES (?,?,?)')
    t.questionIds.forEach((qid, i) => ins.run(id, qid, i))
  }
  res.status(201).json(db.prepare('SELECT * FROM tests WHERE id=?').get(id))
})
router.put('/tests/:id', (req, res) => {
  const t = parse(testSchema.partial(), req.body)
  db.prepare('UPDATE tests SET title=COALESCE(?,title), is_published=COALESCE(?,is_published), duration_min=COALESCE(?,duration_min) WHERE id=?')
    .run(t.title ?? null, t.is_published ?? null, t.duration_min ?? null, req.params.id)
  res.json(db.prepare('SELECT * FROM tests WHERE id=?').get(req.params.id))
})
router.delete('/tests/:id', (req, res) => { db.prepare('DELETE FROM tests WHERE id=?').run(req.params.id); res.json({ ok: true }) })

// ── ingestion skipped log ────────────────────────────────────────────
router.get('/ingestion/skipped', (_req, res) => {
  const dir = isAbsolute(process.env.INGEST_DIR || '') ? process.env.INGEST_DIR : resolve(serverRoot, process.env.INGEST_DIR || '../scripts/ingest/out')
  const f = resolve(dir, '_skipped.json')
  if (!existsSync(f)) return res.json({ skipped: [], note: 'no ingestion log yet' })
  res.json(JSON.parse(readFileSync(f, 'utf-8')))
})

export default router
